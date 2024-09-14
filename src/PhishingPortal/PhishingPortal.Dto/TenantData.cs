namespace PhishingPortal.Dto
{
    public class TenantData : Auditable
    {

        public class Keys
        {
            public const string ConnString = "CONN_STR";
            public const string License = "License";
            public const string PrivateKey = "PRV_KEY";
            public const string PublicKey = "PUB_KEY";
        }

        public string Key { get; set; }
        public string Value { get; set; }
        public int TenantId { get; set; }
    }
}
