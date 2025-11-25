using MedicalManagementSystem.Models;
using MedicalManagementSystem.Repositories;
using MedicalManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace MedicalManagementSystem.Services;

public class ProcedureService : IProcedureService
{
    private readonly IRepository<Procedure> _procedureRepository;
    private readonly ApplicationDbContext _context;

    public ProcedureService(IRepository<Procedure> procedureRepository, ApplicationDbContext context)
    {
        _procedureRepository = procedureRepository;
        _context = context;
    }

    public async Task<IEnumerable<Procedure>> GetAllProceduresAsync()
    {
        return await _context.Procedures
            .Include(p => p.Patient)
            .Include(p => p.Doctor)
            .Include(p => p.Nurse)
            .OrderByDescending(p => p.ProcedureDate)
            .ToListAsync();
    }

    public async Task<Procedure?> GetProcedureByIdAsync(int id)
    {
        return await _context.Procedures
            .Include(p => p.Patient)
            .Include(p => p.Doctor)
            .Include(p => p.Nurse)
            .Include(p => p.Prescriptions)
            .Include(p => p.LabTests)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Procedure> CreateProcedureAsync(Procedure procedure)
    {
        if (string.IsNullOrEmpty(procedure.ProcedureNumber))
        {
            procedure.ProcedureNumber = await GenerateProcedureNumberAsync();
        }
        return await _procedureRepository.AddAsync(procedure);
    }

    public async Task UpdateProcedureAsync(Procedure procedure)
    {
        procedure.UpdatedAt = DateTime.UtcNow;
        await _procedureRepository.UpdateAsync(procedure);
    }

    public async Task DeleteProcedureAsync(int id)
    {
        await _procedureRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Procedure>> GetProceduresByPatientIdAsync(int patientId)
    {
        return await _context.Procedures
            .Include(p => p.Doctor)
            .Where(p => p.PatientId == patientId && p.IsActive)
            .OrderByDescending(p => p.ProcedureDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Procedure>> GetProceduresByDoctorIdAsync(int doctorId)
    {
        return await _context.Procedures
            .Include(p => p.Patient)
            .Where(p => p.DoctorId == doctorId && p.IsActive)
            .OrderByDescending(p => p.ProcedureDate)
            .ToListAsync();
    }

    public async Task<string> GenerateProcedureNumberAsync()
    {
        var year = DateTime.Now.Year;
        var lastProcedure = await _context.Procedures
            .Where(p => p.ProcedureNumber.StartsWith($"PROC{year}"))
            .OrderByDescending(p => p.ProcedureNumber)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastProcedure != null && !string.IsNullOrEmpty(lastProcedure.ProcedureNumber))
        {
            var parts = lastProcedure.ProcedureNumber.Split('-');
            if (parts.Length > 1 && int.TryParse(parts[1], out int lastSeq))
            {
                sequence = lastSeq + 1;
            }
        }

        return $"PROC{year}-{sequence:D6}";
    }
}
