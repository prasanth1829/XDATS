using System.ComponentModel.DataAnnotations;

namespace ResumeApp.Models
{
    public class ClientDocumentItem
    {
        public int Id { get; set; }

        [Required]
        public int ClientId { get; set; }
        public Client Client { get; set; } = default!;

        [Required]
        public int DocumentTypeId { get; set; }
        public DocumentType DocumentType { get; set; } = default!;

        [Required, MaxLength(500)]
        public string FilePath { get; set; } = default!;

        public DateTime UploadedOn { get; set; } = DateTime.UtcNow;
    }
}
