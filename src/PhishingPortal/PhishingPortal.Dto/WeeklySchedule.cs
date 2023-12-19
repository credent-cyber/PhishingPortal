using Newtonsoft.Json;
using System;

namespace PhishingPortal.Dto
{
    public class WeeklySchedule : BaseSchedule
    {
        public DayOfWeek WeekDay { get; set; }

        public DateTime ScheduleTime { get; set; }

        public WeeklySchedule(string value)
        {
            var weekDayStr = value.Split("|")[0];
            var timespan = TimeSpan.Parse(value.Split("|")[1]);


            this.WeekDay = Enum.Parse<DayOfWeek>(weekDayStr);
            this.ScheduleTime = DateTime.MinValue.AddHours(timespan.Hours)
                .AddMinutes(timespan.Minutes)
                .AddSeconds(timespan.Seconds);
        }

        public override bool Eval()
        {
            var weekDay = DateTime.Now.DayOfWeek;
            if (weekDay == WeekDay)
            {
                TimeSpan timespan = TimeSpan.Parse(this.ScheduleTime.ToString("HH:mm:ss"));
                var minutesSoFar = (DateTime.Now - DateTime.Now.Date).TotalMinutes;
                return minutesSoFar >= timespan.TotalMinutes && minutesSoFar - timespan.TotalMinutes < 10;
            }
            return false;
        }

        public override string ToString()
        {
            return $"{WeekDay}|{ScheduleTime.ToString("HH:mm:ss")}";
        }

        public override double GetElapsedTimeInMinutes()
        {
            throw new NotImplementedException();
        }
    }
}
