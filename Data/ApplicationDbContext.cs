using Claim_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Claim_System.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Claim> Claims { get; set; }
        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<Contractor> Contractors { get; set; }
        public DbSet<ClaimStatus> ClaimStatuses { get; set; }
        public DbSet<ModuleSchedule> ModuleSchedules { get; set; } // Added DbSet for ModuleSchedules

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the ClaimStatus entity
            modelBuilder.Entity<ClaimStatus>(entity =>
            {
                // Define primary key
                entity.HasKey(cs => cs.Id);

                // Configure foreign key relationship
                entity.HasOne(cs => cs.Claim)
                      .WithMany(c => c.ClaimStatuses) // Assuming a Claim has multiple statuses
                      .HasForeignKey(cs => cs.ClaimId)
                      .OnDelete(DeleteBehavior.Cascade); // Matches the "ON DELETE CASCADE" in your SQL schema

                // Additional configuration if needed
                entity.Property(cs => cs.Status)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(cs => cs.ContractorFeedback)
                      .HasMaxLength(1000);

                entity.Property(cs => cs.ContractorType)
                      .HasMaxLength(100);

                entity.Property(cs => cs.ContractorWorkCampus)
                      .HasMaxLength(100);

                // Configure the DateUpdated property to use the default value (GETDATE)
                entity.Property(cs => cs.DateUpdated)
                      .HasDefaultValueSql("GETDATE()"); // Matches the SQL default
            });

            // Configure the Contractor entity
            modelBuilder.Entity<Contractor>(entity =>
            {
                // Define ContractorEmail as the primary key
                entity.HasKey(c => c.ContractorEmail);

                // Configure ContractorPassword as required
                entity.Property(c => c.ContractorPassword)
                      .IsRequired()
                      .HasMaxLength(255); // Assuming a max length based on database constraints
            });

            // Configure the Lecturer entity
            modelBuilder.Entity<Lecturer>(entity =>
            {
                // Define LecturerEmail as the primary key
                entity.HasKey(l => l.LecturerEmail);

                // Configure LecturerPassword as required
                entity.Property(l => l.LecturerPassword)
                      .IsRequired()
                      .HasMaxLength(255); // Matches the database schema
            });

            // Configure the Claim entity to ensure it handles its related ClaimStatuses correctly
            modelBuilder.Entity<Claim>(entity =>
            {
                // Configure the ClaimStatuses relationship (already handled but adding clarity)
                entity.HasMany(c => c.ClaimStatuses)
                      .WithOne(cs => cs.Claim)
                      .HasForeignKey(cs => cs.ClaimId)
                      .OnDelete(DeleteBehavior.Cascade); // Cascade delete for claim statuses
            });

            // Configure the ModuleSchedule entity
            modelBuilder.Entity<ModuleSchedule>(entity =>
            {
                // Define primary key
                entity.HasKey(ms => ms.Id);

                // Configure foreign key relationship with Lecturer
                entity.HasOne<Lecturer>()
                      .WithMany()
                      .HasForeignKey(ms => ms.LecturerEmail)
                      .OnDelete(DeleteBehavior.Cascade);

                // Additional configuration for properties
                entity.Property(ms => ms.ModuleCode)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(ms => ms.ReminderDay)
                      .IsRequired()
                      .HasMaxLength(20); // Limiting the day name length
            });
        }
    }
}
