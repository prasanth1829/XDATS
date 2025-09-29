using Microsoft.AspNetCore.Identity;

namespace ResumeApp.Models
{
    public class Users: IdentityUser
    {
        public string FullName { get; set; }
        public bool IsApproved { get; set; } = false;
        public bool IsDenied { get; set; } = false;

    }
}
