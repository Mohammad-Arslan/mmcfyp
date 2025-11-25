using MedicalManagementSystem.Models;
using MedicalManagementSystem.Repositories;
using MedicalManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace MedicalManagementSystem.Services;

public class LabTestService : ILabTestService
{
    private readonly IRepository<LabTest> _labTestRepository;
    private readonly ApplicationDbContext _context;

    public LabTestService(IRepository<LabTest> labTestRepository, ApplicationDbContext context)
    {
        _labTestRepository = labTestRepository;
        _context = context;
    }

    public async Task<IEnumerable<LabTest>> GetAllLabTestsAsync()
    {
        return await _context.LabTests
            .Include(l => l.Patient)
            .Include(l => l.LabTestCategory)
            .Include(l => l.AssignedToStaff)
            .Include(l => l.Procedure)
            .OrderByDescending(l => l.TestDate)
            .ToListAsync();
    }

    public async Task<LabTest?> GetLabTestByIdAsync(int id)
    {
        return await _context.LabTests
            .Include(l => l.Patient)
            .Include(l => l.LabTestCategory)
            .Include(l => l.AssignedToStaff)
            .Include(l => l.Procedure)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<LabTest> CreateLabTestAsync(LabTest labTest)
    {
        if (string.IsNullOrEmpty(labTest.TestNumber))
        {
            labTest.TestNumber = await GenerateTestNumberAsync();
        }
        return await _labTestRepository.AddAsync(labTest);
    }

    public async Task UpdateLabTestAsync(LabTest labTest)
    {
        labTest.UpdatedAt = DateTime.UtcNow;
        await _labTestRepository.UpdateAsync(labTest);
    }

    public async Task DeleteLabTestAsync(int id)
    {
        await _labTestRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<LabTest>> GetLabTestsByPatientIdAsync(int patientId)
    {
        return await _context.LabTests
            .Include(l => l.LabTestCategory)
            .Where(l => l.PatientId == patientId && l.IsActive)
            .OrderByDescending(l => l.TestDate)
            .ToListAsync();
    }

    public async Task<string> GenerateTestNumberAsync()
    {
        var year = DateTime.Now.Year;
        var lastTest = await _context.LabTests
            .Where(l => l.TestNumber.StartsWith($"LAB{year}"))
            .OrderByDescending(l => l.TestNumber)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastTest != null && !string.IsNullOrEmpty(lastTest.TestNumber))
        {
            var parts = lastTest.TestNumber.Split('-');
            if (parts.Length > 1 && int.TryParse(parts[1], out int lastSeq))
            {
                sequence = lastSeq + 1;
            }
        }

        return $"LAB{year}-{sequence:D6}";
    }
}
