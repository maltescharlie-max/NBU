using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NBU.Data;
using NBU.Data.Models;

namespace NBU.Web.Controllers;

public class StudentsController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Index(string? search)
    {
        var students = context.Students.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            students = students.Where(student =>
                student.StudentCode.Contains(term) ||
                student.FirstName.Contains(term) ||
                student.LastName.Contains(term) ||
                student.NationalId.Contains(term) ||
                student.Email.Contains(term));
        }

        ViewData["Search"] = search;
        return View(await students.OrderBy(student => student.LastName).ThenBy(student => student.FirstName).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var student = await context.Students.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        return student is null ? NotFound() : View(student);
    }

    public IActionResult Create() => View(new Student());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("StudentCode,FirstName,LastName,NationalId,Email,Phone,Career,University,Status")] Student student)
    {
        await ValidateUniqueFields(student);
        if (!ModelState.IsValid) return View(student);

        context.Add(student);
        await context.SaveChangesAsync();
        TempData["Success"] = "El estudiante se registró correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var student = await context.Students.FindAsync(id);
        return student is null ? NotFound() : View(student);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,StudentCode,FirstName,LastName,NationalId,Email,Phone,Career,University,Status")] Student student)
    {
        if (id != student.Id) return NotFound();

        await ValidateUniqueFields(student, id);
        if (!ModelState.IsValid) return View(student);

        try
        {
            context.Update(student);
            await context.SaveChangesAsync();
            TempData["Success"] = "Los datos del estudiante se actualizaron.";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await context.Students.AnyAsync(item => item.Id == id)) return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var student = await context.Students.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        return student is null ? NotFound() : View(student);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var student = await context.Students.FindAsync(id);
        if (student is null) return NotFound();

        context.Students.Remove(student);
        try
        {
            await context.SaveChangesAsync();
            TempData["Success"] = "El estudiante se eliminó correctamente.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "No se puede eliminar este estudiante porque tiene solicitudes o notificaciones relacionadas.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateUniqueFields(Student student, int? currentId = null)
    {
        var duplicateCode = await context.Students.AnyAsync(item => item.StudentCode == student.StudentCode && item.Id != currentId);
        var duplicateId = await context.Students.AnyAsync(item => item.NationalId == student.NationalId && item.Id != currentId);
        var duplicateEmail = await context.Students.AnyAsync(item => item.Email == student.Email && item.Id != currentId);

        if (duplicateCode) ModelState.AddModelError(nameof(student.StudentCode), "Este código estudiantil ya está registrado.");
        if (duplicateId) ModelState.AddModelError(nameof(student.NationalId), "Esta cédula ya está registrada.");
        if (duplicateEmail) ModelState.AddModelError(nameof(student.Email), "Este correo electrónico ya está registrado.");
    }
}
