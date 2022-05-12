using Newtonsoft.Json;
using System;

namespace PhishingPortal.Dto
{
    public class DailySchedule : BaseSchedule
    {
        public TimeSpan DailyScheduleTime { get; set; }

        public DailySchedule(string value)
        {
            this.DailyScheduleTime = TimeSpan.Parse(value);
        }

        public override bool Eval()
        {
            var minutesSoFar = (DateTime.Now - DateTime.Now.Date).TotalMinutes;
            return minutesSoFar >= DailyScheduleTime.TotalMinutes && minutesSoFar - DailyScheduleTime.TotalMinutes < 10;
        }

        public override string ToString()
        {
            return this.DailyScheduleTime.ToString();
        }
    }
}
