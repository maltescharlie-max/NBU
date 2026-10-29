using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using NBU.Data.Models;

namespace NBU.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<NbuUser, IdentityRole<int>, int>(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Scholarship> Scholarships => Set<Scholarship>();
    public DbSet<ScholarshipApplication> ScholarshipApplications => Set<ScholarshipApplication>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Publication> Publications => Set<Publication>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<NbuUser>(entity =>
        {
            entity.Property(user => user.DisplayName).HasMaxLength(120).IsRequired();
            entity.HasIndex(user => user.StudentId).IsUnique();
            entity.HasOne(user => user.Student)
                .WithOne()
                .HasForeignKey<NbuUser>(user => user.StudentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasIndex(student => student.StudentCode).IsUnique();
            entity.HasIndex(student => student.NationalId).IsUnique();
            entity.HasIndex(student => student.Email).IsUnique();
            entity.Property(student => student.Status).HasMaxLength(30);
        });

        modelBuilder.Entity<Scholarship>(entity =>
        {
            entity.Property(scholarship => scholarship.Amount).HasPrecision(12, 2);
            entity.Property(scholarship => scholarship.Status).HasMaxLength(30);
        });

        modelBuilder.Entity<ScholarshipApplication>(entity =>
        {
            entity.Property(application => application.Status).HasMaxLength(30);
            entity.HasIndex(application => new { application.StudentId, application.ScholarshipId }).IsUnique();
            entity.HasOne(application => application.Student)
                .WithMany(student => student.Applications)
                .HasForeignKey(application => application.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(application => application.Scholarship)
                .WithMany(scholarship => scholarship.Applications)
                .HasForeignKey(application => application.ScholarshipId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(payment => payment.Amount).HasPrecision(12, 2);
            entity.Property(payment => payment.Period).HasMaxLength(50);
            entity.Property(payment => payment.Status).HasMaxLength(30);
            entity.HasOne(payment => payment.ScholarshipApplication)
                .WithMany(application => application.Payments)
                .HasForeignKey(payment => payment.ScholarshipApplicationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Publication>(entity =>
        {
            entity.Property(publication => publication.Type).HasMaxLength(30);
            entity.Property(publication => publication.Title).HasMaxLength(200);
            entity.Property(publication => publication.Content).HasMaxLength(5000);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.Property(notification => notification.Message).HasMaxLength(500);
            entity.HasOne(notification => notification.Student)
                .WithMany(student => student.Notifications)
                .HasForeignKey(notification => notification.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(notification => notification.Publication)
                .WithMany(publication => publication.Notifications)
                .HasForeignKey(notification => notification.PublicationId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
