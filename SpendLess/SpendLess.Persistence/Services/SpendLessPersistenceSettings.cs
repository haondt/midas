namespace SpendLess.Persistence.Services
{
    public class SpendLessPersistenceSettings
    {
        public SpendLessPersistenceDrivers Driver { get; set; } = SpendLessPersistenceDrivers.File;
        public FileStorageSettings? FileStorageSettings { get; set; }

    }

    public class FileStorageSettings
    {
        public string DataFile { get; set; } = "./data.json";
    }
}
