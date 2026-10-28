namespace NBU.Data.Models;

public sealed class Scholarship
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Requirements { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string Status { get; set; } = "Open";

    public ICollection<ScholarshipApplication> Applications { get; set; } = new List<ScholarshipApplication>();
}
