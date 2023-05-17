namespace PhishingPortal.Dto.Extensions
{
    public static class ScheduleExtensions
    {
        /// <summary>
        /// Check if schedule is now
        /// </summary>
        /// <param name="scheduleType"></param>
        /// <param name="scheduleInfo"></param>
        /// <returns></returns>
        public static bool IsScheduledNow(this CampaignSchedule schedule)
        {
            bool result = false;


            if(schedule == null)
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

        public static BaseSchedule ToActualScheduleType(this CampaignSchedule schedule)
        {
            if (schedule == null)
                return null;

            var scheduleInfo = schedule.ScheduleInfo;
            var scheduleType = schedule.ScheduleType;

            switch (scheduleType)
            {
                case ScheduleTypeEnum.Once:
                    return new OnceOffSchedule(scheduleInfo);

                case ScheduleTypeEnum.Daily:
                    return new DailySchedule(scheduleInfo);

                case ScheduleTypeEnum.Weekly:
                    return new WeeklySchedule(scheduleInfo);

                case ScheduleTypeEnum.NoSchedule:
                    return null;

                default: return null;

            }
        }
    }
}
