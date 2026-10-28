using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NBU.Data.Models;

public sealed class Payment
{
    public int Id { get; set; }
    [Display(Name = "Solicitud beneficiada")]
    public int ScholarshipApplicationId { get; set; }
    [Range(typeof(decimal), "0.01", "999999999.99")]
    [Display(Name = "Monto depositado")]
    public decimal Amount { get; set; }
    [Display(Name = "Fecha del depósito")]
    public DateOnly DepositDate { get; set; }
    [Required, StringLength(50)]
    [Display(Name = "Período")]
    public string Period { get; set; } = string.Empty;
    [Required, StringLength(30)]
    [Display(Name = "Estado del pago")]
    public string Status { get; set; } = "Pending";

    public ScholarshipApplication ScholarshipApplication { get; set; } = null!;

    [NotMapped]
    public string ApplicationLabel => $"{ScholarshipApplication.Student.FullName} · {ScholarshipApplication.Scholarship.Name}";
}
