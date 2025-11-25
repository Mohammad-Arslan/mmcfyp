using MedicalManagementSystem.Models;

namespace MedicalManagementSystem.Services;

public interface IProcedureService
{
    Task<IEnumerable<Procedure>> GetAllProceduresAsync();
    Task<Procedure?> GetProcedureByIdAsync(int id);
    Task<Procedure> CreateProcedureAsync(Procedure procedure);
    Task UpdateProcedureAsync(Procedure procedure);
    Task DeleteProcedureAsync(int id);
    Task<IEnumerable<Procedure>> GetProceduresByPatientIdAsync(int patientId);
    Task<IEnumerable<Procedure>> GetProceduresByDoctorIdAsync(int doctorId);
    Task<string> GenerateProcedureNumberAsync();
}
