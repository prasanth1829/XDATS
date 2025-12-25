using ResumeApp.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ResumeApp.Services
{
    public class MatchScoringService: IMatchScoringService
    {
        // Default weights
        private const int MustWeight = 40;     
        private const int PrimaryWeight = 40;  
        private const int SecondaryWeight = 20;
        private const int ExpBonusMax = 10;

        // Simple synonyms map
        private static readonly Dictionary<string, string[]> Synonyms =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["c#"] = new[] { "c sharp", "csharp" },
                [".net"] = new[] { "dotnet", "asp.net", "aspnet", "net core", ".net core" },
                ["asp.net core"] = new[] { "asp net core", "web api", "rest api" },
                ["javascript"] = new[] { "js", "vanilla js" },
                ["typescript"] = new[] { "ts" },
                ["sql server"] = new[] { "mssql", "t-sql", "tsql" },
                ["git"] = new[] { "github", "gitlab" },
                ["azure"] = new[] { "azure devops", "az devops" },
                ["react"] = new[] { "reactjs", "react.js" },
                ["angular"] = new[] { "angularjs" },
                ["node.js"] = new[] { "node", "nodejs" },
                ["docker"] = new[] { "container", "containers" },
                ["kubernetes"] = new[] { "k8s" },
                ["java"]= new[] { "jdk" },
                ["python"] = new[] { "py" },
                ["linux"] = new[] { "unix" },
                ["aws"] = new[] { "amazon web services" },
                ["rest"] = new[] { "restful" },
                ["ci/cd"] = new[] { "continuous integration", "continuous delivery", "continuous deployment" },
                ["html"] = new[] { "html5" },
                ["css"] = new[] { "css3" },
                ["agile"] = new[] { "scrum", "kanban" }
            };

        public MatchResult Compute(ClientRequirement jd, Resume resume)
        {
            //Normalize JD skills
            var must = SplitSkills(jd.SkillsRequired);
            var primary = SplitSkills(jd.SkillsPrimary);
            var secondary = SplitSkills(jd.SkillsSecondary);

            // Build resume searchable text (skills + experience)
            var resumeText = $"{resume.Skills} {resume.Experience}".ToLowerInvariant();

            // Check matches
            var matchedMust = new List<string>();
            var missingMust = new List<string>();
            foreach (var s in must)
                (ContainsSkill(resumeText, s) ? matchedMust : missingMust).Add(s);

            var (matchedPrimary, missingPrimary) = MatchSet(resumeText, primary);
            var (matchedSecondary, missingSecondary) = MatchSet(resumeText, secondary);

            //Score parts
            int score = 0;

            //if any missing, cap total to 60 later
            var mustPart = must.Count == 0 ? MustWeight : (int)Math.Round(MustWeight * (matchedMust.Count / (double)must.Count));
            score += mustPart;

            //Primary: proportional
            var primaryPart = primary.Count == 0 ? 0 : (int)Math.Round(PrimaryWeight * (matchedPrimary.Count / (double)primary.Count));
            score += primaryPart;

            //Secondary: proportional
            var secondaryPart = secondary.Count == 0 ? 0 : (int)Math.Round(SecondaryWeight * (matchedSecondary.Count / (double)secondary.Count));
            score += secondaryPart;

            // Experience bonus
            int expBonus = 0;
            if (jd.ExperienceMin.HasValue)
            {
                if (resume.YearsOfExperience >= jd.ExperienceMin.Value)
                {
                    // linear up to +10 pts if resume years >= min; cap at max
                    var delta = resume.YearsOfExperience - jd.ExperienceMin.Value;
                    expBonus = Math.Min(ExpBonusMax, 2 * Math.Max(0, delta)); // +2 per extra year, up to +10
                }
            }
            score += expBonus;

            // If any must-have missing → cap to 60
            if (missingMust.Count > 0)
                score = Math.Min(score, 60);

            // clamp 0..100
            score = Math.Max(0, Math.Min(100, score));

            // 5) breakdown json (keep compact)
            var breakdown = new
            {
                must = new { matched = matchedMust, missing = missingMust, weight = MustWeight, part = mustPart },
                primary = new { matched = matchedPrimary, missing = missingPrimary, weight = PrimaryWeight, part = primaryPart },
                secondary = new { matched = matchedSecondary, missing = missingSecondary, weight = SecondaryWeight, part = secondaryPart },
                exp = new { resumeYears = resume.YearsOfExperience, min = jd.ExperienceMin, bonus = expBonus },
                final = score
            };
            var json = JsonSerializer.Serialize(breakdown);

            return new MatchResult(score, json);
        }

        private static (List<string> matched, List<string> missing) MatchSet(string resumeText, List<string> wanted)
        {
            var m = new List<string>();
            var miss = new List<string>();
            foreach (var s in wanted)
                (ContainsSkill(resumeText, s) ? m : miss).Add(s);
            return (m, miss);
        }

        private static List<string> SplitSkills(string? csv)
        {
            if (string.IsNullOrWhiteSpace(csv)) return new List<string>();
            return csv.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(s => s.Trim().ToLowerInvariant())
                      .Distinct()
                      .ToList();
        }

        private static bool ContainsSkill(string resumeText, string skill)
        {
            //exact word boundary check
            if (Regex.IsMatch(resumeText, $@"\b{Regex.Escape(skill)}\b", RegexOptions.IgnoreCase))
                return true;

            //synonyms of this skill
            if (Synonyms.TryGetValue(skill, out var alts))
            {
                foreach (var alt in alts)
                    if (Regex.IsMatch(resumeText, $@"\b{Regex.Escape(alt)}\b", RegexOptions.IgnoreCase))
                        return true;
            }

            //reverse: some skills are alt names of other canonical keys
            foreach (var kv in Synonyms)
            {
                if (kv.Value.Any(v => v.Equals(skill, StringComparison.OrdinalIgnoreCase)))
                {
                    if (Regex.IsMatch(resumeText, $@"\b{Regex.Escape(kv.Key)}\b", RegexOptions.IgnoreCase))
                        return true;
                }
            }

            //soft contains as last resort
            return resumeText.Contains(skill, StringComparison.OrdinalIgnoreCase);
        }
    }
}
