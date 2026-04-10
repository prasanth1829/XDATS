using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ResumeApp.Models;
using ResumeApp.Models.Master;
namespace ResumeApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<Users>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Resume> Resumes { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Spokesperson> Spokespersons { get; set; }
        public DbSet<ClientDocument> ClientDocuments { get; set; }
        public DbSet<ClientRequirement> ClientRequirements { get; set; }
        public DbSet<RequirementAssignment> RequirementAssignments { get; set; }
        public DbSet<ResumeRequirementLink> ResumeRequirementLinks { get; set; }
        public DbSet<CandidateStatusHistory> CandidateStatusHistories { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<PanelAssignment> PanelAssignments { get; set; }
        public DbSet<PanelFeedback> PanelFeedbacks { get; set; }
        public DbSet<InterviewSchedule> InterviewSchedules { get; set; }
        public DbSet<InterviewFeedback> InterviewFeedbacks { get; set; }

        public DbSet<Country> Countries { get; set; } = default!;
        public DbSet<Location> Locations { get; set; } = default!;
        public DbSet<ClientOtherLocation> ClientOtherLocations { get; set; } = default!;
        public DbSet<Designation> Designations { get; set; } = default!;
        public DbSet<DocumentType> DocumentTypes { get; set; } = default!;
        public DbSet<ClientDocumentItem> ClientDocumentItems { get; set; } = default!;
        public DbSet<Skill> Skills { get; set; } = default!;
        public DbSet<RequirementSkill> RequirementSkills { get; set; } = default!;
        public DbSet<Qualification> Qualifications { get; set; } = default!;
        public DbSet<RequirementQualification> RequirementQualifications { get; set; } = default!;
        public DbSet<NoticePeriodOption> NoticePeriodOptions { get; set; } = default!;
        public DbSet<RequirementMom> RequirementMoms { get; set; } = default!;
        public DbSet<RequirementMomHistory> RequirementMomHistories { get; set; } = default!;
        public DbSet<UserSessionLog> UserSessionLogs { get; set; }
        public DbSet<CandidateCallLog> CandidateCallLogs { get; set; }
        public DbSet<CandidateDetail> CandidateDetails { get; set; }
        public DbSet<ResumeSkill> ResumeSkills { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Notification>()
        .HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });
            builder.Entity<CandidateStatusHistory>()
        .HasIndex(h => new { h.RequirementId, h.ResumeId, h.ChangedAt });
            builder.Entity<PanelAssignment>()
        .HasIndex(x => new { x.PanelUserId, x.RequirementId, x.ResumeId })
        .IsUnique(); 
            builder.Entity<PanelFeedback>()
        .HasIndex(x => new { x.PanelUserId, x.RequirementId, x.ResumeId, x.DecidedAt });
            builder.Entity<InterviewSchedule>()
        .HasIndex(x => new { x.RequirementId, x.ResumeId, x.Round });
            builder.Entity<InterviewSchedule>()
                .HasIndex(x => x.ScheduledStartUtc);
            builder.Entity<InterviewFeedback>()
        .HasIndex(x => new { x.InterviewScheduleId, x.PanelUserId })
        .IsUnique();
            builder.Entity<InterviewFeedback>()
        .HasIndex(x => new { x.RequirementId, x.ResumeId, x.Round });
            builder.Entity<ResumeRequirementLink>()
        .HasIndex(l => new { l.RequirementId, l.MatchScore });
            builder.Entity<Client>()
       .HasQueryFilter(c => !c.IsDeleted);
            builder.Entity<ClientRequirement>()
       .HasQueryFilter(r => !r.IsDeleted);

