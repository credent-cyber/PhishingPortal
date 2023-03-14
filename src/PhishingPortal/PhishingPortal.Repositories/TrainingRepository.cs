using Microsoft.Extensions.Logging;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;
using Microsoft.EntityFrameworkCore;

namespace PhishingPortal.Repositories
{
    public class TrainingRepository : BaseRepository, ITrainingRepository
    {
        public TenantDbContext DbContext { get; }

        public TrainingRepository(ILogger logger, TenantDbContext dbContext)
            : base(logger)
        {
            DbContext = dbContext;
        }

        public async Task<IEnumerable<(Recipient, TrainingLog, Training)>> GetMyTrainings(string email)
        {
            var recipient = from r in DbContext.Recipients
                            join tl in DbContext.TrainingLog on r.Id equals tl.ReicipientID
                            join t in DbContext.Training on tl.TrainingID equals t.Id
                            where r.Email == email
                            select new { r, t, tl };

            var result = new List<(Recipient, TrainingLog, Training)>();

            foreach (var r in recipient)
            {
                result.Add((r.r, r.tl, r.t));
            }

            return await Task.FromResult(result);
        }

        public async Task<(Training Training, TrainingLog TrainingLog)> GetTrainingByUniqueID(Guid uniqueID, string email)
        {
            var trainingLog = DbContext.TrainingLog.FirstOrDefault(o => o.UniqueID == uniqueID);

            try
            {
                if (trainingLog == null)
                    throw new InvalidOperationException("Training not found");

                var recipient = DbContext.Recipients.FirstOrDefault(o => o.Email == email && trainingLog.ReicipientID == o.Id);

                if (recipient == null)
                    throw new ArgumentException("The training is not assigned to the currently logged in user");

                if (trainingLog == null)
                    throw new ArgumentNullException("No training found");

                var training = DbContext.Training.Find(trainingLog.TrainingID);

                return await Task.FromResult((training, trainingLog));

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw;
            }
        }

        public async Task<TrainingLog> UpdateTrainingProgress(Guid uniqueID, decimal percentage, string email)
        {
            var trainingLog = await DbContext.TrainingLog.FirstOrDefaultAsync(o => o.UniqueID == uniqueID);

            if (trainingLog == null)
                throw new InvalidOperationException("Training not found");

            var recipient = DbContext.Recipients.FirstOrDefault(o => o.Email == email && trainingLog.ReicipientID == o.Id);

            if (recipient == null)
                throw new ArgumentException("The training is not assigned to the currently logged in user");


            if (percentage > trainingLog.PercentCompleted)
            {
                trainingLog.PercentCompleted = percentage;
                DbContext.SaveChanges();
            }

            return await Task.FromResult(trainingLog);
        }


    }
}