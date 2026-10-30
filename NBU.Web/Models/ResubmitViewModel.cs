using System.ComponentModel.DataAnnotations;
namespace NBU.Web.Models;
public sealed class ResubmitViewModel
{
 [Required,EmailAddress] public string Email { get; set; } = "";
 [Required] public string OriginalNationalId { get; set; } = "";
 [Required] public string NationalId { get; set; } = "";
 [Required,StringLength(80)] public string FirstName { get; set; } = "";
 [Required,StringLength(80)] public string LastName { get; set; } = "";
 [Phone,StringLength(30)] public string? Phone { get; set; }
 [Required,StringLength(120)] public string Career { get; set; } = "";
 [Required,StringLength(120)] public string University { get; set; } = "";
}
