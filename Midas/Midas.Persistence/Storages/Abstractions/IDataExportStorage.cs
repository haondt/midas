namespace Midas.Persistence.Storages.Abstractions
{
    public interface IDataExportStorage
    {
        void Export(string targetPath);
    }
}