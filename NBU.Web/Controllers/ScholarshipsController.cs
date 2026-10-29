using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NBU.Data;
using NBU.Data.Models;

namespace NBU.Web.Controllers;

[Authorize(Roles = NbuRoles.Admin)]
public class ScholarshipsController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Index(string? search)
    {
        var scholarships = context.Scholarships.AsNoTracking().Include(item => item.Applications).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            scholarships = scholarships.Where(item => item.Name.Contains(term) || item.Description.Contains(term));
        }

        ViewData["Search"] = search;
        return View(await scholarships.OrderByDescending(item => item.StartDate).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var scholarship = await context.Scholarships.AsNoTracking()
            .Include(item => item.Applications).ThenInclude(item => item.Student)
            .FirstOrDefaultAsync(item => item.Id == id);
        return scholarship is null ? NotFound() : View(scholarship);
    }

    public IActionResult Create() => View(new Scholarship { StartDate = DateOnly.FromDateTime(DateTime.Today), EndDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)) });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Description,Requirements,Amount,StartDate,EndDate,Status")] Scholarship scholarship)
    {
        ValidateDates(scholarship);
        if (!ModelState.IsValid) return View(scholarship);
        context.Add(scholarship);
        await context.SaveChangesAsync();
        TempData["Success"] = "La beca se publicó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var scholarship = await context.Scholarships.FindAsync(id);
        return scholarship is null ? NotFound() : View(scholarship);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Requirements,Amount,StartDate,EndDate,Status")] Scholarship scholarship)
    {
        if (id != scholarship.Id) return NotFound();
        ValidateDates(scholarship);
        if (!ModelState.IsValid) return View(scholarship);
        context.Update(scholarship);
        await context.SaveChangesAsync();
        TempData["Success"] = "Los datos de la beca se actualizaron.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var scholarship = await context.Scholarships.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        return scholarship is null ? NotFound() : View(scholarship);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var scholarship = await context.Scholarships.FindAsync(id);
        if (scholarship is null) return NotFound();
        context.Remove(scholarship);
        try
        {
            await context.SaveChangesAsync();
            TempData["Success"] = "La beca se eliminó correctamente.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se puede eliminar esta beca porque tiene solicitudes relacionadas.";
        }
        return RedirectToAction(nameof(Index));
    }

    private void ValidateDates(Scholarship scholarship)
    {
        if (scholarship.EndDate < scholarship.StartDate)
            ModelState.AddModelError(nameof(scholarship.EndDate), "La fecha de cierre debe ser igual o posterior a la fecha de inicio.");
        if (!new[] { "Open", "Closed", "Draft" }.Contains(scholarship.Status))
            ModelState.AddModelError(nameof(scholarship.Status), "Selecciona un estado válido.");
    }
}
