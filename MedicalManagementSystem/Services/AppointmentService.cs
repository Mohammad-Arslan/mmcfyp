using MedicalManagementSystem.Models;
using MedicalManagementSystem.Repositories;
using MedicalManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace MedicalManagementSystem.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IRepository<Appointment> _appointmentRepository;
    private readonly ApplicationDbContext _context;

    public AppointmentService(IRepository<Appointment> appointmentRepository, ApplicationDbContext context)
    {
        _appointmentRepository = appointmentRepository;
        _context = context;
    }

    public async Task<IEnumerable<Appointment>> GetAllAppointmentsAsync()
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.Nurse)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();
    }

    public async Task<Appointment?> GetAppointmentByIdAsync(int id)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Include(a => a.Nurse)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
    {
        if (string.IsNullOrEmpty(appointment.AppointmentNumber))
        {
            appointment.AppointmentNumber = await GenerateAppointmentNumberAsync();
        }
        return await _appointmentRepository.AddAsync(appointment);
    }

    public async Task UpdateAppointmentAsync(Appointment appointment)
    {
        appointment.UpdatedAt = DateTime.UtcNow;
        await _appointmentRepository.UpdateAsync(appointment);
    }

    public async Task DeleteAppointmentAsync(int id)
    {
        await _appointmentRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByDateAsync(DateTime date)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Where(a => a.AppointmentDate.Date == date.Date && a.IsActive)
            .OrderBy(a => a.AppointmentTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByPatientIdAsync(int patientId)
    {
        return await _context.Appointments
            .Include(a => a.Doctor)
            .Where(a => a.PatientId == patientId && a.IsActive)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetAppointmentsByDoctorIdAsync(int doctorId)
    {
        return await _context.Appointments
            .Include(a => a.Patient)
            .Where(a => a.DoctorId == doctorId && a.IsActive)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();
    }

    public async Task SendNotificationAsync(int appointmentId, string notificationType)
    {
        var appointment = await GetAppointmentByIdAsync(appointmentId);
        if (appointment == null) return;

        // TODO: Implement actual SMS/WhatsApp notification logic
        // This is a placeholder for future implementation
        if (notificationType.ToLower() == "sms")
        {
            appointment.SMSNotificationSent = true;
        }
        else if (notificationType.ToLower() == "whatsapp")
        {
            appointment.WhatsAppNotificationSent = true;
        }

        appointment.NotificationSentAt = DateTime.UtcNow;
        await UpdateAppointmentAsync(appointment);
    }

    private async Task<string> GenerateAppointmentNumberAsync()
    {
        var year = DateTime.Now.Year;
        var lastAppointment = await _context.Appointments
            .Where(a => a.AppointmentNumber.StartsWith($"APT{year}"))
            .OrderByDescending(a => a.AppointmentNumber)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastAppointment != null && !string.IsNullOrEmpty(lastAppointment.AppointmentNumber))
        {
            var parts = lastAppointment.AppointmentNumber.Split('-');
            if (parts.Length > 1 && int.TryParse(parts[1], out int lastSeq))
            {
                sequence = lastSeq + 1;
            }
        }

        return $"APT{year}-{sequence:D6}";
    }
}
