﻿using Haondt.Web.Assets;
using Midas.Domain.Import.Models;
using Midas.UI.Services.Admin;
using Midas.UI.Services.Transactions;
using Midas.UI.Shared.Models;

namespace Midas.UI.Extensions
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddMidasUI(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(new NavigationSection
            {
                Label = "Reporting",
                Children = new List<NavigationItem>
                {
                    new NavigationLink
                    {
                        Title = "Dashboard",
                        Slug = "dashboard",
                    },
                    new NavigationLink
                    {
                        Title = "Generate Report",
                        Slug = "reports",
                    },
                    new NavigationLink
                    {
                        Title = "Supercategories",
                        Slug = "supercategories",
                    },
                    new NavigationLink
                    {
                        Title = "Categories",
                        Slug = "categories",
                    },
                    new NavigationLink
                    {
                        Title = "Tags",
                        Slug = "tags",
                    },
                }
            });
            services.AddSingleton(new NavigationSection
            {
                Label = "Transactions",
                Children = new List<NavigationItem>
                {
                    new NavigationLink
                    {
                        Title = "Create New",
                        Slug = "transactions/edit"
                    },
                    new NavigationLink
                    {
                        Title = "Search",
                        Slug = "transactions",
                    },
                    new NavigationLink
                    {
                        Title = "Import",
                        Slug = "transaction-import",
                    },
                    new NavigationLink
                    {
                        Title = "Re-Import",
                        Slug = "transaction-import/reimport",
                    },
                    new NavigationLink
                    {
                        Title = "Split",
                        Slug = "split",
                    },
                    new NavigationLink
                    {
                        Title = "Merge",
                        Slug = "merge",
                    },
                    new NavigationLink
                    {
                        Title = "Reconcile",
                        Slug = "reconcile",
                    }
                }
            });
            services.AddSingleton(new NavigationSection
            {
                Label = "Accounts",
                Children = new List<NavigationItem>
                {
                    new NavigationLink
                    {
                        Title = "My Accounts",
                        Slug = "accounts/mine",
                    },
                    new NavigationLink
                    {
                        Title = "All Accounts",
                        Slug = "accounts/all",
                    }
                }
            });
            services.AddSingleton(new NavigationSection
            {
                Label = "Automation",
                Children = new List<NavigationItem>
                {
                    new NavigationLink
                    {
                        Title = "Node-RED",
                        Slug = "node-red",
                    },
                    new NavigationLink
                    {
                        Title = "Node-RED Editor",
                        Slug = "node-red/editor",
                    },
                    new NavigationLink
                    {
                        Title = "Key-Value Store",
                        Slug = "kvs",
                    }
                }
            });
            services.AddSingleton(new NavigationLink
            {
                Title = "Administration",
                Slug = "admin",
            });

            services.Configure<TransactionImportSettings>(configuration.GetSection(nameof(TransactionImportSettings)));
            services.AddSingleton<IAssetSource, ExportMappingsAssetSource>();
            services.AddSingleton<IAssetSource, TakeoutAssetSource>();
            services.AddSingleton<ITransactionFilterService, TransactionFilterService>();

            return services;
        }
    }
}
