namespace ResumeApp.ViewModels
{
    public class UserWithRolesViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Roles { get; set; }
        public bool IsLocked { get; set; }

        public bool IsApproved { get; set; }

    }

}
