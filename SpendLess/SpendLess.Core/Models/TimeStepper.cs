namespace SpendLess.Core.Models
{
    public struct TimeStepper
    {
        public AbsoluteDateTime AbsoluteDateTime { get; private set; }
        public TimeStepSize StepSize { get; private set; }

        public TimeStepper(DateTime dateTime, TimeStepSize stepSize)
        {
            var absoluteDateTime = AbsoluteDateTime.Create(dateTime);
            AbsoluteDateTime = stepSize switch
            {
                TimeStepSize.Day => absoluteDateTime.FloorToLocalDay(),
                TimeStepSize.Month => absoluteDateTime.FloorToLocalMonth(),
                TimeStepSize.Year => absoluteDateTime.FloorToLocalYear(),
                _ => throw new ArgumentException($"Unknown stepsize {stepSize}")
            };

            StepSize = stepSize;
        }

        public TimeStepper(AbsoluteDateTime absoluteDateTime, TimeStepSize stepSize)
        {
            AbsoluteDateTime = absoluteDateTime;
            StepSize = stepSize;
        }

        public TimeStepper Step()
        {
            return StepSize switch
            {
                TimeStepSize.Day => this with { AbsoluteDateTime = AbsoluteDateTime.AddLocalDays(1) },
                TimeStepSize.Month => this with { AbsoluteDateTime = AbsoluteDateTime.AddLocalMonths(1) },
                TimeStepSize.Year => this with { AbsoluteDateTime = AbsoluteDateTime.AddLocalDays(1) },
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
