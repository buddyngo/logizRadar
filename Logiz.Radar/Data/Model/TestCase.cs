using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Logiz.Radar.Data.Model
{
    public enum TestCaseStatus
    {
        Open,
        Passed,
        Failed,
        Pending,
        Hold
    }

    public class TestCase: BaseModel
    {
        [Required]
        public string TestCaseName { get; set; }
        [Required]
        public string TestVariantID { get; set; }
        [Required]
        public string TestCaseSteps { get; set; }
        [Required]
        public string ExpectedResult { get; set; }
        public string ActualResult { get; set; }
        public string TesterName { get; set; }
        public DateTime? PlannedDate { get; set; }
        [Required]
        public TestCaseStatus TestStatus { get; set; }
        public string Note { get; set; }
    }
}
