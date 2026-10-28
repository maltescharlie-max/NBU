using System.ComponentModel.DataAnnotations;

namespace NBU.Data.Models;

public sealed class ScholarshipApplication
{
    public int Id { get; set; }
    [Display(Name = "Estudiante")]
    public int StudentId { get; set; }
    [Display(Name = "Beca")]
    public int ScholarshipId { get; set; }
    [Display(Name = "Fecha de solicitud")]
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    [Required, StringLength(30)]
    [Display(Name = "Estado")]
    public string Status { get; set; } = "Pending";
    [StringLength(1000)]
    [Display(Name = "Observaciones")]
    public string? Observations { get; set; }

    public Student Student { get; set; } = null!;
    public Scholarship Scholarship { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
