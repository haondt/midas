using Midas.Core.Models;

namespace Midas.UI.Components.Reporting.TableFactories
{
    public partial class ReportingTableDataFactory
    {
        public required decimal TotalSpending { get; set; }
        public required decimal TotalIncome { get; set; }
        public required decimal TotalCashFlow { get; set; }
        private AbsoluteDateTime _startTime;
        private string _startDateString = default!;
        public required AbsoluteDateTime StartTime
        {
            get { return _startTime; }
            set
            {
                _startTime = value;
                _startDateString = StringFormatter.FormatDate(StartTime);
            }
        }
        private AbsoluteDateTime _endTime;
        private string _endDateString = default!;
        public required AbsoluteDateTime EndTime
        {
            get { return _endTime; }
            set
            {
                _endTime = value;
                _endDateString = StringFormatter.FormatDate(EndTime);
            }
        }

        private decimal AmortizeAmount(decimal amount)
        {
            return Math.Round(amount / (decimal)DurationInMonths, 2);
        }
    }
}
