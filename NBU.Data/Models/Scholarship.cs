using System.ComponentModel.DataAnnotations;

namespace NBU.Data.Models;

public sealed class Scholarship
{
    public int Id { get; set; }
    [Required, StringLength(120)]
    [Display(Name = "Nombre de la beca")]
    public string Name { get; set; } = string.Empty;
    [Required, StringLength(2000)]
    [Display(Name = "Descripción")]
    public string Description { get; set; } = string.Empty;
    [Required, StringLength(2000)]
    [Display(Name = "Requisitos")]
    public string Requirements { get; set; } = string.Empty;
    [Range(typeof(decimal), "0.01", "999999999.99")]
    [Display(Name = "Monto por período")]
    public decimal Amount { get; set; }
    [Display(Name = "Fecha de inicio")]
    public DateOnly StartDate { get; set; }
    [Display(Name = "Fecha de cierre")]
    public DateOnly EndDate { get; set; }
    [Required, StringLength(30)]
    [Display(Name = "Estado")]
    public string Status { get; set; } = "Open";

    public ICollection<ScholarshipApplication> Applications { get; set; } = new List<ScholarshipApplication>();
}
