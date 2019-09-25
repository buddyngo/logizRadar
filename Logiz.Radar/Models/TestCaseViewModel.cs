using Logiz.Radar.Data.Model;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Logiz.Radar.Models
{
    public class TestCaseViewModel
    {
        public string ProjectID { get; set; }
        public string ProjectName { get; set; }
        public string ScenarioID { get; set; }
        public string ScenarioName { get; set; }
        public string VariantName { get; set; }
        public string TesterName { get; set; }
        public DateTime? FromPlannedDate { get; set; }
        public DateTime? ToPlannedDate { get; set; }
        public string SearchTestStatus { get; set; }
        public TestCase TestCase { get; set; }
        public List<TestCaseAttachment> TestCaseAttachments { get; set; }
    }
}
