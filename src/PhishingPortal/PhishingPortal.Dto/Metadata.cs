using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{
    public class Metadata
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Expression { get; set; }
    }
}
