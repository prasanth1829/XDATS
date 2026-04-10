using System.Text.RegularExpressions;

namespace ResumeApp.Services
{
    public class ResumeParsingService
    {
        // -------------------------
        // PUBLIC ENTRY POINT
        // -------------------------
        public static ParsedResume Parse(string rawText)
        {
            var text = NormalizeText(rawText);
            var sections = DetectSections(text);

            var name = ExtractName(text, sections);
            var email = ExtractEmail(text);
            var phone = ExtractPhone(text);
            var skills = ExtractSkills(sections);
            var experienceText = ExtractExperienceText(sections);
            var years = ExtractYearsOfExperience(experienceText);

            return new ParsedResume
            {
                Name = name,
                Email = email,
                Phone = phone,
                Skills = skills,
                Experience = experienceText,
                Years = years
            };
        }

        // -------------------------
        // STEP 1: NORMALIZATION
        // -------------------------
        private static string NormalizeText(string text)
        {
            text = text.Replace("\t", " ");
            text = Regex.Replace(text, @"\s{2,}", " ");
            text = Regex.Replace(text, @"(\r\n|\r|\n){2,}", "\n");
            return text.Trim();
        }

        // -------------------------
        // STEP 2: SECTION DETECTION
        // -------------------------
        private static Dictionary<string, string> DetectSections(string text)
        {
            var sections = new Dictionary<string, string>();
            var lines = text.Split('\n');

            string current = "header";
            sections[current] = "";

            foreach (var raw in lines)
            {
                var line = raw.Trim();
                var lower = line.ToLower();

                if (IsSectionHeader(lower, out string section))
                {
                    current = section;
                    if (!sections.ContainsKey(current))
                        sections[current] = "";
                    continue;
                }

                sections[current] += line + "\n";
            }

            return sections;
        }

        private static bool IsSectionHeader(string line, out string section)
        {
            section = "";

            if (Regex.IsMatch(line, @"^(skills|technical skills|key skills)$"))
                section = "skills";
            else if (Regex.IsMatch(line, @"^(experience|work experience|professional experience)$"))
                section = "experience";
            else if (Regex.IsMatch(line, @"^(summary|profile)$"))
                section = "summary";
            else if (Regex.IsMatch(line, @"^(education|qualification)$"))
                section = "education";
            else if (Regex.IsMatch(line, @"^(projects)$"))
                section = "projects";

            return section != "";
        }

        // -------------------------
        // STEP 3: NAME
        // -------------------------
        private static string ExtractName(string text, Dictionary<string, string> sections)
        {
            var header = sections.ContainsKey("header") ? sections["header"] : text;
            var lines = header.Split('\n').Take(5);

            foreach (var line in lines)
            {
                if (line.Length < 3 || line.Length > 60) continue;
                if (Regex.IsMatch(line, @"\d")) continue;
                if (line.Contains("@")) continue;

                if (Regex.IsMatch(line, @"^[A-Za-z ]+$"))
                    return line.Trim();
            }

            return "Not found";
        }

        // -------------------------
        // EMAIL & PHONE
        // -------------------------
        private static string ExtractEmail(string text)
        {
            var match = Regex.Match(text, @"[\w\.-]+@[\w\.-]+\.\w+");
            return match.Success ? match.Value : "Not found";
        }

        private static string ExtractPhone(string text)
        {
            var match = Regex.Match(text, @"(\+91[-\s]?)?[6-9]\d{9}");
            return match.Success ? match.Value : "Not found";
        }

        // -------------------------
        // STEP 4: SKILLS (HYBRID)
        // -------------------------
        private static string ExtractSkills(Dictionary<string, string> sections)
        {
            var skillSources = new List<string>();

            if (sections.ContainsKey("skills"))
                skillSources.Add(sections["skills"]);

            if (sections.ContainsKey("experience"))
                skillSources.Add(sections["experience"]);

            if (sections.ContainsKey("projects"))
                skillSources.Add(sections["projects"]);

            var combined = string.Join(" ", skillSources).ToLower();

            var knownSkills = new[]
            {
                "c#", "dotnet", ".net", "sql", "html", "css",
                "javascript", "react", "angular", "asp.net",
                "mvc", "api", "azure", "aws"
            };

            var found = knownSkills
                .Where(s => combined.Contains(s))
                .Distinct()
                .ToList();

            return found.Count == 0 ? "Not found" : string.Join(", ", found);
        }

        // -------------------------
        // STEP 5: EXPERIENCE
        // -------------------------
        private static string ExtractExperienceText(Dictionary<string, string> sections)
        {
            return sections.ContainsKey("experience")
                ? sections["experience"].Trim()
                : "Not found";
        }

        private static int ExtractYearsOfExperience(string experience)
        {
            var match = Regex.Match(experience, @"(\d+(\.\d+)?)\s*(\+)?\s*(yrs?|years?)");
            if (match.Success && double.TryParse(match.Groups[1].Value, out var yrs))
                return (int)Math.Floor(yrs);

            var dateMatch = Regex.Match(experience, @"(20\d{2}).*(20\d{2}|present)");
            if (dateMatch.Success)
            {
                int start = int.Parse(dateMatch.Groups[1].Value);
                int end = dateMatch.Groups[2].Value == "present"
                    ? DateTime.Now.Year
                    : int.Parse(dateMatch.Groups[2].Value);
                return Math.Max(0, end - start);
            }

            return 0;
        }
    }

    // -------------------------
    // RESULT MODEL
    // -------------------------
    public class ParsedResume
    {
        public string Name { get; set; } = "Not found";
        public string Email { get; set; } = "Not found";
        public string Phone { get; set; } = "Not found";
        public string Skills { get; set; } = "Not found";
        public string Experience { get; set; } = "Not found";
        public int Years { get; set; }
    }
}
