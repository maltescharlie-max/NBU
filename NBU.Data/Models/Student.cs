namespace NBU.Data.Models;

public sealed class Student
{
    public int Id { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Career { get; set; } = string.Empty;
    public string University { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";

    public ICollection<ScholarshipApplication> Applications { get; set; } = new List<ScholarshipApplication>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
