using Microsoft.EntityFrameworkCore;
using NBU.Data;
using NBU.Data.Models;

namespace NBU.Desktop;

internal static class ModuleForms
{
    private static readonly EditorChoice[] ScholarshipStatuses = [new("Open", "Abierta"), new("Closed", "Cerrada"), new("Draft", "Borrador")];
    private static readonly EditorChoice[] ApplicationStatuses = [new("Pending", "Pendiente"), new("Accepted", "Aceptada"), new("Rejected", "Rechazada")];
    private static readonly EditorChoice[] PaymentStatuses = [new("Pending", "Pendiente"), new("Paid", "Pagado"), new("Cancelled", "Cancelado")];
    private static readonly EditorChoice[] PublicationTypes = [new("News", "Noticia"), new("Event", "Evento"), new("Announcement", "Aviso")];

    public static DesktopCrudManager<Scholarship, ScholarshipRow> Scholarships(DbContextOptions<AppDbContext> options) => new(
        options, "Becas", "Administra las convocatorias y sus condiciones.",
        [new("Name", "Beca", 120), new("Amount", "Monto", 75), new("Period", "Vigencia", 105), new("Status", "Estado", 65), new("Applications", "Solicitudes", 55)],
        async db => await db.Scholarships.AsNoTracking().Include(item => item.Applications).OrderByDescending(item => item.StartDate).ToListAsync(),
        item => new ScholarshipRow(item.Id, item.Name, item.Amount.ToString("C2", new System.Globalization.CultureInfo("es-NI")), $"{item.StartDate:dd/MM/yy} – {item.EndDate:dd/MM/yy}", ScholarshipStatus(item.Status), item.Applications.Count.ToString()),
        (_, _) => Task.FromResult<IReadOnlyList<EditorField>>([
            new("Name", "Nombre de la beca"),
            new("Amount", "Monto por período", EditorKind.Decimal),
            new("StartDate", "Fecha de inicio", EditorKind.Date),
            new("EndDate", "Fecha de cierre", EditorKind.Date),
            new("Status", "Estado", EditorKind.Choice, ScholarshipStatuses),
            new("Description", "Descripción", EditorKind.Multiline),
            new("Requirements", "Requisitos", EditorKind.Multiline)
        ]),
        (db, item, _) =>
        {
            DesktopCrudManager<Scholarship, ScholarshipRow>.ValidateModel(item);
            if (item.EndDate < item.StartDate) throw new InvalidOperationException("La fecha de cierre debe ser igual o posterior al inicio.");
            if (item.Id == 0) db.Scholarships.Add(item); else db.Scholarships.Update(item);
            return Task.CompletedTask;
        },
        (db, item) => { db.Scholarships.Remove(item); return Task.CompletedTask; },
        row => $"{row.Name} {row.Status} {row.Period}",
        "¿Eliminar esta convocatoria? Si tiene solicitudes, la base de datos conservará el historial y no permitirá eliminarla.");

    public static DesktopCrudManager<ScholarshipApplication, ApplicationRow> Applications(DbContextOptions<AppDbContext> options) => new(
        options, "Solicitudes", "Revisa solicitudes y registra la decisión del personal encargado.",
        [new("Student", "Estudiante", 120), new("Scholarship", "Beca", 130), new("Submitted", "Fecha", 80), new("Status", "Estado", 75)],
        async db => await db.ScholarshipApplications.AsNoTracking().Include(item => item.Student).Include(item => item.Scholarship).OrderByDescending(item => item.SubmittedAt).ToListAsync(),
        item => new ApplicationRow(item.Id, item.Student.FullName, item.Scholarship.Name, item.SubmittedAt.ToLocalTime().ToString("dd MMM yyyy"), ApplicationStatus(item.Status)),
        async (db, _) =>
        {
            var students = await db.Students.AsNoTracking().OrderBy(item => item.LastName).ThenBy(item => item.FirstName).ToListAsync();
            var scholarships = await db.Scholarships.AsNoTracking().OrderBy(item => item.Name).ToListAsync();
            return [
                new("StudentId", "Estudiante", EditorKind.Choice, students.Select(item => new EditorChoice(item.Id.ToString(), item.FullName)).ToList()),
                new("ScholarshipId", "Beca", EditorKind.Choice, scholarships.Select(item => new EditorChoice(item.Id.ToString(), item.Name)).ToList()),
                new("Status", "Resultado", EditorKind.Choice, ApplicationStatuses),
                new("Observations", "Observaciones", EditorKind.Multiline)
            ];
        },
        async (db, item, _) =>
        {
            DesktopCrudManager<ScholarshipApplication, ApplicationRow>.ValidateModel(item);
            var studentExists = await db.Students.AnyAsync(row => row.Id == item.StudentId);
            var scholarshipExists = await db.Scholarships.AnyAsync(row => row.Id == item.ScholarshipId);
            if (!studentExists || !scholarshipExists) throw new InvalidOperationException("Selecciona un estudiante y una beca válidos.");
            if (await db.ScholarshipApplications.AnyAsync(row => row.StudentId == item.StudentId && row.ScholarshipId == item.ScholarshipId && row.Id != item.Id))
                throw new InvalidOperationException("Este estudiante ya tiene una solicitud para esa beca.");
            if (item.Id == 0) { item.SubmittedAt = DateTime.UtcNow; db.ScholarshipApplications.Add(item); }
            else db.ScholarshipApplications.Update(item);
        },
        (db, item) => { db.ScholarshipApplications.Remove(item); return Task.CompletedTask; },
        row => $"{row.Student} {row.Scholarship} {row.Status}",
        "¿Eliminar esta solicitud? Si hay pagos asociados, no se permitirá eliminar el historial.");

