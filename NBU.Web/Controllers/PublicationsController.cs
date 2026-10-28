using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NBU.Data;
using NBU.Data.Models;

namespace NBU.Web.Controllers;

public class PublicationsController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Index(string? type)
    {
        var publications = context.Publications.AsNoTracking().Include(item => item.Notifications).AsQueryable();
        if (!string.IsNullOrWhiteSpace(type)) publications = publications.Where(item => item.Type == type);
        ViewData["Type"] = type;
        return View(await publications.OrderByDescending(item => item.PublishedAt).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var publication = await context.Publications.AsNoTracking().Include(item => item.Notifications)
            .FirstOrDefaultAsync(item => item.Id == id);
        return publication is null ? NotFound() : View(publication);
    }

    public IActionResult Create() => View(new Publication { PublishedAt = DateTime.Now });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Type,Title,Content,EventDate")] Publication publication, bool notifyStudents)
    {
        ValidatePublication(publication);
        if (!ModelState.IsValid) return View(publication);

        publication.PublishedAt = DateTime.UtcNow;
        context.Publications.Add(publication);
        var recipientCount = 0;
        if (notifyStudents)
        {
            var studentIds = await context.Students.AsNoTracking().Where(item => item.Status == "Active")
                .Select(item => item.Id).ToListAsync();
            recipientCount = studentIds.Count;
            foreach (var studentId in studentIds)
            {
                context.Notifications.Add(new Notification
                {
                    StudentId = studentId,
                    Publication = publication,
                    Message = $"Nueva publicación: {publication.Title}",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await context.SaveChangesAsync();
        TempData["Success"] = notifyStudents
            ? $"La publicación se creó y se generaron {recipientCount} notificación(es)."
            : "La publicación se creó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var publication = await context.Publications.FindAsync(id);
        return publication is null ? NotFound() : View(publication);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Type,Title,Content,EventDate")] Publication publication)
    {
        if (id != publication.Id) return NotFound();
        ValidatePublication(publication);
        if (!ModelState.IsValid) return View(publication);

        var existing = await context.Publications.FindAsync(id);
        if (existing is null) return NotFound();
        existing.Type = publication.Type;
        existing.Title = publication.Title;
        existing.Content = publication.Content;
        existing.EventDate = publication.EventDate;
        await context.SaveChangesAsync();
        TempData["Success"] = "La publicación se actualizó correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var publication = await context.Publications.AsNoTracking().Include(item => item.Notifications)
            .FirstOrDefaultAsync(item => item.Id == id);
        return publication is null ? NotFound() : View(publication);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var publication = await context.Publications.FindAsync(id);
        if (publication is null) return NotFound();
        context.Publications.Remove(publication);
        await context.SaveChangesAsync();
        TempData["Success"] = "La publicación se eliminó. Las notificaciones ya enviadas permanecen en el historial.";
        return RedirectToAction(nameof(Index));
    }

    private void ValidatePublication(Publication publication)
    {
        if (!new[] { "News", "Event", "Announcement" }.Contains(publication.Type))
            ModelState.AddModelError(nameof(publication.Type), "Selecciona un tipo de publicación válido.");
        if (publication.Type == "Event" && publication.EventDate is null)
            ModelState.AddModelError(nameof(publication.EventDate), "Indica la fecha del evento.");
    }
}
