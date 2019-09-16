using Logiz.Radar.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Logiz.Radar.Models
{
    public class TestCaseViewModel
    {
        public string ProjectID { get; set; }
        public string ScenarioID { get; set; }
        public TestCase TestCase { get; set; }
    }
}
