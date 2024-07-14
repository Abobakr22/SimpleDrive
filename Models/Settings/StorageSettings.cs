namespace SimpleDrive.Models.Settings
{
    public class StorageSettings
    {
        public LocalStorageSettings Local { get; set; }
        public S3StorageSettings S3 { get; set; }
        public DatabaseStorageSettings Database { get; set; }
        public FTPStorageSettings FTP { get; set; }
    }
}
