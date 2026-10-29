using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace NBU.Data.Models;

public sealed class NbuUser : IdentityUser<int>
{
    [Required, StringLength(120)]
    public string DisplayName { get; set; } = string.Empty;

    public int? StudentId { get; set; }
    public Student? Student { get; set; }
}
