using MedicalManagementSystem.Models;

namespace MedicalManagementSystem.Services;

public interface IPatientService
{
    Task<IEnumerable<Patient>> GetAllPatientsAsync();
    Task<Patient?> GetPatientByIdAsync(int id);
    Task<Patient?> GetPatientByMRNumberAsync(string mrNumber);
    Task<Patient> CreatePatientAsync(Patient patient);
    Task UpdatePatientAsync(Patient patient);
    Task DeletePatientAsync(int id);
    Task<string> GenerateMRNumberAsync();
}
