using GolfManager.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GolfManager.Data.Seed;

public class DbSeeder
{
    private readonly GolfManagerDbContext _context;
    private readonly ILogger<DbSeeder> _logger;
    private readonly IShortIdService _shortIdService;

    public DbSeeder(GolfManagerDbContext context, ILogger<DbSeeder> logger, IShortIdService shortIdService)
    {
        _context = context;
        _logger = logger;
        _shortIdService = shortIdService;
    }

    public async Task SeedAsync()
    {
        if (await _context.Users.AnyAsync())
        {
            _logger.LogInformation("Database already contains data — skipping seed.");
            await RecalculateStatsIfNeededAsync();
            return;
        }

        var backupPath = FindBackupFile();

        if (backupPath == null)
        {
            _logger.LogWarning(
                "No DkGolf_Backup_*.sql file found. Database will remain empty. " +
                "Place a backup file in the API directory and restart.");
            return;
        }

        _logger.LogInformation("Importing from {Path}...", backupPath);

        var importer = new HolyGrailImporter(_context, _logger, _shortIdService);
        var success = await importer.ImportFromBackupAsync(backupPath);

        if (success)
        {
            _logger.LogInformation("Import complete. Default password for all users: ChangeMe123!");
            await RecalculateStatsIfNeededAsync();
        }
        else
        {
            _logger.LogError("Import failed — database may be partially populated.");
        }
    }

    private async Task RepairRoundLeagueLinkagesAsync()
    {
        var orphanCount = await _context.Rounds
            .IgnoreQueryFilters()
            .CountAsync(r => string.IsNullOrEmpty(r.LeagueId) && !string.IsNullOrEmpty(r.GolferId));

        if (orphanCount == 0) return;

        _logger.LogInformation("Repairing league linkage for {Count} rounds without LeagueId...", orphanCount);

        // Build golferId → first LeagueGolfer mapping
        var leagueGolfers = await _context.LeagueGolfers
            .IgnoreQueryFilters()
            .ToListAsync();

        var lgByGolferId = leagueGolfers
            .GroupBy(lg => lg.GolferId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var orphanRounds = await _context.Rounds
            .IgnoreQueryFilters()
            .Where(r => string.IsNullOrEmpty(r.LeagueId) && !string.IsNullOrEmpty(r.GolferId))
            .ToListAsync();

        var repaired = 0;
        foreach (var round in orphanRounds)
        {
            if (!lgByGolferId.TryGetValue(round.GolferId!, out var lg)) continue;
            round.LeagueId = lg.LeagueId;
            if (string.IsNullOrEmpty(round.LeagueGolferId))
                round.LeagueGolferId = lg.Id;
            repaired++;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Repaired {Repaired}/{Total} rounds.", repaired, orphanCount);
    }

    private async Task RepairHandicapsIfNeededAsync()
    {
        var anyWithHandicap = await _context.LeagueGolfers
            .IgnoreQueryFilters()
            .AnyAsync(lg => lg.LeagueHandicap != null);
        if (anyWithHandicap) return;

        var backupPath = FindBackupFile();
        if (backupPath == null) return;

        _logger.LogInformation("No league handicaps found — repairing from backup...");
        var sqlContent = await File.ReadAllTextAsync(backupPath);
        var importer = new HolyGrailImporter(_context, _logger, _shortIdService);
        await importer.RepairHandicapsAsync(sqlContent);
    }

    private static string? FindBackupFile()
    {
        var searchDirs = new[]
        {
            AppDomain.CurrentDomain.BaseDirectory,
            Directory.GetCurrentDirectory(),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..")),
        };

        return searchDirs
            .Where(Directory.Exists)
            .SelectMany(dir => Directory.GetFiles(dir, "DkGolf_Backup_*.sql"))
            .OrderByDescending(f => f)
            .FirstOrDefault();
    }

    private async Task RecalculateStatsIfNeededAsync()
    {
        var roundCount = await _context.Rounds.IgnoreQueryFilters().CountAsync();
        if (roundCount == 0) return;

        // Repair rounds that are missing LeagueId (caused by import-order bug where
        // ImportSeasonEventGolfersAsync ran before rounds existed in _roundMap).
        await RepairRoundLeagueLinkagesAsync();
        await RepairHandicapsIfNeededAsync();

        var anyWithStats = await _context.LeagueGolfers
            .IgnoreQueryFilters()
            .AnyAsync(lg => lg.TotalRounds > 0);

        if (anyWithStats)
        {
            _logger.LogInformation("Player stats already populated — skipping recalculation.");
            return;
        }

        _logger.LogInformation("Rounds found but player stats are zero — recalculating...");

        var roundStats = await _context.Rounds
            .IgnoreQueryFilters()
            .Where(r => r.LeagueGolferId != null && r.TotalScore != null)
            .GroupBy(r => r.LeagueGolferId!)
            .Select(g => new
            {
                LeagueGolferId = g.Key,
                Count = g.Count(),
                Average = g.Average(r => (double)r.TotalScore!.Value),
                Best = g.Min(r => r.TotalScore!.Value)
            })
            .ToDictionaryAsync(x => x.LeagueGolferId);

        var leagueGolfers = await _context.LeagueGolfers.IgnoreQueryFilters().ToListAsync();

        var updated = 0;
        foreach (var lg in leagueGolfers)
        {
            if (roundStats.TryGetValue(lg.Id, out var stats))
            {
                lg.TotalRounds = stats.Count;
                lg.AverageScore = stats.Average;
                lg.BestScore = stats.Best;
                updated++;
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Recalculated stats for {Updated} players from {Rounds} rounds.", updated, roundCount);
    }
}
