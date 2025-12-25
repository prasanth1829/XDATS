using ResumeApp.Models;

namespace ResumeApp.Services
{
    public record MatchResult(
        int Score,
        string BreakdownJson
    );

    public interface IMatchScoringService
    {
        MatchResult Compute(ClientRequirement jd, Resume resume);
    }
}
