namespace PhishingPortal.Dto
{
    public class TenantData : Auditable
    {

        public class Keys
        {
            public const string ConnString = "CONN_STR";
        }

        public string Key { get; set; }
        public string Value { get; set; }

    }
}
