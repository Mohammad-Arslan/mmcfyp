using MedicalManagementSystem.Models;

namespace MedicalManagementSystem.Services;

public interface ILabTestService
{
    Task<IEnumerable<LabTest>> GetAllLabTestsAsync();
    Task<LabTest?> GetLabTestByIdAsync(int id);
    Task<LabTest> CreateLabTestAsync(LabTest labTest);
    Task UpdateLabTestAsync(LabTest labTest);
    Task DeleteLabTestAsync(int id);
    Task<IEnumerable<LabTest>> GetLabTestsByPatientIdAsync(int patientId);
    Task<string> GenerateTestNumberAsync();
}
