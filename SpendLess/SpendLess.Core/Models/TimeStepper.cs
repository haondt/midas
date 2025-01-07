namespace SpendLess.Core.Models
{
    public struct TimeStepper
    {
        public DateTime DateTime { get; private set; }
        public TimeStepSize StepSize { get; private set; }

        public TimeStepper(DateTime dateTime, TimeStepSize stepSize)
        {
            DateTime = stepSize switch
            {
                TimeStepSize.Day => new DateTime(dateTime.Year, dateTime.Month, dateTime.Day),
                TimeStepSize.Month => new DateTime(dateTime.Year, dateTime.Month, 1),
                TimeStepSize.Year => new DateTime(dateTime.Year, 1, 1),
                _ => throw new ArgumentException($"Unknown stepsize {stepSize}")
            };

            StepSize = stepSize;
        }

        public TimeStepper Step()
        {
            return StepSize switch
            {
                TimeStepSize.Day => this with { DateTime = DateTime.AddDays(1) },
                TimeStepSize.Month => this with { DateTime = DateTime.AddMonths(1) },
                TimeStepSize.Year => this with { DateTime = DateTime.AddYears(1) },
                _ => throw new ArgumentException($"Unknown stepsize {StepSize}")
            };
        }

    }

    public enum TimeStepSize
    {
        Day,
        Month,
        Year
    }
}
