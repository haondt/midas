namespace SpendLess.Persistence.Storages.Abstractions
{
    public interface IDataExportStorage
    {
        void Export(string targetPath);
    }
}