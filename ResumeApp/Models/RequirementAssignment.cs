namespace ResumeApp.Models
{
    public class RequirementAssignment
    {
        public int Id { get; set; }

        public int RequirementId { get; set; }
        public ClientRequirement Requirement { get; set; }

        public string UserId { get; set; }
        public Users User { get; set; }

        public DateTime AssignedDate { get; set; } = DateTime.Now;
    }
}