    public static DesktopCrudManager<Payment, PaymentRow> Payments(DbContextOptions<AppDbContext> options) => new(
        options, "Pagos", "Registra los depósitos asociados a solicitudes aceptadas.",
        [new("Student", "Beneficiario", 120), new("Scholarship", "Beca", 110), new("Period", "Período", 75), new("Date", "Depósito", 75), new("Amount", "Monto", 80), new("Status", "Estado", 65)],
        async db => await db.Payments.AsNoTracking().Include(item => item.ScholarshipApplication).ThenInclude(item => item.Student).Include(item => item.ScholarshipApplication).ThenInclude(item => item.Scholarship).OrderByDescending(item => item.DepositDate).ToListAsync(),
        item => new PaymentRow(item.Id, item.ScholarshipApplication.Student.FullName, item.ScholarshipApplication.Scholarship.Name, item.Period, item.DepositDate.ToString("dd MMM yyyy"), item.Amount.ToString("C2", new System.Globalization.CultureInfo("es-NI")), PaymentStatus(item.Status)),
        async (db, _) =>
        {
            var accepted = await db.ScholarshipApplications.AsNoTracking().Where(item => item.Status == "Accepted").Include(item => item.Student).Include(item => item.Scholarship).OrderBy(item => item.Student.LastName).ToListAsync();
            return [
                new("ScholarshipApplicationId", "Solicitud aceptada", EditorKind.Choice, accepted.Select(item => new EditorChoice(item.Id.ToString(), $"{item.Student.FullName} · {item.Scholarship.Name}")).ToList()),
                new("Amount", "Monto depositado", EditorKind.Decimal),
                new("DepositDate", "Fecha del depósito", EditorKind.Date),
                new("Period", "Período", EditorKind.Text),
                new("Status", "Estado", EditorKind.Choice, PaymentStatuses)
            ];
        },
        async (db, item, _) =>
        {
            DesktopCrudManager<Payment, PaymentRow>.ValidateModel(item);
            var accepted = await db.ScholarshipApplications.AnyAsync(application => application.Id == item.ScholarshipApplicationId && application.Status == "Accepted");
            if (!accepted) throw new InvalidOperationException("Solo se pueden registrar pagos para solicitudes aceptadas.");
            if (item.Id == 0) db.Payments.Add(item); else db.Payments.Update(item);
        },
        (db, item) => { db.Payments.Remove(item); return Task.CompletedTask; },
        row => $"{row.Student} {row.Scholarship} {row.Period} {row.Status}");

