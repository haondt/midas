using Midas.UI.Models.Charts;

namespace Midas.UI.Components.Reporting.ChartFactories
{
    public partial class ReportingChartFactory
    {
        public ChartConfiguration CreateAggregateCashFlowChart()
        {
            var cashFlowColors = CashFlowPerTimeStep
                .Select(q => q < 0 ? DangerColor : SuccessColor)
                .ToList();

            var aggregatedCashFlowPerTimeStep = new List<object>();
            var currentAggregatedCashFlowPerTimeStep = 0m;
            foreach (var cashFlow in CashFlowPerTimeStep)
            {
                currentAggregatedCashFlowPerTimeStep += cashFlow;
                aggregatedCashFlowPerTimeStep.Add(currentAggregatedCashFlowPerTimeStep);
            }

            return new ChartConfiguration
            {
                Type = ChartConfigurationType.Line,
                Data = new()
                {
                    Labels = TimeStepLabels,
                    Datasets = new()
                    {
                        new()
                        {
                            Data = aggregatedCashFlowPerTimeStep,
                            Tension = 0.35,
                            BorderColor = "#2e333d",
                            PointBackgroundColor = cashFlowColors,
                            PointBorderColor = cashFlowColors,
                            BackgroundColor = "#2e333d",
                        }
                    },
                },
                Options = new()
                {
                    Plugins = new()
                    {
                        Tooltip = new()
                        {
                            Callbacks = new()
                            {
                                Label = new()
                                {
                                    Function = TooltipFormatAmountFunction
                                }
                            }
                        },
                        Legend = new()
                        {
                            Display = false
                        }
                    },
                    Scales = new()
                    {
                        Y = new()
                        {
                            BeginAtZero = true,
                            Grid = new()
                            {
                                Color = new()
                                {
                                    Function = "({ tick }) => tick.value == 0 ? '#2e333d' : Chart.defaults.borderColor"
                                }
                            },
                            Ticks = new()
                            {
                                Callback = new()
                                {
                                    Function = TicksFormatAmountFunction
                                }
                            },
                        }
                    }
                }
            };
        }

        public ChartConfiguration CreateCashFlowTrendChart()
        {
            var trendingCashFlowPerTimeStep = new List<object>();
            var windowPercentageSize = 0.3;
            var windowSize = Math.Max(1, (int)(CashFlowPerTimeStep.Count * windowPercentageSize));
            var currentWindow = 0m;
            for (int i = 0; i < CashFlowPerTimeStep.Count; i++)
            {
                currentWindow += CashFlowPerTimeStep[i];
                if (i >= windowSize)
                    currentWindow -= CashFlowPerTimeStep[i - windowSize];

                var cv = currentWindow / (i >= windowSize ? windowSize : i + 1);
                trendingCashFlowPerTimeStep.Add(currentWindow / (i >= windowSize ? windowSize : i + 1));
            }

            return new ChartConfiguration
            {
                Type = ChartConfigurationType.Line,
                Data = new()
                {
                    Labels = TimeStepLabels,
                    Datasets = new()
                    {
                        new()
                        {
                            Data = trendingCashFlowPerTimeStep,
                            Tension = 0.35,
                        }
                    },
                },
                Options = new()
                {
                    Plugins = new()
                    {
                        Tooltip = new()
                        {
                            Callbacks = new()
                            {
                                Label = new()
                                {
                                    Function = TooltipFormatAmountFunction
                                }
                            }
                        },
                        Legend = new()
                        {
                            Display = false
                        }
                    },
                    Scales = new()
                    {
                        Y = new()
                        {
                            BeginAtZero = true,
                            Grid = new()
                            {
                                Color = new()
                                {
                                    Function = HighlightAxisFunction
                                }
                            },
                            Ticks = new()
                            {
                                Callback = new()
                                {
                                    Function = TicksFormatAmountFunction
                                }
                            },
                        }
                    }
                }
            };
        }

        public ChartConfiguration CreateIncomeSpendingChart()
        {
            return new ChartConfiguration
            {
                Type = ChartConfigurationType.Bar,
                Data = new()
                {
                    Labels = TimeStepLabels,
                    Datasets = new()
                    {
                        new()
                        {
                            Label = "Spending",
                            Data = SpendingPerTimeStep.Cast<object>().ToList(),
                            BackgroundColor = DangerColor
                        },
                        new()
                        {
                            Label = "Income",
                            Data = IncomePerTimeStep.Cast<object>().ToList(),
                            BackgroundColor = SuccessColor
                        },
                    },
                },
                Options = new()
                {
                    Plugins = new()
                    {
                        Tooltip = new()
                        {
                            Callbacks = new()
                            {
                                Label = new()
                                {
                                    Function = TooltipFormatAmountWithLabelFunction
                                }
                            }
                        }
                    },
                    Scales = new()
                    {
                        Y = new()
                        {
                            BeginAtZero = true,
                            Grid = new()
                            {
                                Color = new()
                                {
                                    Function = HighlightAxisFunction
                                }
                            },
                            Ticks = new()
                            {
                                Callback = new()
                                {
                                    Function = TicksFormatAmountFunction
                                }
                            },
                        },
                    }
                }
            };

        }

    }
}
