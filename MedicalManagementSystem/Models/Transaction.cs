namespace MedicalManagementSystem.Models;

public class Transaction : BaseEntity
{
    public string TransactionNumber { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public int? AppointmentId { get; set; }
    public int? ProcedureId { get; set; }
    public int? LabTestId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string PaymentMode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal? Discount { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Status { get; set; } = "Pending";
    public string Notes { get; set; } = string.Empty;
    public bool PaymentConfirmationSent { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;

    public virtual Patient Patient { get; set; } = null!;
    public virtual Appointment? Appointment { get; set; }
    public virtual Procedure? Procedure { get; set; }
    public virtual LabTest? LabTest { get; set; }
}
