using System.Collections.Generic;
using Vintagestory.API.MathTools;

namespace claims.src.clientMapHandling
{
    public class MapZoneInfo
    {
        public Dictionary<Vec2i, SavedPlotInfo> savedPlotInfos;

        public bool getPlotByPlotCoords(int x, int y, out SavedPlotInfo savedPlot)
        {
            if(savedPlotInfos.TryGetValue(new Vec2i(x, y), out savedPlot))
            {
                return true;
            }
            return false;
        }
        public bool getPlotByPlotVec2i(Vec2i vec, out SavedPlotInfo savedPlot)
        {
            if (savedPlotInfos.TryGetValue(vec, out savedPlot))
            {
                return true;
            }
            return false;
        }
    }
}
