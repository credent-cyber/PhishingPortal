using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{
    public class Training : Auditable
    {
        public string TrainingName { get; set; }
        public bool IsActive { get; set; }
        public string Content { get; set; }
    }
}
