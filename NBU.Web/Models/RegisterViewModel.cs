using System.ComponentModel.DataAnnotations;

namespace NBU.Web.Models;

public sealed class RegisterViewModel
{
    [Required, StringLength(80)] public string FirstName { get; set; } = "";
    [Required, StringLength(80)] public string LastName { get; set; } = "";
    [Required, StringLength(30)] public string NationalId { get; set; } = "";
    [Required, EmailAddress, StringLength(160)] public string Email { get; set; } = "";
    [Phone, StringLength(30)] public string? Phone { get; set; }
    [Required, StringLength(120)] public string Career { get; set; } = "";
    [Required, StringLength(120)] public string University { get; set; } = "";
    [StringLength(20)] public string? ReferralCode { get; set; }
    [Required, MinLength(8), RegularExpression("^[A-Za-z0-9]+$", ErrorMessage = "Usa solo letras y números (mínimo 8 caracteres).")]
    [DataType(DataType.Password)] public string Password { get; set; } = "";
    [Required, Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    [DataType(DataType.Password)] public string ConfirmPassword { get; set; } = "";
}
