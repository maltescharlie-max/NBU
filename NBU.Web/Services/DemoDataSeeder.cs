using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NBU.Data;
using NBU.Data.Models;

namespace NBU.Web.Services;

public static class DemoDataSeeder
{
    public const string AdminEmail = "admin@nbu.test";
    public const string AdminPassword = "NbuAdmin2026!";
    public const string StudentEmail = "ana.mendez@example.test";
    public const string StudentPassword = "NbuEstudiante2026!";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var userManager = services.GetRequiredService<UserManager<NbuUser>>();
        var context = services.GetRequiredService<AppDbContext>();

        await EnsureRoleAsync(roleManager, NbuRoles.Admin);
        await EnsureRoleAsync(roleManager, NbuRoles.Student);

        var students = new[]
        {
            new Student
            {
                StudentCode = "NBU-DEMO-001", FirstName = "Ana", LastName = "Méndez López",
                NationalId = "DEMO-001", Email = StudentEmail, Phone = "+505 0000 0001",
                Career = "Ingeniería en Sistemas", University = "UNAN-Managua", Status = "Active"
            },
            new Student
            {
                StudentCode = "NBU-DEMO-002", FirstName = "Luis", LastName = "Gómez Ruiz",
                NationalId = "DEMO-002", Email = "luis.gomez@example.test", Phone = "+505 0000 0002",
                Career = "Administración de Empresas", University = "UNAN-Managua", Status = "Active"
            },
            new Student
            {
                StudentCode = "NBU-DEMO-003", FirstName = "María", LastName = "Pérez Díaz",
                NationalId = "DEMO-003", Email = "maria.perez@example.test", Phone = "+505 0000 0003",
                Career = "Medicina", University = "UNAN-León", Status = "Active"
            }
        };

        foreach (var student in students)
        {
            if (!await context.Students.AnyAsync(item => item.StudentCode == student.StudentCode))
                context.Students.Add(student);
        }
        await context.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var scholarships = new[]
        {
            new Scholarship
            {
                Name = "Beca de Excelencia Académica",
                Description = "Apoyo económico de demostración para estudiantes con buen rendimiento académico.",
                Requirements = "Matrícula activa y promedio académico vigente.", Amount = 3500m,
                StartDate = today.AddMonths(-1), EndDate = today.AddMonths(2), Status = "Open"
            },
            new Scholarship
            {
                Name = "Beca de Transporte Universitario",
                Description = "Apoyo de demostración para cubrir traslados hacia la universidad.",
                Requirements = "Matrícula activa y residencia fuera del campus.", Amount = 1200m,
                StartDate = today.AddMonths(-1), EndDate = today.AddMonths(1), Status = "Open"
            },
            new Scholarship
            {
                Name = "Beca de Apoyo Socioeconómico",
                Description = "Convocatoria de demostración para estudiantes que solicitan apoyo socioeconómico.",
                Requirements = "Matrícula activa y solicitud con información socioeconómica.", Amount = 2500m,
                StartDate = today.AddMonths(-1), EndDate = today.AddMonths(3), Status = "Open"
            }
        };

        foreach (var scholarship in scholarships)
        {
            if (!await context.Scholarships.AnyAsync(item => item.Name == scholarship.Name))
                context.Scholarships.Add(scholarship);
        }
        await context.SaveChangesAsync();

        var demoStudents = await context.Students.Where(item => item.StudentCode.StartsWith("NBU-DEMO-"))
            .OrderBy(item => item.StudentCode).ToListAsync();
        var excellence = await context.Scholarships.SingleAsync(item => item.Name == "Beca de Excelencia Académica");
        var transport = await context.Scholarships.SingleAsync(item => item.Name == "Beca de Transporte Universitario");
        var socioEconomic = await context.Scholarships.SingleAsync(item => item.Name == "Beca de Apoyo Socioeconómico");

        await EnsureApplicationAsync(context, demoStudents[0].Id, excellence.Id, "Accepted", "Solicitud de demostración aceptada.");
        await EnsureApplicationAsync(context, demoStudents[0].Id, transport.Id, "Pending", "Solicitud de demostración en revisión.");
        await EnsureApplicationAsync(context, demoStudents[1].Id, socioEconomic.Id, "Pending", "Registro ficticio para mostrar el flujo de revisión.");
        await context.SaveChangesAsync();

