using System.Text.Json;

namespace ResumeApp.Services
{
    public class SkillParser
    {
        public static List<string> Parse(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new List<string>();

            input = input.Trim();

            List<string> skills = new();

            // ✅ CASE 1: Handle Tagify JSON format
            if (input.StartsWith("["))
            {
                try
                {
                    var tags = JsonSerializer.Deserialize<List<TagModel>>(input);

                    skills = tags?
                        .Where(t => !string.IsNullOrWhiteSpace(t.value))
                        .Select(t => t.value)
                        .ToList()
                        ?? new List<string>();
                }
                catch
                {
                    skills = new List<string>();
                }
            }
            else
            {
                // ✅ CASE 2: Normal comma-separated
                skills = input
                    .Split(new[] { ',', '|' }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
            }

            // ✅ COMMON CLEANING LOGIC
            return skills
                .Select(s => s.Trim().ToLower())
                .Select(Normalize)
                .Where(s =>
                    s.Length > 1 &&
                    s.Length <= 50 &&
                    s.Count(c => c == ' ') <= 3 &&
                    !s.Contains('.') &&
                    !s.Contains("experience") &&
                    !s.Contains("years"))
                .Distinct()
                .ToList();
        }

        private static string Normalize(string skill)
        {
            return skill switch
            {
                "c sharp" => "c#",
                "csharp" => "c#",
                "ms sql" => "sql",
                "mssql" => "sql",
                _ => skill
            };
        }

        private class TagModel
        {
            public string value { get; set; } = "";
        }
    }
}
