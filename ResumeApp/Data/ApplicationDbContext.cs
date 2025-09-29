using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ResumeApp.Models;

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
    }
}
