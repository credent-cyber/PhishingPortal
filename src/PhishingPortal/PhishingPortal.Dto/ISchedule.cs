namespace PhishingPortal.Dto
{
    public interface ISchedule
    {
        bool Eval();
    }

    public class BaseSchedule : ISchedule
    {
        public virtual bool Eval() {
            return false;
        }
    }
}
