using Haondt.Core.Extensions;
using Haondt.Core.Models;
using Haondt.Identity.StorageKey;
using Haondt.Persistence.Services;
using Haondt.Web.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Accounts.Services;
using SpendLess.Core.Exceptions;
using SpendLess.TransactionImport.Components;
using SpendLess.TransactionImport.Models;
using SpendLess.TransactionImport.Services;
using SpendLess.Transactions.Controllers;
using SpendLess.Transactions.Models;
using SpendLess.Transactions.Services;
using SpendLess.Web.Core.ModelBinders;
using SpendLess.Web.Domain.Components;
using SpendLess.Web.Domain.Controllers;
using SpendLess.Web.Domain.Extensions;
using System.Text.RegularExpressions;

namespace SpendLess.TransactionImport.Controllers
{
    [Route("transaction-import")]
    public class TransactionImportController(
        IComponentFactory componentFactory,
        IAccountsService accountsService,
        IStorage storage,
        ITransactionService transactionService,
        ITransactionImportService import) : SpendLessUIController
    {
        [HttpGet]
        [ServiceFilter(typeof(RenderPageFilter))]
        public Task<IResult> Get()
        {
            return componentFactory.RenderComponentAsync<TransactionImport.Components.TransactionImport>();
        }

        [HttpGet("reimport")]
        [ServiceFilter(typeof(RenderPageFilter))]
        public async Task<IResult> GetReimport(
            [FromQuery] IEnumerable<string> filters,
            [FromQuery(Name = "select-all"), ModelBinder(typeof(CheckboxModelBinder))] bool selectAll)
        {
            var result = new SetupSelectTransactionsField();
            if (selectAll)
            {
                var parsedFilters = TransactionsController.ParseFilters(filters).ToList();
                result.SelectedTransactions = await transactionService.GetTransactionsCount(parsedFilters);
                result.SelectedTransactionFilters = filters.ToList();
            }
            else
            {
                result.SelectedTransactionIds = Request.AsRequestData().Query
                    .Where(kvp => Regex.IsMatch(kvp.Key, "^t-[0-9]+$"))
                    .Select(kvp => kvp.Key.Substring(2))
                    .Select(s => long.Parse(s))
                    .ToList();
                result.SelectedTransactions = result.SelectedTransactionIds.Count;
            }

            return await componentFactory.RenderComponentAsync(new TransactionReimport
            {
                SelectTransactionsField = result,
            });
        }

        [HttpGet("reimport/search")]
        public async Task<IResult> GetReimportSearchModal()
        {
            return await componentFactory.RenderComponentAsync<TransactionReimportSearchModal>();
        }

        [HttpPost("reimport/search")]
        public async Task<IResult> UpdateReimportSearchQuery(
            [FromForm] IEnumerable<string> filters,
            [FromForm(Name = "select-all"), ModelBinder(typeof(CheckboxModelBinder))] bool selectAll)
        {
            var result = new SetupSelectTransactionsField
            {
                Swap = true
            };

            if (selectAll)
            {
                var parsedFilters = TransactionsController.ParseFilters(filters).ToList();
                result.SelectedTransactions = await transactionService.GetTransactionsCount(parsedFilters);
                result.SelectedTransactionFilters = filters.ToList();
            }
            else
            {
                result.SelectedTransactionIds = Request.AsRequestData().Form
                    .Where(kvp => Regex.IsMatch(kvp.Key, "^t-[0-9]+$"))
                    .Select(kvp => kvp.Key.Substring(2))
                    .Select(s => long.Parse(s))
                    .ToList();
                result.SelectedTransactions = result.SelectedTransactionIds.Count;
            }

            return await componentFactory.RenderComponentAsync(new AppendComponentLayout
            {
                Components = [
                    result,
                    new CloseModal()
                ]
            });
        }

        [HttpGet("configs")]
        public async Task<IResult> GetConfigModal()
        {
            return await componentFactory.RenderComponentAsync<UpsertConfigurationModal>();
        }


        [HttpPost("configs")]
        public async Task<IResult> UpsertConfigs([FromForm(Name = "config-slug")] IEnumerable<string?> slugs)
        {
            await storage.Set(StorageKey<TransactionImportConfigurationSlugsDto>.Empty, new TransactionImportConfigurationSlugsDto
            {
                Slugs = slugs
                    .Select(s => s?.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Cast<string>()
                    .ToList()
            });

            return await componentFactory.RenderComponentAsync(new AppendComponentLayout
            {
                Components = [
                    new CloseModal(),
                    new SetupConfigurationField()
                ]
            });
        }

        [HttpPost("dry-run")]
        public async Task<IResult> StartDryRun(
            [FromForm] string? account,
            [FromForm] string? config,
            [FromForm] IFormFile? file,
            [FromForm(Name = "is-reimport")] bool isReimport,
            [FromForm(Name = "add-import-tag"), ModelBinder(typeof(CheckboxModelBinder))] bool addImportTag,
            [FromForm] IEnumerable<string> filters,
            [FromForm] TransactionImportConflictResolutionStrategy conflicts,
            [FromForm] string? transactions)
        {
            // TODO 
            //if (setDefaultConfiguration
            //    && !string.IsNullOrEmpty(account)
            //    && !string.IsNullOrEmpty(config))
            //    await accountMetadataStorage.Set(account.SeedStorageKey<TransactionImportAccountMetadataDto>(), new TransactionImportAccountMetadataDto
            //    {
            //        DefaultConfiguration = config
            //    });

            string jobId;
            if (!isReimport)
            {
                if (file is null)
                    throw new UserException("Please select a file.");
                if (string.IsNullOrEmpty(account))
                    throw new UserException("Please select an account.");
                if (string.IsNullOrEmpty(config))
                    throw new UserException("Please select a configuration.");
                var csvData = file.ParseAsCsv();
                jobId = import.StartDryRun(csvData, addImportTag, config, account, conflicts);
            }
            else
            {
                var parsedFilters = TransactionsController.ParseFilters(filters).ToList();
                if (!string.IsNullOrEmpty(transactions))
                    parsedFilters.Add(TransactionFilter.TransactionIdIsOneOf(transactions
                        .Split(',')
                        .Select(q => long.Parse(q))
                        .ToList()));

                if (string.IsNullOrEmpty(account))
                {
                    jobId = import.StartDryRun(parsedFilters.ToList(), addImportTag);
                }
                else
                {
                    jobId = import.StartDryRun(parsedFilters.ToList(), addImportTag, config, account);
                }

            }

            return await componentFactory.RenderComponentAsync(new ProgressPanel
            {
                CallbackEndpoint = $"/transaction-import/dry-run/{jobId}",
                ProgressPercent = 0
            });
        }

        [HttpGet("dry-run/{id}")]
        public async Task<IResult> GetDryRunStatus(string id)
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


            var creditBalanceChanges = result.Value.Transactions
                .Where(t => t.TransactionData.HasValue)
                .GroupBy(t => t.TransactionData.Value.Destination.Id)
                .ToDictionary(grp => grp.Key, grp => grp.Sum(t => t.TransactionData.Value.Amount));


            var debitBalanceChanges = result.Value.Transactions
                .Where(t => t.TransactionData.HasValue)
                .GroupBy(t => t.TransactionData.Value.Source.Id)
                .ToDictionary(grp => grp.Key, grp => grp.Sum(t => t.TransactionData.Value.Amount)); ;

            var balanceChanges = creditBalanceChanges;
            foreach (var (key, value) in debitBalanceChanges)
            {
                if (!balanceChanges.ContainsKey(key))
                    balanceChanges[key] = 0;
                balanceChanges[key] -= value;
            }

            var unimportedTransactionIds = result.Value.Transactions
                .Where(t => t.ReplacementTarget.HasValue)
                .Select(t => t.ReplacementTarget.Value);
            var unimportedTransactions = await transactionService.GetAmounts([TransactionFilter.TransactionIdIsOneOf(unimportedTransactionIds.ToList())]);
            foreach (var (key, value) in unimportedTransactions.BySource)
            {
                if (!balanceChanges.ContainsKey(key))
                    balanceChanges[key] = 0;
                balanceChanges[key] += value;
            }
            foreach (var (key, value) in unimportedTransactions.ByDestination)
            {
                if (!balanceChanges.ContainsKey(key))
                    balanceChanges[key] = 0;
                balanceChanges[key] -= value;
            }

            var pulledAccounts = new Dictionary<string, string>();
            var balanceChangesTasks = balanceChanges
                .Where(kvp => kvp.Value != 0)
                .Select(async kvp =>
            {
                if (result.Value.NewAccounts.TryGetValue(kvp.Key, out var accountName))
                    return (accountName, kvp.Value);

                if (!pulledAccounts.TryGetValue(kvp.Key, out accountName))
                    pulledAccounts[kvp.Key] = (await accountsService.GetAccount(kvp.Key)).Name;
                return (pulledAccounts[kvp.Key], kvp.Value);
            });
            var balanceChangesList = await Task.WhenAll(balanceChangesTasks);

            return await componentFactory.RenderComponentAsync(new DryRunResult
            {
                BalanceChanges = balanceChangesList.ToList(),
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
                TransactionImportFilterTargets.Amount => !value.HasValue
                    ? new Optional<string>()
                    : decimal.TryParse(value.Value, out var decimalValue)
                        ? new(decimalValue.ToString("F2"))
                        : new(),
                TransactionImportFilterTargets.Status => value.Value,
                TransactionImportFilterTargets.Warning => value.Value,
                TransactionImportFilterTargets.Description => value.Or(""),
                TransactionImportFilterTargets.Category => value.Or(""),
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

                switch (target)
                {
                    case TransactionImportFilterTargets.Status:
                        switch (op)
                        {
                            case TransactionImportFilterOperators.IsEqualTo:
                                filteredTransactions = filteredTransactions.Where(t => t.Status == value);
                                break;
                            case TransactionImportFilterOperators.IsNotEqualTo:
                                filteredTransactions = filteredTransactions.Where(t => t.Status != value);
                                break;
                            case TransactionImportFilterOperators.Contains:
                                filteredTransactions = filteredTransactions.Where(t => t.Status.Contains(value));
                                break;
                            case TransactionImportFilterOperators.StartsWith:
                                filteredTransactions = filteredTransactions.Where(t => t.Status.StartsWith(value));
                                break;
                            default:
                                throw new InvalidOperationException($"unknown operator {op}");
                        }
                        break;
                    case TransactionImportFilterTargets.Warning:
                        switch (op)
                        {
                            case TransactionImportFilterOperators.IsEqualTo:
                                filteredTransactions = filteredTransactions.Where(t => t.Warnings.Contains(value, StringComparer.CurrentCultureIgnoreCase));
                                break;
                            case TransactionImportFilterOperators.IsNotEqualTo:
                                filteredTransactions = filteredTransactions.Where(t => !t.Warnings.Contains(value, StringComparer.CurrentCultureIgnoreCase));
                                break;
                            case TransactionImportFilterOperators.Contains:
                                filteredTransactions = filteredTransactions.Where(t => t.Warnings.Any(w => w.Contains(value, StringComparison.CurrentCultureIgnoreCase)));
                                break;
                            case TransactionImportFilterOperators.StartsWith:
                                filteredTransactions = filteredTransactions.Where(t => t.Warnings.Any(w => w.StartsWith(value, StringComparison.CurrentCultureIgnoreCase)));
                                break;
                            default:
                                throw new InvalidOperationException($"unknown operator {op}");
                        }
                        break;
                    case TransactionImportFilterTargets.Error:
                        switch (op)
                        {
                            case TransactionImportFilterOperators.IsEqualTo:
                                filteredTransactions = filteredTransactions.Where(t => t.Errors.Contains(value, StringComparer.CurrentCultureIgnoreCase));
                                break;
                            case TransactionImportFilterOperators.IsNotEqualTo:
                                filteredTransactions = filteredTransactions.Where(t => !t.Errors.Contains(value, StringComparer.CurrentCultureIgnoreCase));
                                break;
                            case TransactionImportFilterOperators.Contains:
                                filteredTransactions = filteredTransactions.Where(t => t.Errors.Any(e => e.Contains(value, StringComparison.CurrentCultureIgnoreCase)));
                                break;
                            case TransactionImportFilterOperators.StartsWith:
                                filteredTransactions = filteredTransactions.Where(t => t.Errors.Any(e => e.StartsWith(value, StringComparison.CurrentCultureIgnoreCase)));
                                break;
                            default:
                                throw new InvalidOperationException($"unknown operator {op}");
                        }
                        break;
                    case TransactionImportFilterTargets.Description:
                        switch (op)
                        {
                            case TransactionImportFilterOperators.IsEqualTo:
                                filteredTransactions = filteredTransactions.Where(t => t.TransactionData.As(t => string.Equals(t.Description, value, StringComparison.CurrentCultureIgnoreCase)).Or(false));
                                break;
                            case TransactionImportFilterOperators.IsNotEqualTo:
                                filteredTransactions = filteredTransactions.Where(t => t.TransactionData.As(t => !string.Equals(t.Description, value, StringComparison.CurrentCultureIgnoreCase)).Or(false));
                                break;
                            case TransactionImportFilterOperators.Contains:
                                filteredTransactions = filteredTransactions.Where(t => t.TransactionData.As(t => t.Description.Contains(value, StringComparison.CurrentCultureIgnoreCase)).Or(false));
                                break;
                            case TransactionImportFilterOperators.StartsWith:
                                filteredTransactions = filteredTransactions.Where(t => t.TransactionData.As(t => t.Description.StartsWith(value, StringComparison.CurrentCultureIgnoreCase)).Or(false));
                                break;
                            default:
                                throw new InvalidOperationException($"unknown operator {op}");
                        }
                        break;
                    case TransactionImportFilterTargets.Category:
                        switch (op)
                        {
                            case TransactionImportFilterOperators.IsEqualTo:
                                filteredTransactions = filteredTransactions.Where(t => t.TransactionData.As(t => string.Equals(t.Category, value, StringComparison.CurrentCultureIgnoreCase)).Or(false));
                                break;
                            case TransactionImportFilterOperators.IsNotEqualTo:
                                filteredTransactions = filteredTransactions.Where(t => t.TransactionData.As(t => !string.Equals(t.Category, value, StringComparison.CurrentCultureIgnoreCase)).Or(false));
                                break;
                            case TransactionImportFilterOperators.Contains:
                                filteredTransactions = filteredTransactions.Where(t => t.TransactionData.As(t => t.Category.Contains(value, StringComparison.CurrentCultureIgnoreCase)).Or(false));
                                break;
                            case TransactionImportFilterOperators.StartsWith:
                                filteredTransactions = filteredTransactions.Where(t => t.TransactionData.As(t => t.Category.StartsWith(value, StringComparison.CurrentCultureIgnoreCase)).Or(false));
                                break;
                            default:
                                throw new InvalidOperationException($"unknown operator {op}");
                        }
                        break;
                    case TransactionImportFilterTargets.Amount:
                        switch (op)
                        {
                            case TransactionImportFilterOperators.IsEqualTo:
                                filteredTransactions = filteredTransactions.Where(t => t.TransactionData.As(t => t.Amount == decimal.Parse(value)).Or(false));
                                break;
                            case TransactionImportFilterOperators.IsNotEqualTo:
                                filteredTransactions = filteredTransactions.Where(t => t.TransactionData.As(t => t.Amount != decimal.Parse(value)).Or(false));
                                break;
                            case TransactionImportFilterOperators.Contains:
                            case TransactionImportFilterOperators.StartsWith:
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
                TotalPages = totalPages,
                PageSize = pageSize.Value
            });
        }

        [HttpPost("import/{id}/start")]
        public async Task<IResult> StartImport(string id)
        {
            var jobId = import.StartImport(id);

            return await componentFactory.RenderComponentAsync(new ProgressPanel
            {
                CallbackEndpoint = $"/transaction-import/import/{jobId}",
                ProgressPercent = 0
            });
        }

        [HttpGet("import/{id}")]
        public async Task<IResult> GetImportStatus(string id)
        {
            var result = import.GetImportResult(id);
            if (!result.IsSuccessful)
            {
                return await componentFactory.RenderComponentAsync(new ProgressPanel
                {
                    CallbackEndpoint = $"/transaction-import/import/{id}",
                    ProgressPercent = result.Reason.Progress
                });
            }

            return await componentFactory.RenderComponentAsync(new TransactionImportResult
            {
                Errors = result.Value.Errors.ToList(),
                TotalTransactions = result.Value.TotalTransactions,
                ImportTag = result.Value.ImportTag,
                IsSuccessful = result.Value.IsSuccessful
            });
        }
    }
}
