namespace PhishingPortal.Dto
{
    public class TenantData : Auditable
    {

        public class Keys
        {
            public const string ConnString = "CONN_STR";
            public const string License = "License";
        }

        public string Key { get; set; }
        public string Value { get; set; }
        public int TenantId { get; set; }
    }
}
