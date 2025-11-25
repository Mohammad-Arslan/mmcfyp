namespace MedicalManagementSystem.Models;

public class Prescription : BaseEntity
{
    public string PrescriptionNumber { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public int? ProcedureId { get; set; }
    public int? DoctorId { get; set; }
    public DateTime PrescriptionDate { get; set; }
    public string Medications { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public virtual Patient Patient { get; set; } = null!;
    public virtual Procedure? Procedure { get; set; }
    public virtual Doctor? Doctor { get; set; }
}
