using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Dto.Extensions
{
    public static class TrainingScheduleExtension
    {
        public static bool IsScheduledNow(this TrainingSchedule schedule)
        {
            bool result = false;


            if (schedule == null)
                return false;

            var scheduleInfo = schedule.ScheduleInfo;
            var scheduleType = schedule.ScheduleType;

            switch (scheduleType)
            {
                case ScheduleTypeEnum.Once:
                    result = new OnceOffSchedule(scheduleInfo).Eval();
                    break;

                case ScheduleTypeEnum.Daily:
                    result = new DailySchedule(scheduleInfo).Eval();
                    break;

                case ScheduleTypeEnum.Weekly:
                    result = new WeeklySchedule(scheduleInfo).Eval();
                    break;

                case ScheduleTypeEnum.NoSchedule:
                    result = true;
                    break;

            }

            return result;
        }
    }
}
