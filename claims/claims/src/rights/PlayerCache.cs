using claims.src.auxialiry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace claims.src.rights
{
    public class PlayerCache
    {
        PlotPosition lastChunk;
        public PlotPosition LastChunk { get { return lastChunk; } }
        bool?[] playerCache = new bool?[3];
        public PlayerCache()
        {
            playerCache = new bool?[3]; // use, build, attack in the last plot
        }
        public PlotPosition getLastLocation()
        {
            return lastChunk;
        }
        public void Reset()
        {
            for(int i = 0; i < playerCache.Length; i++)
            {
                playerCache[i] = null;
            }
        }
        public bool?[] getCache()
        {
            return playerCache;
        }
        public void setPlotPosition(PlotPosition loc)
        {
            this.lastChunk = loc;
        }
    }
}
