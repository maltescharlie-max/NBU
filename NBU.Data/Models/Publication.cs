using System.ComponentModel.DataAnnotations;

namespace NBU.Data.Models;

public sealed class Publication
{
    public int Id { get; set; }
    [Required, StringLength(30)]
    [Display(Name = "Tipo de publicación")]
    public string Type { get; set; } = "News";
    [Required, StringLength(200)]
    [Display(Name = "Título")]
    public string Title { get; set; } = string.Empty;
    [Required, StringLength(5000)]
    [Display(Name = "Contenido")]
    public string Content { get; set; } = string.Empty;
    [Display(Name = "Fecha de publicación")]
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    [Display(Name = "Fecha del evento")]
    public DateTime? EventDate { get; set; }

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
