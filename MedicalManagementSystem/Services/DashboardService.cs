using MedicalManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace MedicalManagementSystem.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetTotalAppointmentsAsync()
    {
        return await _context.Appointments.CountAsync(a => a.IsActive);
    }

    public async Task<int> GetTotalPatientsAsync()
    {
        return await _context.Patients.CountAsync(p => p.IsActive);
    }

    public async Task<int> GetTotalProceduresAsync()
    {
        return await _context.Procedures.CountAsync(p => p.IsActive);
    }

    public async Task<int> GetTotalLabReportsAsync()
    {
        return await _context.LabTests.CountAsync(l => l.IsActive && l.Status == "Completed");
    }

    public async Task<decimal> GetTotalRevenueAsync()
    {
        return await _context.Transactions
            .Where(t => t.Status == "Paid" && t.IsActive)
            .SumAsync(t => t.TotalAmount);
    }

    public async Task<decimal> GetMonthlyRevenueAsync(int month, int year)
    {
        return await _context.Transactions
            .Where(t => t.TransactionDate.Month == month && 
                       t.TransactionDate.Year == year && 
                       t.Status == "Paid" && 
                       t.IsActive)
            .SumAsync(t => t.TotalAmount);
    }

    public async Task<IEnumerable<object>> GetDailyAppointmentsAsync(DateTime date)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Where(a => a.AppointmentDate.Date == date.Date && a.IsActive)
            .OrderBy(a => a.AppointmentTime)
            .Select(a => new
            {
                a.Id,
                a.AppointmentNumber,
                PatientName = a.Patient.FullName,
                DoctorName = a.Doctor != null ? a.Doctor.FullName : "Not Assigned",
                a.AppointmentTime,
                a.Status,
                a.AppointmentType
            })
            .ToListAsync();
    }

    public async Task<Dictionary<string, int>> GetMonthlyStatisticsAsync(int month, int year)
    {
        var stats = new Dictionary<string, int>
        {
            ["Appointments"] = await _context.Appointments
                .CountAsync(a => a.AppointmentDate.Month == month && 
                                a.AppointmentDate.Year == year && 
                                a.IsActive),
            ["Patients"] = await _context.Patients
                .CountAsync(p => p.CreatedAt.Month == month && 
                                p.CreatedAt.Year == year && 
                                p.IsActive),
            ["Procedures"] = await _context.Procedures
                .CountAsync(p => p.ProcedureDate.Month == month && 
                                p.ProcedureDate.Year == year && 
                                p.IsActive),
            ["LabTests"] = await _context.LabTests
                .CountAsync(l => l.TestDate.Month == month && 
                                l.TestDate.Year == year && 
                                l.IsActive)
        };

        return stats;
    }
}
