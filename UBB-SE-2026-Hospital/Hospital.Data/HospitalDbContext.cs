using Hospital.Shared.Models;
using Hospital.Shared.Models.PatientEr;
using Hospital.Shared.Models.StaffPharmacy;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Data;

public class HospitalDbContext(DbContextOptions<HospitalDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Staff> Staff => Set<Staff>();

    public DbSet<Item> Items => Set<Item>();

    public DbSet<Patient> Patients => Set<Patient>();

    public DbSet<Prescription> Prescriptions => Set<Prescription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasIndex(user => user.Email).IsUnique();
        modelBuilder.Entity<Staff>().HasKey(staff => staff.StaffId);
        modelBuilder.Entity<Patient>().HasKey(patient => patient.PatientId);
        modelBuilder.Entity<Prescription>().HasKey(prescription => prescription.PrescriptionId);
    }
}
