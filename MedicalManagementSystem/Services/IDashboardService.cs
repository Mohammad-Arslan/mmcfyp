namespace MedicalManagementSystem.Services;

public interface IDashboardService
{
    Task<int> GetTotalAppointmentsAsync();
    Task<int> GetTotalPatientsAsync();
    Task<int> GetTotalProceduresAsync();
    Task<int> GetTotalLabReportsAsync();
    Task<decimal> GetTotalRevenueAsync();
    Task<decimal> GetMonthlyRevenueAsync(int month, int year);
    Task<IEnumerable<object>> GetDailyAppointmentsAsync(DateTime date);
    Task<Dictionary<string, int>> GetMonthlyStatisticsAsync(int month, int year);
}
