using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Logiz.Radar.Data.Context
{
    public class RadarContextInitializer
    {
        public static void Initialize(RadarContext context)
        {
            context.Database.EnsureCreated();
        }
    }
}
