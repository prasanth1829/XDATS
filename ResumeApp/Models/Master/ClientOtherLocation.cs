namespace ResumeApp.Models
{
    public class ClientOtherLocation
    {
        public int Id { get; set; }

        public int ClientId { get; set; }
        public Client Client { get; set; } = default!;

        public int LocationId { get; set; }
        public Location Location { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
