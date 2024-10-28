using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Services.Notification.TrainingReminder
{
    public interface ITrainingRemindProvider
    {
        Task CheckAndPublish(CancellationToken stopppingToken);
    }
}
