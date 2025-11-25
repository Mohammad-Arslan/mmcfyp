namespace MedicalManagementSystem.Models;

public class LabStaff : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";

    public virtual ICollection<LabTest> LabTests { get; set; } = new List<LabTest>();

    public string FullName => $"{FirstName} {LastName}";
}
