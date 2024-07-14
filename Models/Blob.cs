namespace SimpleDrive.Models
{
    public class Blob
    {
        public string? Id { get; set; }
        public byte[]? Data { get; set; }
        public long Size { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
