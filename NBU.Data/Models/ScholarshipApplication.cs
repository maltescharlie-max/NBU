using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    [StringLength(300)] public string? DocumentFileName { get; set; }
    [StringLength(260)] public string? DocumentStorageName { get; set; }

    public Student Student { get; set; } = null!;
    public Scholarship Scholarship { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [NotMapped]
    public string PaymentLabel => Student is null || Scholarship is null
        ? string.Empty
        : $"{Student.FullName} · {Scholarship.Name}";
}
