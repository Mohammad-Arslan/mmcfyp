namespace MedicalManagementSystem.Models;

public class Appointment : BaseEntity
{
    public string AppointmentNumber { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public int? DoctorId { get; set; }
    public int? NurseId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public string AppointmentType { get; set; } = string.Empty;
    public string Status { get; set; } = "Scheduled";
    public string Reason { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public bool SMSNotificationSent { get; set; }
    public bool WhatsAppNotificationSent { get; set; }
    public DateTime? NotificationSentAt { get; set; }

    public virtual Patient Patient { get; set; } = null!;
    public virtual Doctor? Doctor { get; set; }
    public virtual Nurse? Nurse { get; set; }
}
