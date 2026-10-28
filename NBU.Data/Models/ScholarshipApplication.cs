namespace NBU.Data.Models;

public sealed class ScholarshipApplication
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int ScholarshipId { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending";
    public string? Observations { get; set; }

    public Student Student { get; set; } = null!;
    public Scholarship Scholarship { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
