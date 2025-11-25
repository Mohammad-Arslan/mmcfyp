namespace MedicalManagementSystem.Models;

public class Procedure : BaseEntity
{
    public string ProcedureNumber { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public int? DoctorId { get; set; }
    public int? NurseId { get; set; }
    public string ProcedureType { get; set; } = string.Empty;
    public string ProcedureName { get; set; } = string.Empty;
    public DateTime ProcedureDate { get; set; }
    public TimeSpan? ProcedureTime { get; set; }
    public string TreatmentNotes { get; set; } = string.Empty;
    public string Status { get; set; } = "Scheduled";
    public decimal Cost { get; set; }
    public bool InvoiceGenerated { get; set; }

    public virtual Patient Patient { get; set; } = null!;
    public virtual Doctor? Doctor { get; set; }
    public virtual Nurse? Nurse { get; set; }
    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    public virtual ICollection<LabTest> LabTests { get; set; } = new List<LabTest>();
}
