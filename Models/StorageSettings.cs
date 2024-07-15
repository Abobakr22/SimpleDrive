using System.ComponentModel.DataAnnotations;

namespace SimpleDrive.Models
{
    public class StorageSettings
    {
        [Key]
        public Guid Id { get; set; }
        public string StorageType { get; set; }
        public string BlobId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
