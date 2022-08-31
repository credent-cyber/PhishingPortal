namespace PhishingPortal.Services.Notification
{
    /// <summary>
    /// Unsubscriber
    /// </summary>
    /// <typeparam name="EmailCampaignInfo"></typeparam>
    internal class Unsubscriber<T> : IDisposable where T : class
    {
        private List<IObserver<T>> _observers;
        private IObserver<T> _observer;

        internal Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }
}
