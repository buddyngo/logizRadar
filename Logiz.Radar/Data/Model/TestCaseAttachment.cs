using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Logiz.Radar.Data.Model
{
    public class TestCaseAttachment : BaseModel
    {
        public string TestCaseID { get; set; }
        public string FullFileName { get; set; }
        public string OriginalFileName { get; set; }
        public string ContentType { get; set; }
    }
}
