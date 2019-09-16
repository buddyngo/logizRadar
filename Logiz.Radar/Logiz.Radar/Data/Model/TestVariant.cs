using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Logiz.Radar.Data.Model
{
    public class TestVariant: BaseModel
    {
        [Required]
        public string VariantName { get; set; }
        [Required]
        public string ScenarioID { get; set; }
    }
}
