namespace MedicalManagementSystem.Models;

public class LabTestCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public virtual ICollection<LabTest> LabTests { get; set; } = new List<LabTest>();
}
