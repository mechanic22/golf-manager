namespace GolfManager.Mobile.Services;

public class DistanceService
{
    private const double EarthRadiusMeters = 6_371_000.0;
    private const double MetersPerYard = 0.9144;

    public static double HaversineYards(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2))
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusMeters * c / MetersPerYard;
    }

    public static (double Front, double Center, double Back) GreenDistances(
        double playerLat, double playerLon,
        double greenLat, double greenLon,
        double greenRadiusYards)
    {
        var center = HaversineYards(playerLat, playerLon, greenLat, greenLon);
        var front = Math.Max(0, center - greenRadiusYards);
        var back = center + greenRadiusYards;
        return (front, center, back);
    }

    private static double ToRad(double degrees) => degrees * Math.PI / 180.0;
}
