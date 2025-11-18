using Microsoft.EntityFrameworkCore;
using ContractReviewScheduler.Models.Domain;

namespace ContractReviewScheduler.Data
{
    /// <summary>
    /// 應用程式資料庫上下文
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<LeaveSchedule> LeaveSchedules { get; set; } = null!;
        public DbSet<AppointmentHistory> AppointmentHistories { get; set; } = null!;
        public DbSet<NotificationLog> NotificationLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User 配置
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.AdAccount).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.AdAccount).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Role).HasMaxLength(50).IsRequired();
            });

            // Appointment 配置
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.ReviewerId, e.Date, e.TimeStart, e.TimeEnd });
                entity.HasIndex(e => e.ApplicantId);
                entity.HasIndex(e => e.Status);
                entity.Property(e => e.ObjectName).HasMaxLength(500).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
                entity.Property(e => e.DelegateStatus).HasMaxLength(50);

                // Foreign key relationships
                entity.HasOne(e => e.Applicant)
                    .WithMany(u => u.ApplicantAppointments)
                    .HasForeignKey(e => e.ApplicantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Reviewer)
                    .WithMany(u => u.ReviewerAppointments)
                    .HasForeignKey(e => e.ReviewerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.DelegateReviewer)
                    .WithMany()
                    .HasForeignKey(e => e.DelegateReviewerId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // LeaveSchedule 配置
            modelBuilder.Entity<LeaveSchedule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.ReviewerId, e.Date, e.TimeStart, e.TimeEnd });
                
                entity.HasOne(e => e.Reviewer)
                    .WithMany(u => u.LeaveSchedules)
                    .HasForeignKey(e => e.ReviewerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // AppointmentHistory 配置
            modelBuilder.Entity<AppointmentHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.AppointmentId);
                entity.Property(e => e.Action).HasMaxLength(100).IsRequired();

                entity.HasOne(e => e.Appointment)
                    .WithMany(a => a.Histories)
                    .HasForeignKey(e => e.AppointmentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Actor)
                    .WithMany()
                    .HasForeignKey(e => e.ActorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // NotificationLog 配置
            modelBuilder.Entity<NotificationLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.AppointmentId, e.Status });
                entity.Property(e => e.RecipientEmail).HasMaxLength(255).IsRequired();
                entity.Property(e => e.NotificationType).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Subject).HasMaxLength(500).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(50).IsRequired();

                entity.HasOne(e => e.Appointment)
                    .WithMany(a => a.NotificationLogs)
                    .HasForeignKey(e => e.AppointmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
