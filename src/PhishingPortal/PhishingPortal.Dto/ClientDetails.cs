using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{
    public class ClientDetails
    {
        public string Ip { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string Browser { get; set; }
        public string OS { get; set; }
    }

    public class CampaignHitRequest
    {
        public string Key { get; set; }
        public ClientDetails ClientDetails { get; set; }
    }
}
