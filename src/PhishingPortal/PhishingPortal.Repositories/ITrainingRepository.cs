using PhishingPortal.Dto;

namespace PhishingPortal.Repositories
{
    public interface ITrainingRepository
    {
        Task<IEnumerable<(Recipient Recipient, TrainingLog TrainingLog, Training Training)>> GetMyTrainings(string email);
        Task<(Training Training, TrainingLog TrainingLog)> GetTrainingByUniqueID(Guid uniqueID, string email);
        Task<TrainingLog> UpdateTrainingProgress(Guid uniqueID, decimal percentage, string email, string checkpoint);
    }
}