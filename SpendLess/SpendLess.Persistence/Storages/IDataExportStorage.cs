namespace SpendLess.Persistence.Storages
{
    public interface IDataExportStorage
    {
        void Export(string targetPath);
    }
}