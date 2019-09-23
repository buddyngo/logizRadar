using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Logiz.Radar.Data.Model
{
    public class Project : BaseModel
    {
        [Required]
        public string ProjectName { get; set; }
    }
}
