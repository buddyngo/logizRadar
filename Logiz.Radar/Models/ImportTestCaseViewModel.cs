using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Logiz.Radar.Models
{
    public class ImportTestCaseViewModel
    {
        [Required]
        public string ProjectID { get; set; }
        [Required]
        public string ScenarioID { get; set; }
        [Required]
        public IFormFile DataFile { get; set; }
    }
}