            builder.Entity<Location>()
                   .HasOne(l => l.Country)
                   .WithMany() // not exposing Country.Locations navigation (optional)
                   .HasForeignKey(l => l.CountryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Client>()
                   .HasOne(c => c.HeadquarterCountry)
                   .WithMany()
                   .HasForeignKey(c => c.HeadquarterCountryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Client>()
                   .HasOne(c => c.HeadquarterLocation)
                   .WithMany()
                   .HasForeignKey(c => c.HeadquarterLocationId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ClientOtherLocation>()
                   .HasOne(x => x.Client)
                   .WithMany(c => c.OtherLocationsMap)
                   .HasForeignKey(x => x.ClientId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ClientOtherLocation>()
                   .HasOne(x => x.Location)
                   .WithMany()
                   .HasForeignKey(x => x.LocationId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ClientOtherLocation>()
                   .HasIndex(x => new { x.ClientId, x.LocationId })
                   .IsUnique();

            builder.Entity<Country>()
                   .HasIndex(c => new { c.IsActive, c.SortOrder, c.Name });

            builder.Entity<Location>()
                   .HasIndex(l => new { l.CountryId, l.IsActive, l.SortOrder, l.Name });
            builder.Entity<Designation>()
                   .HasIndex(d => new { d.IsActive, d.SortOrder, d.Name });
            builder.Entity<ClientRequirement>()
                   .HasOne(cr => cr.JobLocationRef)      
                   .WithMany()                            
                   .HasForeignKey(cr => cr.JobLocationId) 
                   .OnDelete(DeleteBehavior.NoAction);
            builder.Entity<Skill>()
                .HasIndex(s => s.Name)
                .IsUnique();
            builder.Entity<Skill>()
                .HasIndex(s => new { s.IsActive, s.SortOrder });
            builder.Entity<RequirementSkill>()
                .HasOne(rs => rs.Requirement)
                .WithMany(r => r.RequirementSkills)
                .HasForeignKey(rs => rs.RequirementId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<RequirementSkill>()
                .HasOne(rs => rs.Skill)
                .WithMany()
                .HasForeignKey(rs => rs.SkillId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<RequirementSkill>()
                .HasIndex(rs => new { rs.RequirementId, rs.SkillId, rs.IsPrimary })
                .IsUnique();
            // Qualification indexes
            builder.Entity<Qualification>()
                .HasIndex(q => q.Name)
                .IsUnique();

            builder.Entity<Qualification>()
                .HasIndex(q => new { q.IsActive, q.SortOrder });

            // RequirementQualification mappings
            builder.Entity<RequirementQualification>()
                .HasOne(rq => rq.Requirement)
                .WithMany(r => r.RequirementQualifications)
                .HasForeignKey(rq => rq.RequirementId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RequirementQualification>()
                .HasOne(rq => rq.Qualification)
                .WithMany()
                .HasForeignKey(rq => rq.QualificationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RequirementQualification>()
                .HasIndex(rq => new { rq.RequirementId, rq.QualificationId })
                .IsUnique();
            builder.Entity<NoticePeriodOption>()
                .HasIndex(n => new { n.IsActive, n.SortOrder, n.Name });
            // Requirement MOM (Minutes of Meeting)
            builder.Entity<RequirementMom>()
                .HasOne(m => m.Requirement)
                .WithMany(r => r.MeetingNotes)
                .HasForeignKey(m => m.RequirementId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RequirementMom>()
                .HasOne(m => m.CreatedByUser)
                .WithMany()
                .HasForeignKey(m => m.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RequirementMom>()
                .HasOne(m => m.LastEditedByUser)
                .WithMany()
                .HasForeignKey(m => m.LastEditedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RequirementMom>()
                .HasIndex(m => new { m.RequirementId, m.MeetingDate, m.CreatedAt });

            builder.Entity<RequirementMomHistory>()
                .HasOne(h => h.RequirementMom)
                .WithMany(m => m.History)
                .HasForeignKey(h => h.RequirementMomId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RequirementMomHistory>()
                .HasOne(h => h.EditedByUser)
                .WithMany()
                .HasForeignKey(h => h.EditedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RequirementMomHistory>()
                .HasIndex(h => new { h.RequirementMomId, h.EditedAt });

            builder.Entity<UserSessionLog>()
                 .HasIndex(x => new { x.UserId, x.LoginTime });

            builder.Entity<UserSessionLog>()
                .HasIndex(x => x.LogoutTime);
            builder.Entity<CandidateDetail>()
                .HasIndex(x => new { x.ResumeId, x.RequirementId })
                .IsUnique();
            builder.Entity<ResumeSkill>()
                .HasIndex(x => x.SkillName);

            builder.Entity<ResumeSkill>()
                .HasIndex(x => x.ResumeId);

        }
    }
}
