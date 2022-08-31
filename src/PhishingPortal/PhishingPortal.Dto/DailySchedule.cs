using Newtonsoft.Json;
using System;

namespace PhishingPortal.Dto
{
    public class DailySchedule : BaseSchedule
    {
        public DateTime DailyScheduleTime { get; set; }

        public DailySchedule(string value)
        {
            TimeSpan timeSpan = TimeSpan.Parse(value);

            this.DailyScheduleTime = DateTime.MinValue
                                .AddHours(timeSpan.Hours)
                                .AddMinutes(timeSpan.Minutes)
                                .AddSeconds(timeSpan.Seconds);
        }

        public override bool Eval()
        {
            var timespan = TimeSpan.Parse(this.DailyScheduleTime.ToString());

            var minutesSoFar = (DateTime.Now - DateTime.Now.Date).TotalMinutes;

            return minutesSoFar >= timespan.TotalMinutes && minutesSoFar - timespan.TotalMinutes < 10;
        }

        public override string ToString()
        {
            return this.DailyScheduleTime.ToString("HH:mm:ss");
        }
    }
}
