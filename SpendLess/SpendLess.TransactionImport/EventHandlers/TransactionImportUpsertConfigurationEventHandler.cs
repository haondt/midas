//using Haondt.Identity.StorageKey;
//using Haondt.Persistence.Services;
//using Haondt.Web.Components;
//using Haondt.Web.Core.Components;
//using Haondt.Web.Core.Extensions;
//using Haondt.Web.Core.Http;
//using SpendLess.Core.Exceptions;
//using SpendLess.Core.Extensions;
//using SpendLess.Domain.Models;
//using SpendLess.Persistence.Extensions;
//using SpendLess.Persistence.Services;
//using SpendLess.TransactionImport.SpendLess.TransactionImport;
//using SpendLess.Web.Domain.Services;
//using SpendLess.Web.Domain.SpendLess.Domain;

//namespace SpendLess.TransactionImport.EventHandlers
//{
//    public class TransactionImportUpsertConfigurationEventHandler(IComponentFactory componentFactory,
//        ISingleTypeSpendLessStorage<TransactionImportConfigurationDto> configStorage,
//        ISingleTypeSpendLessStorage<AccountDto> accountStorage,
//        IStorage storage) : ISingleEventHandler
//    {
//        public string EventName => "TransactionImportUpsertConfiguration";

//        public async Task<IComponent> HandleAsync(IRequestData requestData)
//        {
//            var name = requestData.Form.TryGetValue<string>("name");
//            if (!name.HasValue || string.IsNullOrEmpty(name.Value))
//                throw new UserException($"Name is required.");

//            var id = requestData.Form.TryGetValue<string>("id");
//            var addCurrentDateTag = requestData.Form.TryGetValue<string>("add-ts-tag").HasValue;
//            var account = await requestData.Form.TryGetValue<string>("account")
//                .ConvertAsync(async a => new { StorageKey = a.SeedStorageKey<AccountDto>(), Value = await accountStorage.Get(a.SeedStorageKey<AccountDto>()) });
//            var setDefault = requestData.Form.TryGetValue<string>("set-default").HasValue;

//            var configurationId = id.Or(() => Guid.NewGuid().ToString());
//            var configurationStorageKey = configurationId.SeedStorageKey<TransactionImportConfigurationDto>();
//            await configStorage.Set(configurationStorageKey, new TransactionImportConfigurationDto
//            {
//                AddTagWithCurrentDateTimeToAllTransactions = addCurrentDateTag,
//                Name = name.Value
//            });

//            if (account.HasValue && setDefault)
//            {
//                account.Value.Value.DefaultImportConfiguration = configurationId.SeedStorageKey<TransactionImportConfigurationDto>();
//                await accountStorage.Set(account.Value.StorageKey, account.Value.Value);
//            }

//            var configDtos = await configStorage.GetAll();
//            var accountDtos = await accountStorage.GetAll();
//            return await componentFactory.GetPlainComponent(new AppendComponentLayoutModel
//            {
//                Components = new List<IComponent>
//                {
//                    await componentFactory.GetPlainComponent<CloseModalModel>(),
//                    await componentFactory.GetPlainComponent<TransactionImportModel>(
//                        new TransactionImportSetupModel
//                        {
//                            IsSwap =  true,
//                            SelectedConfiguration = configurationId,
//                            SelectedAccount = account.Convert(a => a.StorageKey.SingleValue()).Or(""),
//                            Configurations = configDtos.ToDictionary(
//                                kvp => kvp.Key.SingleValue(),
//                                kvp => kvp.Value.Name),
//                            Accounts = accountDtos.ToDictionary(
//                                kvp => kvp.Key.SingleValue(),
//                                kvp => (kvp.Value.Name, kvp.Value.DefaultImportConfiguration.Convert(k => k.SingleValue())))
//                        })
//                }
//            });
//            //configureResponse: m => m.ConfigureHeadersAction = new HxHeaderBuilder()
//            //.ReSelect("#import-configuration-selection-field")
//            //.ReTarget("#import-configuration-selection-field")
//            //.ReSwap("true")
//            //.Build());
//        }
//    }
//}
