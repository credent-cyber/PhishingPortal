using PhishingPortal.Dto;

namespace PhishingPortal.Services.Notification.Trainings
{
    public class TraininigInfo
    {
        public string Tenantdentifier { get; set; }
        public string TrainingRecipients { get; set; }
        public string TrainingSubject { get; set; }
        public string TrainingContent { get; set; }
        public string TrainingFrom { get; set; }
        public TrainingLog TrainingLogEntry { get; set; }
    }
}
