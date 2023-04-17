using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.Dto
{
    public class TrainingVideo : BaseEntity
    {
        [Required]
        public string VideoTitle { get; set; }
        public string FilePath { get; set; }
    }
}
