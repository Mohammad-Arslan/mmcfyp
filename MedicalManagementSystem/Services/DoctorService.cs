using MedicalManagementSystem.Models;
using MedicalManagementSystem.Repositories;
using MedicalManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace MedicalManagementSystem.Services;

public class DoctorService : IDoctorService
{
    private readonly IRepository<Doctor> _doctorRepository;
    private readonly ApplicationDbContext _context;

    public DoctorService(IRepository<Doctor> doctorRepository, ApplicationDbContext context)
    {
        _doctorRepository = doctorRepository;
        _context = context;
    }

    public async Task<IEnumerable<Doctor>> GetAllDoctorsAsync()
    {
        return await _doctorRepository.GetAllAsync();
    }

    public async Task<Doctor?> GetDoctorByIdAsync(int id)
    {
        return await _doctorRepository.GetByIdAsync(id);
    }

    public async Task<Doctor> CreateDoctorAsync(Doctor doctor)
    {
        return await _doctorRepository.AddAsync(doctor);
    }

    public async Task UpdateDoctorAsync(Doctor doctor)
    {
        doctor.UpdatedAt = DateTime.UtcNow;
        await _doctorRepository.UpdateAsync(doctor);
    }

    public async Task DeleteDoctorAsync(int id)
    {
        await _doctorRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Doctor>> GetDoctorsBySpecializationAsync(string specialization)
    {
        return await _doctorRepository.FindAsync(d => d.Specialization == specialization && d.IsActive);
    }

    public async Task<decimal> GetDoctorTotalRevenueAsync(int doctorId)
    {
        var appointmentRevenue = await _context.Transactions
            .Include(t => t.Appointment)
            .Where(t => t.Appointment != null && t.Appointment.DoctorId == doctorId && t.Status == "Paid")
            .SumAsync(t => t.TotalAmount);

        var procedureRevenue = await _context.Transactions
            .Include(t => t.Procedure)
            .Where(t => t.Procedure != null && t.Procedure.DoctorId == doctorId && t.Status == "Paid")
            .SumAsync(t => t.TotalAmount);

        return appointmentRevenue + procedureRevenue;
    }

    public async Task<int> GetDoctorAppointmentCountAsync(int doctorId)
    {
        return await _context.Appointments
            .CountAsync(a => a.DoctorId == doctorId && a.IsActive);
    }
}
