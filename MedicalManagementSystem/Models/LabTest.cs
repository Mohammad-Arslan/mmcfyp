namespace MedicalManagementSystem.Models;

public class LabTest : BaseEntity
{
    public string TestNumber { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public int? ProcedureId { get; set; }
    public int LabTestCategoryId { get; set; }
    public int? AssignedToStaffId { get; set; }
    public string TestName { get; set; } = string.Empty;
    public DateTime TestDate { get; set; }
    public DateTime? SampleCollectionDate { get; set; }
    public DateTime? ReportDate { get; set; }
    public string Status { get; set; } = "Booked";
    public string ReportFilePath { get; set; } = string.Empty;
    public string ReportText { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public bool InvoiceGenerated { get; set; }

    public virtual Patient Patient { get; set; } = null!;
    public virtual Procedure? Procedure { get; set; }
    public virtual LabTestCategory LabTestCategory { get; set; } = null!;
    public virtual LabStaff? AssignedToStaff { get; set; }
}
