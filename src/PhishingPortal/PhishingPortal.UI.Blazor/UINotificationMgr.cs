namespace PhishingPortal.UI.Blazor
{
    public static class UINotificationMgr
    {

        private static List<string> _errors = new List<string>();
        private static List<string> _messages = new List<string>();

        public static void AddError(string message)
        {
            _errors.Add(message);
        }

        public static void AddMessage(string message)
        {
            _messages.Add(message);
        }

        public static List<string> GetErrors()
        {
            var tmp = new List<string>();
            foreach (var error in _errors)
                tmp.Add(error);

            _errors.Clear();
            return tmp;
        }

        public static List<string> GetMessages()
        {
            var tmp = new List<string>();
            foreach (var error in _messages)
                tmp.Add(error);

            _errors.Clear();
            return tmp;
        }

        public static void Clear()
        {
            _errors.Clear();
            _messages.Clear();
        }

    }
}
