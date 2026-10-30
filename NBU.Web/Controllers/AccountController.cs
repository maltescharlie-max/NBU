using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NBU.Data;
using NBU.Data.Models;
using NBU.Web.Models;

namespace NBU.Web.Controllers;

public sealed class AccountController(
    SignInManager<NbuUser> signInManager,
    UserManager<NbuUser> userManager,
    AppDbContext context) : Controller
{
    [AllowAnonymous, HttpGet]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var email = model.Email.Trim();
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "El correo o la contraseña no son correctos.");
            return View(model);
        }

        var passwordCheck = await signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);
        if (!passwordCheck.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "El correo o la contraseña no son correctos.");
            return View(model);
        }

        if (await userManager.IsInRoleAsync(user, NbuRoles.Student))
        {
            var student = user.StudentId is int studentId
                ? await context.Students.FirstOrDefaultAsync(item => item.Id == studentId)
                : null;
            if (student is null || student.Status != "Active")
            {
                ViewBag.RejectionReason = student?.RejectionReason;
                ModelState.AddModelError(string.Empty, student?.Status == "Rejected"
                    ? "Tu registro no fue aprobado. Comunícate con administración."
                    : "Tu registro está pendiente de revisión por administración.");
                return View(model);
            }
        }

        await signInManager.SignInAsync(user, model.RememberMe);

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return LocalRedirect(model.ReturnUrl);

        return await userManager.IsInRoleAsync(user, NbuRoles.Student)
            ? RedirectToAction("Index", "StudentPortal")
            : RedirectToAction("Index", "Home");
    }

    [AllowAnonymous, HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [AllowAnonymous, HttpGet]
    public IActionResult Resubmit() => View(new ResubmitViewModel());

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Resubmit(ResubmitViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var student = await context.Students.FirstOrDefaultAsync(item => item.Email == model.Email.Trim() && item.NationalId == model.OriginalNationalId.Trim() && item.Status == "Rejected");
        if (student is null)
        {
            ModelState.AddModelError(string.Empty, "No encontramos un registro rechazado con ese correo y cédula.");
            return View(model);
        }
        if (await context.Students.AnyAsync(item => item.Id != student.Id && item.NationalId == model.NationalId.Trim()))
        {
            ModelState.AddModelError(nameof(model.NationalId), "Esa cédula ya pertenece a otro registro.");
            return View(model);
        }
        student.FirstName = model.FirstName.Trim(); student.LastName = model.LastName.Trim(); student.NationalId = model.NationalId.Trim();
        student.Phone = model.Phone?.Trim(); student.Career = model.Career.Trim(); student.University = model.University.Trim();
        student.Status = "Pending"; student.RejectionReason = null;
        var linkedUser = await context.Users.FirstOrDefaultAsync(user => user.StudentId == student.Id);
        if (linkedUser is not null) linkedUser.DisplayName = $"{student.FirstName} {student.LastName}";
        await context.SaveChangesAsync();
        return View("RegistrationSubmitted");
    }

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var email = model.Email.Trim();
        var referral = string.IsNullOrWhiteSpace(model.ReferralCode) ? null : model.ReferralCode.Trim().ToUpperInvariant();
        var referrer = referral is null ? null : await context.Students.FirstOrDefaultAsync(item => item.ReferralCode == referral && item.Status == "Active");
        if (referral is not null && referrer is null)
        {
            ModelState.AddModelError(nameof(model.ReferralCode), "Ese código de recomendación no existe o no está activo.");
            return View(model);
        }
        if (await userManager.FindByEmailAsync(email) is not null ||
            await context.Students.AnyAsync(student => student.Email == email || student.NationalId == model.NationalId.Trim()))
        {
            ModelState.AddModelError(string.Empty, "Ya existe una cuenta o registro con ese correo o cédula.");
            return View(model);
        }

        var student = new Student
        {
            StudentCode = $"NBU-R-{Guid.NewGuid():N}"[..14].ToUpperInvariant(),
            FirstName = model.FirstName.Trim(), LastName = model.LastName.Trim(),
            NationalId = model.NationalId.Trim(), Email = email,
            Phone = model.Phone?.Trim(), Career = model.Career.Trim(),
            University = model.University.Trim(), Status = "Pending",
            ReferralCode = $"NBU{Guid.NewGuid():N}"[..10].ToUpperInvariant(),
            ReferredByCode = referrer?.ReferralCode
        };
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var user = new NbuUser
        {
            UserName = email, Email = email,
            DisplayName = $"{student.FirstName} {student.LastName}", StudentId = student.Id
        };
        var result = await userManager.CreateAsync(user, model.Password);
        if (result.Succeeded) result = await userManager.AddToRoleAsync(user, NbuRoles.Student);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            context.Students.Remove(student);
            await context.SaveChangesAsync();
            return View(model);
        }

        return View("RegistrationSubmitted");
    }

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await userManager.GetUserAsync(User);
        if (user is null) return Challenge();

        var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        await signInManager.RefreshSignInAsync(user);
        TempData["Success"] = "La contraseña se cambió correctamente.";
        return User.IsInRole(NbuRoles.Student)
            ? RedirectToAction("Index", "StudentPortal")
            : RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();
}
