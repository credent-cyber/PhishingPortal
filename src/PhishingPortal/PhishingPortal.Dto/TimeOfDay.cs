using System;

namespace PhishingPortal.Dto
{
    public class TimeOfDay
    {
        public int Hours { get; set; }
        public int Minutes { get; set; }

        public TimeOfDay(string value)
        {
            Hours = int.Parse(value.Split(':')[0]);
            Minutes = int.Parse(value.Split(':')[1]);
        }

        public override string ToString()
        {
            return $"{Hours.ToString().PadLeft(2)}:{Minutes.ToString().PadLeft(2)}";
        }

        public TimeSpan ToTimeSpan()
        {
            return TimeSpan.Parse(this.ToString());
        }

    }
}
