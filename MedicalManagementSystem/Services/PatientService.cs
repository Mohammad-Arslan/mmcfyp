using MedicalManagementSystem.Models;
using MedicalManagementSystem.Repositories;
using MedicalManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace MedicalManagementSystem.Services;

public class PatientService : IPatientService
{
    private readonly IRepository<Patient> _patientRepository;
    private readonly ApplicationDbContext _context;

    public PatientService(IRepository<Patient> patientRepository, ApplicationDbContext context)
    {
        _patientRepository = patientRepository;
        _context = context;
    }

    public async Task<IEnumerable<Patient>> GetAllPatientsAsync()
    {
        return await _patientRepository.GetAllAsync();
    }

    public async Task<Patient?> GetPatientByIdAsync(int id)
    {
        return await _context.Patients
            .Include(p => p.Appointments)
            .Include(p => p.Procedures)
            .Include(p => p.LabTests)
            .Include(p => p.Prescriptions)
            .Include(p => p.Transactions)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Patient?> GetPatientByMRNumberAsync(string mrNumber)
    {
        return await _patientRepository.FirstOrDefaultAsync(p => p.MRNumber == mrNumber);
    }

    public async Task<Patient> CreatePatientAsync(Patient patient)
    {
        if (string.IsNullOrEmpty(patient.MRNumber))
        {
            patient.MRNumber = await GenerateMRNumberAsync();
        }
        return await _patientRepository.AddAsync(patient);
    }

    public async Task UpdatePatientAsync(Patient patient)
    {
        patient.UpdatedAt = DateTime.UtcNow;
        await _patientRepository.UpdateAsync(patient);
    }

    public async Task DeletePatientAsync(int id)
    {
        await _patientRepository.DeleteAsync(id);
    }

    public async Task<string> GenerateMRNumberAsync()
    {
        var year = DateTime.Now.Year;
        var lastPatient = await _context.Patients
            .Where(p => p.MRNumber.StartsWith($"MR{year}"))
            .OrderByDescending(p => p.MRNumber)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastPatient != null && !string.IsNullOrEmpty(lastPatient.MRNumber))
        {
            var parts = lastPatient.MRNumber.Split('-');
            if (parts.Length > 1 && int.TryParse(parts[1], out int lastSeq))
            {
                sequence = lastSeq + 1;
            }
        }

        return $"MR{year}-{sequence:D6}";
    }
}
