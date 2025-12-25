namespace ResumeApp.Models
{
    public class RequirementSkill
    {
        public int Id { get; set; }

        // FK → ClientRequirement
        public int RequirementId { get; set; }
        public ClientRequirement Requirement { get; set; } = null!;

        // FK → Skill
        public int SkillId { get; set; }
        public Skill Skill { get; set; } = null!;

        // true = Primary, false = Secondary
        public bool IsPrimary { get; set; }
    }
}
