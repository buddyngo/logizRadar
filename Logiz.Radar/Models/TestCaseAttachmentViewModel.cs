using Logiz.Radar.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Logiz.Radar.Models
{
    public class TestCaseAttachmentViewModel
    {
        public string TesterName { get; set; }
        public DateTime? FromPlannedDate { get; set; }
        public DateTime? ToPlannedDate { get; set; }
        public DateTime? FromUpdatedDate { get; set; }
        public DateTime? ToUpdatedDate { get; set; }
        public string SearchTestStatus { get; set; }
        public TestCaseAttachment TestCaseAttachment { get; set; }
    }
}
