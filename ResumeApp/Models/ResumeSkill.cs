using System.ComponentModel.DataAnnotations;

namespace ResumeApp.Models
{
    public class ResumeSkill
    {
        public int Id { get; set; }
        public int ResumeId { get; set; }

        [MaxLength(150)]
        public string SkillName { get; set; }

        public Resume Resume { get; set; }
    }
}
