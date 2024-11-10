using Haondt.Persistence.Services;
using Microsoft.Extensions.Options;

namespace SpendLess.Persistence.Services
{
    public class SpendLessFileStorage : FileStorage, ISpendLessStorage
    {
        public SpendLessFileStorage(IOptions<SpendLessPersistenceSettings> options)
            : base(Options.Create(new HaondtFileStorageSettings
            {
                DataFile = options.Value.FileStorageSettings!.DataFile
            }))
        {
            if (options.Value.FileStorageSettings == null)
                throw new ArgumentNullException(nameof(SpendLessPersistenceSettings.FileStorageSettings));
        }
    }
}
