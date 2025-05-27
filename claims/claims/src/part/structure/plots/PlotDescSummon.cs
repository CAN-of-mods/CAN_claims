using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Vintagestory.API.MathTools;

namespace claims.src.part.structure.plots
{
    public class PlotDescSummon: PlotDesc
    {
        public Vec3d SummonPoint { get; set; }
        public string Name { get; set; } = "";
        public PlotDescSummon(Vec3d summonPoint)
        {
            this.SummonPoint = summonPoint;
        }
        public PlotDescSummon()
        {
        }
        public void fromStringPoint(string val)
        {
            SummonPoint = JsonSerializer.Deserialize<Vec3d>(val);
        }
    }
}
