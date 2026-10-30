using System.ComponentModel.DataAnnotations;

namespace NBU.Data.Models;

public sealed class Student
{
    public int Id { get; set; }
    [Required, StringLength(20)]
    [Display(Name = "Código estudiantil")]
    public string StudentCode { get; set; } = string.Empty;
    [Required, StringLength(80)]
    [Display(Name = "Nombres")]
    public string FirstName { get; set; } = string.Empty;
    [Required, StringLength(80)]
    [Display(Name = "Apellidos")]
    public string LastName { get; set; } = string.Empty;
    [Required, StringLength(30)]
    [Display(Name = "Cédula")]
    public string NationalId { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(160)]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;
    [Phone, StringLength(30)]
    [Display(Name = "Teléfono")]
    public string? Phone { get; set; }
    [Required, StringLength(120)]
    [Display(Name = "Carrera")]
    public string Career { get; set; } = string.Empty;
    [Required, StringLength(120)]
    [Display(Name = "Universidad")]
    public string University { get; set; } = string.Empty;
    [Required, StringLength(30)]
    [Display(Name = "Estado")]
    public string Status { get; set; } = "Active";
    [StringLength(500)] public string? RejectionReason { get; set; }
    [StringLength(20)] public string? ReferralCode { get; set; }
    [StringLength(20)] public string? ReferredByCode { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string FullName => $"{FirstName} {LastName} ({StudentCode})";

    public ICollection<ScholarshipApplication> Applications { get; set; } = new List<ScholarshipApplication>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