        var acceptedApplication = await context.ScholarshipApplications.SingleAsync(item =>
            item.StudentId == demoStudents[0].Id && item.ScholarshipId == excellence.Id);
        if (!await context.Payments.AnyAsync(item => item.ScholarshipApplicationId == acceptedApplication.Id))
        {
            context.Payments.Add(new Payment
            {
                ScholarshipApplicationId = acceptedApplication.Id,
                Amount = excellence.Amount,
                DepositDate = today.AddDays(-14),
                Period = today.AddDays(-14).ToString("MMMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("es-NI")),
                Status = "Paid"
            });
        }

        if (!await context.Publications.AnyAsync(item => item.Title == "Convocatoria de becas NBU"))
        {
            context.Publications.Add(new Publication
            {
                Type = "News",
                Title = "Convocatoria de becas NBU",
                Content = "La plataforma presenta una convocatoria de ejemplo. El personal puede publicar aquí requisitos, fechas y avisos institucionales.",
                PublishedAt = DateTime.UtcNow.AddDays(-3)
            });
        }
        if (!await context.Publications.AnyAsync(item => item.Title == "Jornada universitaria de orientación"))
        {
            context.Publications.Add(new Publication
            {
                Type = "Event",
                Title = "Jornada universitaria de orientación",
                Content = "Evento de demostración para mostrar cómo se publican actividades y fechas universitarias.",
                PublishedAt = DateTime.UtcNow.AddDays(-1),
                EventDate = DateTime.Today.AddDays(12)
            });
        }
        await context.SaveChangesAsync();

        var announcement = await context.Publications.SingleAsync(item => item.Title == "Convocatoria de becas NBU");
        if (!await context.Notifications.AnyAsync(item => item.StudentId == demoStudents[0].Id && item.PublicationId == announcement.Id))
        {
            context.Notifications.Add(new Notification
            {
                StudentId = demoStudents[0].Id,
                PublicationId = announcement.Id,
                Message = "Hay una convocatoria de becas de demostración disponible.",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                IsRead = false
            });
        }
        if (!await context.Notifications.AnyAsync(item => item.StudentId == demoStudents[1].Id && item.PublicationId == announcement.Id))
        {
            context.Notifications.Add(new Notification
            {
                StudentId = demoStudents[1].Id,
                PublicationId = announcement.Id,
                Message = "Revisa los requisitos y las fechas de la convocatoria de demostración.",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                IsRead = false
            });
        }
        await context.SaveChangesAsync();

        await EnsureUserAsync(userManager, AdminEmail, AdminPassword, "Administración NBU", NbuRoles.Admin, null);
        await EnsureUserAsync(userManager, StudentEmail, StudentPassword, "Ana Méndez López", NbuRoles.Student, demoStudents[0].Id);
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole<int>> roleManager, string role)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var result = await roleManager.CreateAsync(new IdentityRole<int>(role));
            if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
        }
    }

    private static async Task EnsureApplicationAsync(AppDbContext context, int studentId, int scholarshipId, string status, string observations)
    {
        if (!await context.ScholarshipApplications.AnyAsync(item => item.StudentId == studentId && item.ScholarshipId == scholarshipId))
        {
            context.ScholarshipApplications.Add(new ScholarshipApplication
            {
                StudentId = studentId,
                ScholarshipId = scholarshipId,
                SubmittedAt = DateTime.UtcNow.AddDays(-7),
                Status = status,
                Observations = observations
            });
        }
    }

    private static async Task EnsureUserAsync(
        UserManager<NbuUser> userManager,
        string email,
        string password,
        string displayName,
        string role,
        int? studentId)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new NbuUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                LockoutEnabled = true,
                DisplayName = displayName,
                StudentId = studentId
            };
            var createResult = await userManager.CreateAsync(user, password);
            if (!createResult.Succeeded)
                throw new InvalidOperationException(string.Join("; ", createResult.Errors.Select(error => error.Description)));
        }
        else
        {
            var changed = !user.EmailConfirmed || !user.LockoutEnabled || user.DisplayName != displayName ||
                (studentId.HasValue && user.StudentId != studentId);
            user.EmailConfirmed = true;
            user.LockoutEnabled = true;
            user.DisplayName = displayName;
            if (studentId.HasValue) user.StudentId = studentId;
            if (changed)
            {
                var updateResult = await userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                    throw new InvalidOperationException(string.Join("; ", updateResult.Errors.Select(error => error.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var roleResult = await userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
                throw new InvalidOperationException(string.Join("; ", roleResult.Errors.Select(error => error.Description)));
        }
    }
}
