using Logiz.Radar.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Logiz.Radar.Models
{
    public class ProjectViewModel : Project
    {
        public bool CanWrite { get; set; }
    }
}
