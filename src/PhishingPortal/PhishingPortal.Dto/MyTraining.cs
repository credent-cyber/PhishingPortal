namespace PhishingPortal.Dto
{
    public class MyTraining : BaseEntity
    {
        public Training Training { get; set; }
        public TrainingLog TrainingLog { get; set; }
    }
}
