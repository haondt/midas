using Haondt.Persistence.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SpendLess.Persistence.Converters;

namespace SpendLess.Persistence.Services
{
    public class SpendLessFileStorage : FileStorage
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

        protected override JsonSerializerSettings ConfigureSerializerSettings(JsonSerializerSettings settings)
        {
            settings = base.ConfigureSerializerSettings(settings);
            settings.Converters.Add(new GenericOptionalJsonConverter());
            return settings;
        }
    }
}

