using NBU.Data.Models;

namespace NBU.Web.Models;

public sealed record StudentDashboardViewModel(
    Student Student,
    int OpenScholarships,
    int PendingApplications,
    int UnreadNotifications,
    decimal PaidTotal,
    IReadOnlyList<ScholarshipApplication> Applications,
    IReadOnlyList<Notification> Notifications);

public sealed record StudentScholarshipItem(Scholarship Scholarship, string? ApplicationStatus);

public sealed record StudentApplicationsViewModel(Student Student, IReadOnlyList<ScholarshipApplication> Applications);

public sealed record StudentPaymentsViewModel(Student Student, IReadOnlyList<Payment> Payments, decimal PaidTotal);

public sealed record StudentPublicationsViewModel(Student Student, IReadOnlyList<Publication> Publications);

public sealed record StudentNotificationsViewModel(Student Student, IReadOnlyList<Notification> Notifications);
