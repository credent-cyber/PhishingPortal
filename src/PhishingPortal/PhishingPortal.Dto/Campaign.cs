using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{
    public class Campaign : BaseEntity
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public DateTime ScheduleDateTime { get; set; }
    }
}
