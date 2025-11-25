using MedicalManagementSystem.Models;

namespace MedicalManagementSystem.Services;

public interface ITransactionService
{
    Task<IEnumerable<Transaction>> GetAllTransactionsAsync();
    Task<Transaction?> GetTransactionByIdAsync(int id);
    Task<Transaction> CreateTransactionAsync(Transaction transaction);
    Task UpdateTransactionAsync(Transaction transaction);
    Task DeleteTransactionAsync(int id);
    Task<IEnumerable<Transaction>> GetTransactionsByPatientIdAsync(int patientId);
    Task<string> GenerateInvoiceNumberAsync();
    Task<string> GenerateTransactionNumberAsync();
}
