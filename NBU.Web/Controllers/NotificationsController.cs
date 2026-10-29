using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NBU.Data;

namespace NBU.Web.Controllers;

[Authorize(Roles = NbuRoles.Admin)]
public class NotificationsController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Index(string? state)
    {
        var notifications = context.Notifications.AsNoTracking()
            .Include(item => item.Student).Include(item => item.Publication).AsQueryable();
        if (state == "Unread") notifications = notifications.Where(item => !item.IsRead);
        if (state == "Read") notifications = notifications.Where(item => item.IsRead);
        ViewData["State"] = state;
        ViewBag.UnreadCount = await context.Notifications.CountAsync(item => !item.IsRead);
        return View(await notifications.OrderByDescending(item => item.CreatedAt).ToListAsync());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id)
    {
        var notification = await context.Notifications.FindAsync(id);
        if (notification is null) return NotFound();
        notification.IsRead = true;
        await context.SaveChangesAsync();
        TempData["Success"] = "La notificación se marcó como leída.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var notification = await context.Notifications.FindAsync(id);
        if (notification is null) return NotFound();
        context.Notifications.Remove(notification);
        await context.SaveChangesAsync();
        TempData["Success"] = "La notificación se eliminó del historial.";
        return RedirectToAction(nameof(Index));
    }
}
