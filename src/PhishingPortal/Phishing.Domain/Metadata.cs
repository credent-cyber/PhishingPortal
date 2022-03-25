using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhishingPortal.Domain
{
    public class Metadata
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public int Value { get; set; }
        public int Expression { get; set; }
    }
}
