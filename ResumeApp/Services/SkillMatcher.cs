using Microsoft.EntityFrameworkCore;
using ResumeApp.Data;

namespace ResumeApp.Services
{
    public class SkillMatcher
    {
        private readonly ApplicationDbContext _context;

        public SkillMatcher(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<string>> MatchSkillsAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            text = text.ToLower();

            var skills = await _context.Skills
                .Where(s => s.IsActive)
                .Select(s => s.Name.ToLower())
                .ToListAsync();

            return skills
                .Where(skill => text.Contains(skill))
                .Distinct()
                .ToList();
        }
    }
}
