namespace PhishingPortal.Dto
{
    public interface ISchedule
    {
        bool Eval();
        double GetElapsedTimeInMinutes();
    }

    public class BaseSchedule : ISchedule
    {
        public virtual bool Eval()
        {
            return false;
        }

        public virtual double GetElapsedTimeInMinutes() { return -1; }
    }
}
