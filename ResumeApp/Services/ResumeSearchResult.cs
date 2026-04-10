using ResumeApp.Models;

namespace ResumeApp.Services
{
    public class ResumeSearchResult
    {
        public Resume Resume { get; set; } = null!;
        public int OptionalScore { get; set; }
    }
}
