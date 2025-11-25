using MedicalManagementSystem.Models;
using MedicalManagementSystem.Repositories;
using MedicalManagementSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace MedicalManagementSystem.Services;

public class TransactionService : ITransactionService
{
    private readonly IRepository<Transaction> _transactionRepository;
    private readonly ApplicationDbContext _context;

    public TransactionService(IRepository<Transaction> transactionRepository, ApplicationDbContext context)
    {
        _transactionRepository = transactionRepository;
        _context = context;
    }

    public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
    {
        return await _context.Transactions
            .Include(t => t.Patient)
            .Include(t => t.Appointment)
            .Include(t => t.Procedure)
            .Include(t => t.LabTest)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<Transaction?> GetTransactionByIdAsync(int id)
    {
        return await _context.Transactions
            .Include(t => t.Patient)
            .Include(t => t.Appointment)
            .Include(t => t.Procedure)
            .Include(t => t.LabTest)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
    {
        if (string.IsNullOrEmpty(transaction.TransactionNumber))
        {
            transaction.TransactionNumber = await GenerateTransactionNumberAsync();
        }
        if (string.IsNullOrEmpty(transaction.InvoiceNumber))
        {
            transaction.InvoiceNumber = await GenerateInvoiceNumberAsync();
        }
        transaction.TotalAmount = transaction.Amount - (transaction.Discount ?? 0);
        return await _transactionRepository.AddAsync(transaction);
    }

    public async Task UpdateTransactionAsync(Transaction transaction)
    {
        transaction.UpdatedAt = DateTime.UtcNow;
        transaction.TotalAmount = transaction.Amount - (transaction.Discount ?? 0);
        await _transactionRepository.UpdateAsync(transaction);
    }

    public async Task DeleteTransactionAsync(int id)
    {
        await _transactionRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByPatientIdAsync(int patientId)
    {
        return await _context.Transactions
            .Where(t => t.PatientId == patientId && t.IsActive)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    public async Task<string> GenerateInvoiceNumberAsync()
    {
        var year = DateTime.Now.Year;
        var lastInvoice = await _context.Transactions
            .Where(t => t.InvoiceNumber.StartsWith($"INV{year}"))
            .OrderByDescending(t => t.InvoiceNumber)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastInvoice != null && !string.IsNullOrEmpty(lastInvoice.InvoiceNumber))
        {
            var parts = lastInvoice.InvoiceNumber.Split('-');
            if (parts.Length > 1 && int.TryParse(parts[1], out int lastSeq))
            {
                sequence = lastSeq + 1;
            }
        }

        return $"INV{year}-{sequence:D6}";
    }

    public async Task<string> GenerateTransactionNumberAsync()
    {
        var year = DateTime.Now.Year;
        var lastTransaction = await _context.Transactions
            .Where(t => t.TransactionNumber.StartsWith($"TXN{year}"))
            .OrderByDescending(t => t.TransactionNumber)
            .FirstOrDefaultAsync();

        int sequence = 1;
        if (lastTransaction != null && !string.IsNullOrEmpty(lastTransaction.TransactionNumber))
        {
            var parts = lastTransaction.TransactionNumber.Split('-');
            if (parts.Length > 1 && int.TryParse(parts[1], out int lastSeq))
            {
                sequence = lastSeq + 1;
            }
        }

        return $"TXN{year}-{sequence:D6}";
    }
}
