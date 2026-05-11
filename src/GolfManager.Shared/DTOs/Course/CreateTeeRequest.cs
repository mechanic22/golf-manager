using System.ComponentModel.DataAnnotations;

namespace GolfManager.Shared.DTOs.Course;

public class CreateTeeRequest
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(7)]
    public string HtmlColorCode { get; set; } = "#FFFFFF";

    [Range(0, 50)]
    public double RatingOut { get; set; }

    [Range(55, 155)]
    public int SlopeOut { get; set; } = 113;

    [Range(0, 50)]
    public double RatingIn { get; set; }

    [Range(55, 155)]
    public int SlopeIn { get; set; } = 113;

    public int YardsOut { get; set; }
    public int YardsIn { get; set; }

    [Range(27, 40)]
    public int ParOut { get; set; } = 36;

    [Range(27, 40)]
    public int ParIn { get; set; } = 36;
}
