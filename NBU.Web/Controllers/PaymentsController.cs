using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NBU.Data;
using NBU.Data.Models;

namespace NBU.Web.Controllers;

[Authorize(Roles = NbuRoles.Admin)]
public class PaymentsController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Index(string? status)
    {
        var payments = context.Payments.AsNoTracking()
            .Include(item => item.ScholarshipApplication).ThenInclude(item => item.Student)
            .Include(item => item.ScholarshipApplication).ThenInclude(item => item.Scholarship)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) payments = payments.Where(item => item.Status == status);
        ViewData["Status"] = status;
        return View(await payments.OrderByDescending(item => item.DepositDate).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var payment = await PaymentQuery().FirstOrDefaultAsync(item => item.Id == id);
        return payment is null ? NotFound() : View(payment);
    }

    public async Task<IActionResult> Create()
    {
        await LoadApplications();
        return View(new Payment { DepositDate = DateOnly.FromDateTime(DateTime.Today) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ScholarshipApplicationId,Amount,DepositDate,Period,Status")] Payment payment)
    {
        ModelState.Remove(nameof(payment.ScholarshipApplication));
        await ValidatePayment(payment);
        if (!ModelState.IsValid)
        {
            await LoadApplications(payment.ScholarshipApplicationId);
            return View(payment);
        }
        context.Add(payment);
        await context.SaveChangesAsync();
        TempData["Success"] = "El pago se registró correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var payment = await context.Payments.FindAsync(id);
        if (payment is null) return NotFound();
        await LoadApplications(payment.ScholarshipApplicationId);
        return View(payment);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ScholarshipApplicationId,Amount,DepositDate,Period,Status")] Payment payment)
    {
        if (id != payment.Id) return NotFound();
        ModelState.Remove(nameof(payment.ScholarshipApplication));
        await ValidatePayment(payment);
        if (!ModelState.IsValid)
        {
            await LoadApplications(payment.ScholarshipApplicationId);
            return View(payment);
        }
        context.Update(payment);
        await context.SaveChangesAsync();
        TempData["Success"] = "Los datos del pago se actualizaron.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var payment = await PaymentQuery().FirstOrDefaultAsync(item => item.Id == id);
        return payment is null ? NotFound() : View(payment);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var payment = await context.Payments.FindAsync(id);
        if (payment is null) return NotFound();
        context.Remove(payment);
        await context.SaveChangesAsync();
        TempData["Success"] = "El registro del pago se eliminó.";
        return RedirectToAction(nameof(Index));
    }

    private IQueryable<Payment> PaymentQuery() => context.Payments.AsNoTracking()
        .Include(item => item.ScholarshipApplication).ThenInclude(item => item.Student)
        .Include(item => item.ScholarshipApplication).ThenInclude(item => item.Scholarship);

    private async Task ValidatePayment(Payment payment)
    {
        var application = await context.ScholarshipApplications.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == payment.ScholarshipApplicationId);
        if (application is null)
            ModelState.AddModelError(nameof(payment.ScholarshipApplicationId), "Selecciona una solicitud válida.");
        else if (application.Status != "Accepted")
            ModelState.AddModelError(nameof(payment.ScholarshipApplicationId), "Solo se pueden registrar pagos para solicitudes aceptadas.");

        if (!new[] { "Pending", "Paid", "Cancelled" }.Contains(payment.Status))
            ModelState.AddModelError(nameof(payment.Status), "Selecciona un estado de pago válido.");
    }

    private async Task LoadApplications(int? selectedId = null)
    {
        var acceptedApplications = await context.ScholarshipApplications.AsNoTracking()
            .Where(item => item.Status == "Accepted")
            .Include(item => item.Student).Include(item => item.Scholarship)
            .OrderBy(item => item.Student.LastName).ThenBy(item => item.Student.FirstName)
            .ToListAsync();
        ViewBag.Applications = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(acceptedApplications, "Id", "PaymentLabel", selectedId);
    }
}
