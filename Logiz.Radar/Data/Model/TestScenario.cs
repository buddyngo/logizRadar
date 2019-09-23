using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Logiz.Radar.Data.Model
{
    public class TestScenario: BaseModel
    {
        [Required]
        public string ScenarioName { get; set; }
        [Required]
        public string ProjectID { get; set; }
    }
}
