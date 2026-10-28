using System.ComponentModel.DataAnnotations;

namespace NBU.Data.Models;

public sealed class Notification
{
    public int Id { get; set; }
    [Display(Name = "Estudiante destinatario")]
    public int StudentId { get; set; }
    public int? PublicationId { get; set; }
    [Required, StringLength(500)]
    [Display(Name = "Mensaje")]
    public string Message { get; set; } = string.Empty;
    [Display(Name = "Fecha")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Display(Name = "Leída")]
    public bool IsRead { get; set; }

    public Student Student { get; set; } = null!;
    public Publication? Publication { get; set; }
}
