using PhishingPortal.Common;
using PhishingPortal.Services.Notification.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Services.Notification.Trainings
{
    public interface ITrainingExecutor : IObserver<TraininigInfo>
    {

        IEmailClient EmailSender { get; }
        ILogger<TrainingExecutor> Logger { get; }

        void Start();
        void Stop();

    }
}
