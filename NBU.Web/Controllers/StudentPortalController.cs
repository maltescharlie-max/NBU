using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NBU.Data;
using NBU.Data.Models;
using NBU.Web.Models;

namespace NBU.Web.Controllers;

[Authorize(Roles = NbuRoles.Student)]
public sealed class StudentPortalController(AppDbContext context, UserManager<NbuUser> userManager, IWebHostEnvironment environment) : Controller
{
    public async Task<IActionResult> Index()
    {
        var student = await CurrentStudentAsync();
        if (student is null) return Forbid();
        if (string.IsNullOrWhiteSpace(student.ReferralCode))
        {
            var trackedStudent = await context.Students.FirstAsync(item => item.Id == student.Id);
            trackedStudent.ReferralCode = $"NBU{Guid.NewGuid():N}"[..10].ToUpperInvariant();
            await context.SaveChangesAsync();
            student.ReferralCode = trackedStudent.ReferralCode;
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var applications = await context.ScholarshipApplications.AsNoTracking()
            .Where(item => item.StudentId == student.Id)
            .Include(item => item.Scholarship)
            .OrderByDescending(item => item.SubmittedAt)
            .Take(5)
            .ToListAsync();
        var notifications = await context.Notifications.AsNoTracking()
            .Where(item => item.StudentId == student.Id)
            .Include(item => item.Publication)
            .OrderByDescending(item => item.CreatedAt)
            .Take(5)
            .ToListAsync();
        var model = new StudentDashboardViewModel(
            student,
            await context.Scholarships.CountAsync(item => item.Status == "Open" && item.StartDate <= today && item.EndDate >= today),
            await context.ScholarshipApplications.CountAsync(item => item.StudentId == student.Id && item.Status == "Pending"),
            await context.Notifications.CountAsync(item => item.StudentId == student.Id && !item.IsRead),
            await context.Payments.Where(item => item.ScholarshipApplication.StudentId == student.Id && item.Status == "Paid")
                .SumAsync(item => (decimal?)item.Amount) ?? 0m,
            applications,
            notifications);

        return View(model);
    }

    public async Task<IActionResult> Scholarships()
    {
        var student = await CurrentStudentAsync();
        if (student is null) return Forbid();
        var today = DateOnly.FromDateTime(DateTime.Today);
        var scholarships = await context.Scholarships.AsNoTracking()
            .Where(item => item.Status == "Open" && item.StartDate <= today && item.EndDate >= today)
            .OrderBy(item => item.EndDate)
            .ToListAsync();
        var applications = await context.ScholarshipApplications.AsNoTracking()
            .Where(item => item.StudentId == student.Id)
            .ToDictionaryAsync(item => item.ScholarshipId, item => item.Status);

        return View(scholarships.Select(item => new StudentScholarshipItem(
            item, applications.GetValueOrDefault(item.Id))).ToList());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(int scholarshipId, IFormFile? document)
    {
        var student = await CurrentStudentAsync();
        if (student is null) return Forbid();
        var today = DateOnly.FromDateTime(DateTime.Today);
        var scholarship = await context.Scholarships.FirstOrDefaultAsync(item =>
            item.Id == scholarshipId && item.Status == "Open" && item.StartDate <= today && item.EndDate >= today);
        if (scholarship is null)
        {
            TempData["Error"] = "La convocatoria no está abierta para nuevas solicitudes.";
            return RedirectToAction(nameof(Scholarships));
        }

        var alreadyApplied = await context.ScholarshipApplications.AnyAsync(item =>
            item.StudentId == student.Id && item.ScholarshipId == scholarshipId);
        if (alreadyApplied)
        {
            TempData["Error"] = "Ya tienes una solicitud registrada para esta beca.";
            return RedirectToAction(nameof(Scholarships));
        }

        string? storageName = null;
        string? originalName = null;
        if (document is { Length: > 0 })
        {
            var extension = Path.GetExtension(document.FileName).ToLowerInvariant();
            if (document.Length > 5 * 1024 * 1024 || extension is not (".pdf" or ".png" or ".jpg" or ".jpeg"))
            {
                TempData["Error"] = "Adjunta un PDF, JPG o PNG de hasta 5 MB.";
                return RedirectToAction(nameof(Scholarships));
            }
            storageName = $"{Guid.NewGuid():N}{extension}";
            originalName = Path.GetFileName(document.FileName);
            var folder = Path.Combine(environment.ContentRootPath, "App_Data", "ScholarshipDocuments");
            Directory.CreateDirectory(folder);
            await using var stream = System.IO.File.Create(Path.Combine(folder, storageName));
            await document.CopyToAsync(stream);
        }

        context.ScholarshipApplications.Add(new ScholarshipApplication
        {
            StudentId = student.Id,
            ScholarshipId = scholarshipId,
            SubmittedAt = DateTime.UtcNow,
            Status = "Pending", DocumentStorageName = storageName, DocumentFileName = originalName
        });
        await context.SaveChangesAsync();
        TempData["Success"] = "Tu solicitud quedó registrada para revisión.";
        return RedirectToAction(nameof(Applications));
    }

    public async Task<IActionResult> Document(int id)
    {
        var student = await CurrentStudentAsync();
        if (student is null) return Forbid();
        var application = await context.ScholarshipApplications.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id && item.StudentId == student.Id);
        if (application?.DocumentStorageName is null) return NotFound();
        var path = Path.Combine(environment.ContentRootPath, "App_Data", "ScholarshipDocuments", application.DocumentStorageName);
        if (!System.IO.File.Exists(path)) return NotFound();
        return PhysicalFile(path, "application/octet-stream", application.DocumentFileName ?? "documento");
    }

    public async Task<IActionResult> Applications()
    {
        var student = await CurrentStudentAsync();
        if (student is null) return Forbid();
        var applications = await context.ScholarshipApplications.AsNoTracking()
            .Where(item => item.StudentId == student.Id)
            .Include(item => item.Scholarship)
            .Include(item => item.Payments)
            .OrderByDescending(item => item.SubmittedAt)
            .ToListAsync();
        return View(new StudentApplicationsViewModel(student, applications));
    }

    public async Task<IActionResult> Payments()
    {
        var student = await CurrentStudentAsync();
        if (student is null) return Forbid();
        var payments = await context.Payments.AsNoTracking()
            .Where(item => item.ScholarshipApplication.StudentId == student.Id)
            .Include(item => item.ScholarshipApplication).ThenInclude(item => item.Scholarship)
            .OrderByDescending(item => item.DepositDate)
            .ToListAsync();
        var paidTotal = payments.Where(item => item.Status == "Paid").Sum(item => item.Amount);
        return View(new StudentPaymentsViewModel(student, payments, paidTotal));
    }

    public async Task<IActionResult> Publications()
    {
        var student = await CurrentStudentAsync();
        if (student is null) return Forbid();
        var publications = await context.Publications.AsNoTracking()
            .OrderByDescending(item => item.PublishedAt)
            .ToListAsync();
        return View(new StudentPublicationsViewModel(student, publications));
    }

    public async Task<IActionResult> Notifications()
    {
        var student = await CurrentStudentAsync();
        if (student is null) return Forbid();
        var notifications = await context.Notifications.AsNoTracking()
            .Where(item => item.StudentId == student.Id)
            .Include(item => item.Publication)
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync();
        return View(new StudentNotificationsViewModel(student, notifications));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id)
    {
        var student = await CurrentStudentAsync();
        if (student is null) return Forbid();
        var notification = await context.Notifications.FirstOrDefaultAsync(item => item.Id == id && item.StudentId == student.Id);
        if (notification is null) return NotFound();
        notification.IsRead = true;
        await context.SaveChangesAsync();
        TempData["Success"] = "El aviso se marcó como leído.";
        return RedirectToAction(nameof(Notifications));
    }

    private async Task<Student?> CurrentStudentAsync()
    {
        var user = await userManager.GetUserAsync(User);
        return user?.StudentId is int studentId
            ? await context.Students.AsNoTracking().FirstOrDefaultAsync(item => item.Id == studentId)
            : null;
    }
}
