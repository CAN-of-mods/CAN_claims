using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace claims.src.part.structure.plots
{
    public class PlotDescPrison : PlotDesc
    {
        public string prisonGuid { get; set; }
        public PlotDescPrison(string prisonguid)
        {
            this.prisonGuid = prisonguid;
        }
        public void fromStringPoint(string val)
        {
            prisonGuid = val;
        }
        public string getPrisonGuid()
        {
            return prisonGuid;
        }
    }
}
