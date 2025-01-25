using Midas.UI.Models.Charts;

namespace Midas.UI.Components.Reporting.ChartFactories
{
    public partial class ReportingChartFactory
    {
        public required List<decimal> CashFlowPerTimeStep { get; set; }
        public required List<string> TimeStepLabels { get; set; }
        public required List<decimal> SpendingPerTimeStep { get; set; }
        public required List<decimal> IncomePerTimeStep { get; set; }
        public required decimal TotalSpending { get; set; }
        public required decimal TotalIncome { get; set; }

        public string SuccessColor { get; set; } = "#48c78e";
        public string DangerColor { get; set; } = "#ff6685";
        public const string TooltipFormatAmountFunction = @"function(context) { 
            const rawValue = context.raw;
            const formattedValue = Math.abs(rawValue).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
            return rawValue < 0 ? `-$${formattedValue}` : `$${formattedValue}`;
        }";
        public const string TooltipFormatPercentageFunction = @"function(context) { 
            const formattedValue = (context.raw * 100).toLocaleString(undefined, { minimumFractionDigits: 1, maximumFractionDigits: 1 });
            return `${formattedValue}%`;
        }";
        public const string TooltipFormatAmountWithLabelFunction = @"function(context) { 
            const rawValue = context.raw;
            const formattedValue = Math.abs(rawValue).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
            return rawValue < 0 ? `${context.dataset.label}: -$${formattedValue}` : `${context.dataset.label}: $${formattedValue}`;
        }";

        public const string TicksFormatAmountFunction = @"function(value) { 
            const formattedValue = Math.abs(value).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
            return value < 0 ? `-$${formattedValue}` : `$${formattedValue}`;
        }";
        public const string HighlightAxisFunction = "({ tick }) => tick.value == 0 ? '#2e333d' : Chart.defaults.borderColor";

        private ChartConfiguration CreateDoughnutChart(
            List<string> labels,
            List<object> data,
            string? tooltipFormatFunction = null)
        {
            var configuration = new ChartConfiguration
            {
                Type = ChartConfigurationType.Doughnut,
                Data = new()
                {
                    Labels = labels,
                    Datasets = new()
                    {
                        new ()
                        {
                            Data = data,
                            BorderWidth = 0,
                            HoverOffset = 0,
                        }
                    }

                }
            };

            if (tooltipFormatFunction != null)
                configuration.Options = new()
                {
                    IndexAxis = "r",
                    Plugins = new()
                    {
                        Tooltip = new()
                        {
                            Callbacks = new()
                            {
                                Label = new()
                                {
                                    Function = tooltipFormatFunction
                                }
                            }
                        }
                    }
                };

            return configuration;
        }
        public ChartConfiguration CreateBarChart(
            List<string> labels,
            List<object> data,
            (string Tooltip, string Ticks)? formatFunction = null)
        {
            var configuration = new ChartConfiguration
            {
                Type = ChartConfigurationType.Bar,
                Data = new()
                {
                    Labels = labels,
                    Datasets = [
                        new() {
                            Axis = "y",
                            Data = data
                        }
                    ]
                },
                Options = new()
                {
                    IndexAxis = "y",

                }
            };

            if (formatFunction.HasValue)
            {
                configuration.Options.Plugins = new()
                {
                    Legend = new()
                    {
                        Display = false
                    },
                    Tooltip = new()
                    {
                        Callbacks = new()
                        {
                            Label = new()
                            {
                                Function = formatFunction.Value.Tooltip
                            }
                        }
                    }
                };
                configuration.Options.Scales = new()
                {
                    X = new()
                    {
                        Ticks = new()
                        {
                            Callback = new()
                            {
                                Function = formatFunction.Value.Ticks
                            }
                        },
                    }
                };
            }

            return configuration;
        }
    }
}
