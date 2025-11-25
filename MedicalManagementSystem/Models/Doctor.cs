using System.ComponentModel.DataAnnotations;

namespace MedicalManagementSystem.Models;

public class Doctor : BaseEntity
{
    [Required(ErrorMessage = "First name is required")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Last name is required")]
    public string LastName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Specialization is required")]
    public string Specialization { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Phone number is required")]
    public string Phone { get; set; } = string.Empty;
    
    public string Address { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Qualification is required")]
    public string Qualification { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "License number is required")]
    public string LicenseNumber { get; set; } = string.Empty;
    
    public DateTime? DateOfBirth { get; set; }
    
    [Required(ErrorMessage = "Gender is required")]
    public string Gender { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Consultation fee is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Consultation fee must be greater than or equal to 0")]
    public decimal ConsultationFee { get; set; }
    
    public string Status { get; set; } = "Active";

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public virtual ICollection<Procedure> Procedures { get; set; } = new List<Procedure>();
    public virtual ICollection<DoctorSchedule> Schedules { get; set; } = new List<DoctorSchedule>();

    public string FullName => $"{FirstName} {LastName}";
}
