using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NBU.Data;
using NBU.Data.Models;

namespace NBU.Web.Controllers;

[Authorize(Roles = NbuRoles.Admin)]
public class ApplicationsController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Index(string? status)
    {
        var applications = context.ScholarshipApplications.AsNoTracking()
            .Include(item => item.Student).Include(item => item.Scholarship).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) applications = applications.Where(item => item.Status == status);
        ViewData["Status"] = status;
        return View(await applications.OrderByDescending(item => item.SubmittedAt).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var application = await context.ScholarshipApplications.AsNoTracking()
            .Include(item => item.Student).Include(item => item.Scholarship)
            .FirstOrDefaultAsync(item => item.Id == id);
        return application is null ? NotFound() : View(application);
    }

    public async Task<IActionResult> Create(int? scholarshipId)
    {
        await LoadOptions(scholarshipId: scholarshipId);
        return View(new ScholarshipApplication { ScholarshipId = scholarshipId ?? 0 });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("StudentId,ScholarshipId,Status,Observations")] ScholarshipApplication application)
    {
        ModelState.Remove(nameof(application.Student));
        ModelState.Remove(nameof(application.Scholarship));
        await ValidateApplication(application);
        if (!ModelState.IsValid)
        {
            await LoadOptions(application.StudentId, application.ScholarshipId);
            return View(application);
        }
        application.SubmittedAt = DateTime.UtcNow;
        context.Add(application);
        await context.SaveChangesAsync();
        TempData["Success"] = "La solicitud quedó registrada.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var application = await context.ScholarshipApplications.FindAsync(id);
        if (application is null) return NotFound();
        await LoadOptions(application.StudentId, application.ScholarshipId);
        return View(application);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,StudentId,ScholarshipId,Status,Observations")] ScholarshipApplication application)
    {
        if (id != application.Id) return NotFound();
        ModelState.Remove(nameof(application.Student));
        ModelState.Remove(nameof(application.Scholarship));
        await ValidateApplication(application, id);
        if (!ModelState.IsValid)
        {
            await LoadOptions(application.StudentId, application.ScholarshipId);
            return View(application);
        }

        var existing = await context.ScholarshipApplications.FindAsync(id);
        if (existing is null) return NotFound();
        existing.StudentId = application.StudentId;
        existing.ScholarshipId = application.ScholarshipId;
        existing.Status = application.Status;
        existing.Observations = application.Observations;
        await context.SaveChangesAsync();
        TempData["Success"] = "La solicitud se actualizó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var application = await context.ScholarshipApplications.AsNoTracking()
            .Include(item => item.Student).Include(item => item.Scholarship)
            .FirstOrDefaultAsync(item => item.Id == id);
        return application is null ? NotFound() : View(application);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var application = await context.ScholarshipApplications.FindAsync(id);
        if (application is null) return NotFound();
        context.Remove(application);
        try
        {
            await context.SaveChangesAsync();
            TempData["Success"] = "La solicitud se eliminó correctamente.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se puede eliminar esta solicitud porque tiene pagos asociados.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateApplication(ScholarshipApplication application, int? currentId = null)
    {
        if (!await context.Students.AnyAsync(item => item.Id == application.StudentId))
            ModelState.AddModelError(nameof(application.StudentId), "Selecciona un estudiante válido.");
        if (!await context.Scholarships.AnyAsync(item => item.Id == application.ScholarshipId))
            ModelState.AddModelError(nameof(application.ScholarshipId), "Selecciona una beca válida.");
        if (!new[] { "Pending", "Accepted", "Rejected" }.Contains(application.Status))
            ModelState.AddModelError(nameof(application.Status), "Selecciona un estado válido.");
        var duplicate = await context.ScholarshipApplications.AnyAsync(item =>
            item.StudentId == application.StudentId && item.ScholarshipId == application.ScholarshipId && item.Id != currentId);
        if (duplicate) ModelState.AddModelError(string.Empty, "Este estudiante ya tiene una solicitud para esa beca.");
    }

    private async Task LoadOptions(int? studentId = null, int? scholarshipId = null)
    {
        ViewBag.Students = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
            await context.Students.AsNoTracking().OrderBy(item => item.LastName).ThenBy(item => item.FirstName).ToListAsync(), "Id", "FullName", studentId);
        ViewBag.Scholarships = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
            await context.Scholarships.AsNoTracking().OrderBy(item => item.Name).ToListAsync(), "Id", "Name", scholarshipId);
    }
}
