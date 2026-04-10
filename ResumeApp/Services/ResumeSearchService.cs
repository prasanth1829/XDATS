using Microsoft.EntityFrameworkCore;
using ResumeApp.Data;

namespace ResumeApp.Services
{
    public class ResumeSearchService:IResumeSearchService
    {
        private readonly ApplicationDbContext _context;

        public ResumeSearchService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<ResumeSearchResult> BuildSearchQuery(string? search, int? minExperience, string? mandatorySkills,string? optionalSkills)
        {
            var query = _context.Resumes
                .AsNoTracking()
                .AsQueryable();

            if (minExperience.HasValue)
                query = query.Where(r => r.YearsOfExperience >= minExperience.Value);

            var mandatory = SkillParser.Parse(mandatorySkills);

            if (mandatory.Any())
            {
                foreach (var skill in mandatory)
                {
                    var s = skill.ToLower();

                    query = query.Where(r =>
                        _context.ResumeSkills.Any(x =>
                            x.ResumeId == r.Id &&
                            x.SkillName.ToLower() == s
                        )
                        ||
                        EF.Functions.Like(r.ExtractedText.ToLower(), "%" + s + "%")
                    );
                }
            }

            var optional = SkillParser.Parse(optionalSkills);

            

            var finalQuery = query.Select(r => new ResumeSearchResult
            {
                Resume = r,
                OptionalScore = 0
            });

            if (optional.Any())
            {
                finalQuery = finalQuery.Select(x => new ResumeSearchResult
                {
                    Resume = x.Resume,

                    OptionalScore =
                        _context.ResumeSkills
                            .Where(s => s.ResumeId == x.Resume.Id && optional.Contains(s.SkillName.ToLower()))
                            .Count()
                });
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = $"%{search.ToLower()}%";

                finalQuery = finalQuery.Where(x =>
                    EF.Functions.Like((x.Resume.Name ?? "").ToLower(), term) ||
                    EF.Functions.Like((x.Resume.Email ?? "").ToLower(), term) ||
                    EF.Functions.Like((x.Resume.Phone ?? "").ToLower(), term) ||
                    EF.Functions.Like((x.Resume.Skills ?? "").ToLower(), term) ||
                    EF.Functions.Like((x.Resume.ExtractedText ?? "").ToLower(), term)
                );
            }

            return finalQuery;
        }

    }
}