    public static DesktopCrudManager<Publication, PublicationRow> Publications(DbContextOptions<AppDbContext> options) => new(
        options, "Noticias y eventos", "Publica noticias, eventos y avisos para estudiantes.",
        [new("Type", "Tipo", 65), new("Title", "Título", 145), new("Published", "Publicado", 85), new("EventDate", "Fecha del evento", 95), new("Notifications", "Avisos", 55)],
        async db => await db.Publications.AsNoTracking().Include(item => item.Notifications).OrderByDescending(item => item.PublishedAt).ToListAsync(),
        item => new PublicationRow(item.Id, PublicationType(item.Type), item.Title, item.PublishedAt.ToLocalTime().ToString("dd MMM yyyy"), item.EventDate?.ToLocalTime().ToString("dd MMM yyyy HH:mm") ?? "—", item.Notifications.Count.ToString()),
        (_, item) =>
        {
            var fields = new List<EditorField>
            {
                new("Type", "Tipo de publicación", EditorKind.Choice, PublicationTypes),
                new("Title", "Título"),
                new("EventDate", "Fecha del evento (opcional)", EditorKind.DateTime, Optional: true),
                new("Content", "Contenido", EditorKind.Multiline)
            };
            if (item.Id == 0) fields.Add(new("__NotifyStudents", "Notificar a estudiantes activos", EditorKind.Boolean));
            return Task.FromResult<IReadOnlyList<EditorField>>(fields);
        },
        async (db, item, notifyStudents) =>
        {
            DesktopCrudManager<Publication, PublicationRow>.ValidateModel(item);
            if (item.Type == "Event" && item.EventDate is null) throw new InvalidOperationException("Indica la fecha del evento.");
            if (item.Id == 0)
            {
                item.PublishedAt = DateTime.UtcNow;
                db.Publications.Add(item);
                if (notifyStudents)
                {
                    var studentIds = await db.Students.AsNoTracking().Where(student => student.Status == "Active").Select(student => student.Id).ToListAsync();
                    foreach (var studentId in studentIds)
                        db.Notifications.Add(new Notification { StudentId = studentId, Publication = item, Message = $"Nueva publicación: {item.Title}", CreatedAt = DateTime.UtcNow });
                }
            }
            else db.Publications.Update(item);
        },
        (db, item) => { db.Publications.Remove(item); return Task.CompletedTask; },
        row => $"{row.Type} {row.Title} {row.Published} {row.EventDate}");

    public static DesktopCrudManager<Notification, NotificationRow> Notifications(DbContextOptions<AppDbContext> options) => new(
        options, "Notificaciones", "Consulta y administra los avisos enviados dentro de NBU.",
        [new("Student", "Estudiante", 120), new("Message", "Mensaje", 170), new("Date", "Fecha", 95), new("Read", "Leída", 55)],
        async db => await db.Notifications.AsNoTracking().Include(item => item.Student).OrderByDescending(item => item.CreatedAt).ToListAsync(),
        item => new NotificationRow(item.Id, item.Student.FullName, item.Message, item.CreatedAt.ToLocalTime().ToString("dd MMM yyyy HH:mm"), item.IsRead ? "Sí" : "No"),
        async (db, _) =>
        {
            var students = await db.Students.AsNoTracking().OrderBy(item => item.LastName).ToListAsync();
            return [new("StudentId", "Estudiante", EditorKind.Choice, students.Select(item => new EditorChoice(item.Id.ToString(), item.FullName)).ToList()), new("Message", "Mensaje", EditorKind.Multiline), new("IsRead", "Leída", EditorKind.Boolean)];
        },
        (db, item, _) => { if (item.Id == 0) { item.CreatedAt = DateTime.UtcNow; db.Notifications.Add(item); } else db.Notifications.Update(item); return Task.CompletedTask; },
        (db, item) => { db.Notifications.Remove(item); return Task.CompletedTask; },
        row => $"{row.Student} {row.Message} {row.Date} {row.Read}");

    private static string ScholarshipStatus(string value) => value switch { "Open" => "Abierta", "Closed" => "Cerrada", _ => "Borrador" };
    private static string ApplicationStatus(string value) => value switch { "Accepted" => "Aceptada", "Rejected" => "Rechazada", _ => "Pendiente" };
    private static string PaymentStatus(string value) => value switch { "Paid" => "Pagado", "Cancelled" => "Cancelado", _ => "Pendiente" };
    private static string PublicationType(string value) => value switch { "Event" => "Evento", "Announcement" => "Aviso", _ => "Noticia" };
}

internal sealed record ScholarshipRow(int Id, string Name, string Amount, string Period, string Status, string Applications);
internal sealed record ApplicationRow(int Id, string Student, string Scholarship, string Submitted, string Status);
internal sealed record PaymentRow(int Id, string Student, string Scholarship, string Period, string Date, string Amount, string Status);
internal sealed record PublicationRow(int Id, string Type, string Title, string Published, string EventDate, string Notifications);
internal sealed record NotificationRow(int Id, string Student, string Message, string Date, string Read);
