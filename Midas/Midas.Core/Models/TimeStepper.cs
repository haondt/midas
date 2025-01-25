namespace Midas.Core.Models
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
                TimeStepSize.Week => absoluteDateTime.FloorToLocalDay(),
                TimeStepSize.Month => absoluteDateTime.FloorToLocalMonth(),
                TimeStepSize.Quarter => absoluteDateTime.FloorToLocalMonth(),
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
                TimeStepSize.Week => this with { AbsoluteDateTime = AbsoluteDateTime.AddLocalDays(7) },
                TimeStepSize.Month => this with { AbsoluteDateTime = AbsoluteDateTime.AddLocalMonths(1) },
                TimeStepSize.Quarter => this with { AbsoluteDateTime = AbsoluteDateTime.AddLocalMonths(3) },
                TimeStepSize.Year => this with { AbsoluteDateTime = AbsoluteDateTime.AddLocalYears(1) },
                _ => throw new ArgumentException($"Unknown stepsize {StepSize}")
            };
        }

    }

    public enum TimeStepSize
    {
        Day,
        Week,
        Month,
        Quarter,
        Year
    }
}
