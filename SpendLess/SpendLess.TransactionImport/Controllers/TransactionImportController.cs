using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Haondt.Identity.StorageKey;
using Haondt.Web.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Domain.Models;
using SpendLess.Persistence.Extensions;
using SpendLess.Persistence.Services;
using SpendLess.TransactionImport.Components;
using SpendLess.TransactionImport.Models;
using SpendLess.TransactionImport.Services;
using SpendLess.Web.Domain.Components;
using SpendLess.Web.Domain.Controllers;
using SpendLess.Web.Domain.Extensions;
using System.ComponentModel.DataAnnotations;

namespace SpendLess.TransactionImport.Controllers
{
    [Route("transaction-import")]
    public class TransactionImportController(
        IComponentFactory componentFactory,
        ISingleTypeSpendLessStorage<TransactionImportConfigurationDto> configStorage,
        ISingleTypeSpendLessStorage<TransactionImportAccountMetadataDto> accountMetadataStorage,
        ITransactionImportService import) : SpendLessUIController
    {
        [HttpGet]
        [ServiceFilter(typeof(RenderPageFilter))]
        public Task<IResult> Get()
        {
            return componentFactory.RenderComponentAsync<TransactionImport.Components.TransactionImport>();
        }

        [HttpGet("config")]
        public async Task<IResult> GetConfigModal(
            [FromQuery] string? config,
            [FromQuery] string? account)
        {
            var model = new UpsertConfigurationModal();

            if (!string.IsNullOrEmpty(config))
            {
                model.Id = config;
                var configDto = await configStorage.Get(config.SeedStorageKey<TransactionImportConfigurationDto>());
                model.Name = configDto.Name;
                model.AddImportTag = configDto.AddImportTag;

            }
            if (!string.IsNullOrEmpty(account))
            {
                var accountMetadata = await accountMetadataStorage.TryGet(account.SeedStorageKey<TransactionImportAccountMetadataDto>());
                var isDefault = accountMetadata.HasValue
                    && accountMetadata.Value.DefaultConfiguration.HasValue
                    && accountMetadata.Value.DefaultConfiguration.Value.SingleValue() == config;

                model.SelectedAccount = new((account, isDefault));

            }
            return await componentFactory.RenderComponentAsync(model);
        }


        [HttpPost("{config}")]
        public async Task<IResult> UpsertConfig([FromForm] TransactionImportUpsertConfigRequestDto request)
        {
            request.Name = request.Name.Trim();
            request.Id ??= Guid.NewGuid().ToString();
            var configStorageKey = request.Id.SeedStorageKey<TransactionImportConfigurationDto>();

            var updatedConfig = new TransactionImportConfigurationDto
            {
                AddImportTag = request.AddImportTag,
                Name = request.Name,
            };
            await configStorage.Set(configStorageKey, updatedConfig);

            if (!string.IsNullOrEmpty(request.Account))
            {
                var accountStorageKey = request.Account.SeedStorageKey<TransactionImportAccountMetadataDto>();
                await accountMetadataStorage.Set(accountStorageKey, new TransactionImportAccountMetadataDto
                {
                    DefaultConfiguration = configStorageKey
                });
            }

            var setupModel = new Setup();
            if (!string.IsNullOrEmpty(request.Account))
                setupModel.SelectedAccount = request.Account;
            return await componentFactory.RenderComponentAsync(new AppendComponentLayout
            {
                Components = [
                    new CloseModal(),
                    setupModel
                ]
            });
        }

        [HttpPost("dry-run")]
        public async Task<IResult> StartDryRun(
            [FromForm, Required] string account,
            [FromForm, Required] string config,
            [FromForm, Required] IFormFile file)
        {
            var csvData = file.ParseAsCsv();
            var configDto = await configStorage.Get(config.SeedStorageKey<TransactionImportConfigurationDto>());
            var jobId = import.StartDryRun(configDto, account, csvData);

            return await componentFactory.RenderComponentAsync(new ProgressPanel
            {
                CallbackEndpoint = $"/transaction-import/dry-run/{jobId}",
                ProgressPercent = 0
            });
        }

        [HttpGet("dry-run/{id}")]
        public async Task<IResult> GetJobStatus(string id)
        {
            var result = import.GetDryRunResult(id);
            if (!result.IsSuccessful)
            {
                return await componentFactory.RenderComponentAsync(new ProgressPanel
                {
                    CallbackEndpoint = $"/transaction-import/dry-run/{id}",
                    ProgressPercent = result.Reason.Progress
                });
            }


            return await componentFactory.RenderComponentAsync(new DryRunResult
            {
                BalanceChange = result.Value.BalanceChange,
                ImportAccountName = result.Value.ImportAccount.Name,
                Errors = result.Value.Transactions.SelectMany(r => r.Errors.Select(e => (e, r)))
                    .GroupBy(t => t.e)
                    .Select(grp => (grp.Key, grp.Count(), grp.First().r.SourceRequestPayload))
                    .ToList(),
                Warnings = result.Value.Transactions.SelectMany(r => r.Warnings)
                    .GroupBy(w => w)
                    .Select(grp => (TransactionImportWarning.GetDescription(grp.Key), grp.Count()))
                    .ToList(),
                NewAccounts = result.Value.NewAccounts.Select(kvp => kvp.Value)
                    .OrderBy(s => s)
                    .ToList(),
                NewCategories = result.Value.NewCategories,
                NewTags = result.Value.NewTags,
                JobId = id
            });
        }

        [HttpGet("dry-run/{id}/transactions")]
        public Task<IResult> GetDryRunTransactions(string id)
        {
            return componentFactory.RenderComponentAsync(new DryRunTransactionsModal
            {
                JobId = id
            });
        }

        [HttpPost("dry-run/{id}/transactions/filter")]
        public Task<IResult> AddFilter(
            string id,
            [FromForm] string target)
        {
            var filterText = GetFilterText(id, target);
            if (filterText.HasValue)
                return componentFactory.RenderComponentAsync(new DryRunTransactionFilter { Text = filterText.Value });

            return Task.FromResult(Results.BadRequest());
        }

        private Optional<string> GetFilterText(string id, string target)
        {
            var rd = Request.AsRequestData();
            var value = rd.Form
                .TryGetValue<string>($"{target}-value");
            var op = rd.Form
                .GetValue<string>("operator");

            Optional<string> defaultedValue = target switch
            {
                TransactionFilterTargets.Amount => !value.HasValue
                    ? new Optional<string>()
                    : decimal.TryParse(value.Value, out var decimalValue)
                        ? new(decimalValue.ToString("F2"))
                        : new(),
                TransactionFilterTargets.Status => value.Value,
                TransactionFilterTargets.Warning => value.Value,
                TransactionFilterTargets.Description => value.Or(""),
                TransactionFilterTargets.Category => value.Or(""),
                _ => new()
            };

            return defaultedValue.As(v => $"{target} {op} {v}");
        }

        [HttpPost("dry-run/{id}/transactions")]
        public async Task<IResult> GetDryRunTransactions(
            string id,
            [FromForm] IEnumerable<string> filters,
            [FromForm(Name = "page-size")] int? pageSize,
            [FromForm] int? page)
        {
            pageSize ??= 25;
            page ??= 1;

            var result = import.GetDryRunResult(id);
            if (!result.IsSuccessful)
                throw new InvalidOperationException($"Dry run {id} is not complete yet.");

            var transactions = result.Value.Transactions;
            var filteredTransactions = transactions.AsEnumerable();
            foreach (var filter in filters)
            {
                var splitFilter = filter.Split(' ');
                var target = splitFilter[0];
                var op = splitFilter[1];
                var value = splitFilter[2];

                // todo: case insensitive
                switch (target)
                {
                    case TransactionFilterTargets.Status:
                        switch (op)
                        {
                            case TransactionFilterOperators.IsEqualTo:
                                filteredTransactions = filteredTransactions.Where(t => t.Status == value);
                                break;
                            case TransactionFilterOperators.IsNotEqualTo:
                                filteredTransactions = filteredTransactions.Where(t => t.Status != value);
                                break;
                            case TransactionFilterOperators.Contains:
                                filteredTransactions = filteredTransactions.Where(t => t.Status.Contains(value));
                                break;
                            case TransactionFilterOperators.StartsWith:
                                filteredTransactions = filteredTransactions.Where(t => t.Status.StartsWith(value));
                                break;
                            default:
                                throw new InvalidOperationException($"unknown operator {op}");
                        }
                        break;
                    case TransactionFilterTargets.Warning:
                        switch (op)
                        {
                            case TransactionFilterOperators.IsEqualTo:
                                filteredTransactions = filteredTransactions.Where(t => t.Warnings.Contains(value));
                                break;
                            case TransactionFilterOperators.IsNotEqualTo:
                                filteredTransactions = filteredTransactions.Where(t => !t.Warnings.Contains(value));
                                break;
                            case TransactionFilterOperators.Contains:
                                filteredTransactions = filteredTransactions.Where(t => t.Warnings.Any(w => w.Contains(value)));
                                break;
                            case TransactionFilterOperators.StartsWith:
                                filteredTransactions = filteredTransactions.Where(t => t.Warnings.Any(w => w.StartsWith(value)));
                                break;
                            default:
                                throw new InvalidOperationException($"unknown operator {op}");
                        }
                        break;
                    case TransactionFilterTargets.Error:
                        switch (op)
                        {
                            case TransactionFilterOperators.IsEqualTo:
                                filteredTransactions = filteredTransactions.Where(t => t.Errors.Contains(value));
                                break;
                            case TransactionFilterOperators.IsNotEqualTo:
                                filteredTransactions = filteredTransactions.Where(t => !t.Errors.Contains(value));
                                break;
                            case TransactionFilterOperators.Contains:
                                filteredTransactions = filteredTransactions.Where(t => t.Errors.Any(e => e.Contains(value)));
                                break;
                            case TransactionFilterOperators.StartsWith:
                                filteredTransactions = filteredTransactions.Where(t => t.Errors.Any(e => e.StartsWith(value)));
                                break;
                            default:
                                throw new InvalidOperationException($"unknown operator {op}");
                        }
                        break;
                    case TransactionFilterTargets.Description:
                        switch (op)
                        {
                            case TransactionFilterOperators.IsEqualTo:
                                filteredTransactions = filteredTransactions.Where(t => t.Transaction.As(t => t.Description == value).Or(false));
                                break;
                            case TransactionFilterOperators.IsNotEqualTo:
                                filteredTransactions = filteredTransactions.Where(t => t.Transaction.As(t => t.Description != value).Or(false));
                                break;
                            case TransactionFilterOperators.Contains:
                                filteredTransactions = filteredTransactions.Where(t => t.Transaction.As(t => t.Description.Contains(value)).Or(false));
                                break;
                            case TransactionFilterOperators.StartsWith:
                                filteredTransactions = filteredTransactions.Where(t => t.Transaction.As(t => t.Description.StartsWith(value)).Or(false));
                                break;
                            default:
                                throw new InvalidOperationException($"unknown operator {op}");
                        }
                        break;
                    case TransactionFilterTargets.Category:
                        switch (op)
                        {
                            case TransactionFilterOperators.IsEqualTo:
                                filteredTransactions = filteredTransactions.Where(t => t.Transaction.As(t => t.Category == value).Or(false));
                                break;
                            case TransactionFilterOperators.IsNotEqualTo:
                                filteredTransactions = filteredTransactions.Where(t => t.Transaction.As(t => t.Category != value).Or(false));
                                break;
                            case TransactionFilterOperators.Contains:
                                filteredTransactions = filteredTransactions.Where(t => t.Transaction.As(t => t.Category.Contains(value)).Or(false));
                                break;
                            case TransactionFilterOperators.StartsWith:
                                filteredTransactions = filteredTransactions.Where(t => t.Transaction.As(t => t.Category.StartsWith(value)).Or(false));
                                break;
                            default:
                                throw new InvalidOperationException($"unknown operator {op}");
                        }
                        break;
                    case TransactionFilterTargets.Amount:
                        switch (op)
                        {
                            case TransactionFilterOperators.IsEqualTo:
                                filteredTransactions = filteredTransactions.Where(t => t.Transaction.As(t => t.Amount == decimal.Parse(value)).Or(false));
                                break;
                            case TransactionFilterOperators.IsNotEqualTo:
                                filteredTransactions = filteredTransactions.Where(t => t.Transaction.As(t => t.Amount != decimal.Parse(value)).Or(false));
                                break;
                            case TransactionFilterOperators.Contains:
                            case TransactionFilterOperators.StartsWith:
                                break;
                            default:
                                throw new InvalidOperationException($"unknown operator {op}");
                        }
                        break;
                    default:
                        throw new InvalidOperationException($"unknown target {target}");
                }
            }

            var totalItems = filteredTransactions.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize.Value);
            filteredTransactions = filteredTransactions
                .Skip((page.Value - 1) * pageSize.Value)
                .Take(pageSize.Value);

            return await componentFactory.RenderComponentAsync(new DryRunTransactionsSearchResult
            {
                Results = filteredTransactions.ToList(),
                Page = page.Value,
                TotalPages = totalPages
            });
        }
    }


}
