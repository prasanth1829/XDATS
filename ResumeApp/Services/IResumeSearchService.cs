namespace ResumeApp.Services
{
    public interface IResumeSearchService
    {

        IQueryable<ResumeSearchResult> BuildSearchQuery(string? search,int? minExperience,string? mandatorySkills,string? optionalSkills);
    }
}
