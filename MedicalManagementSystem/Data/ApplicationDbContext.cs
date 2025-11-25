using Microsoft.EntityFrameworkCore;
using MedicalManagementSystem.Models;

namespace MedicalManagementSystem.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Nurse> Nurses { get; set; }
    public DbSet<Procedure> Procedures { get; set; }
    public DbSet<LabTest> LabTests { get; set; }
    public DbSet<LabTestCategory> LabTestCategories { get; set; }
    public DbSet<LabStaff> LabStaffs { get; set; }
    public DbSet<Prescription> Prescriptions { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<DoctorSchedule> DoctorSchedules { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships and indexes
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Procedure>()
            .HasOne(p => p.Patient)
            .WithMany(pat => pat.Procedures)
            .HasForeignKey(p => p.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<LabTest>()
            .HasOne(l => l.Patient)
            .WithMany(p => p.LabTests)
            .HasForeignKey(l => l.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Patient)
            .WithMany(p => p.Transactions)
            .HasForeignKey(t => t.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for performance
        modelBuilder.Entity<Patient>()
            .HasIndex(p => p.MRNumber)
            .IsUnique();

        modelBuilder.Entity<Appointment>()
            .HasIndex(a => a.AppointmentNumber)
            .IsUnique();

        modelBuilder.Entity<Procedure>()
            .HasIndex(p => p.ProcedureNumber)
            .IsUnique();

        modelBuilder.Entity<LabTest>()
            .HasIndex(l => l.TestNumber)
            .IsUnique();

        modelBuilder.Entity<Transaction>()
            .HasIndex(t => t.TransactionNumber)
            .IsUnique();
    }
}
