using System.Collections.Generic;
using claims.src.clientMapHandling;
using ProtoBuf;
using Vintagestory.API.MathTools;

namespace claims.src.playerMovements
{
    [ProtoContract]
    public class ClientSavedZone
    {
        [ProtoMember(1)]
        public long timestamp;
        [ProtoMember(2)]
        public Dictionary<Vec2i, SavedPlotInfo> savedPlots;
        public ClientSavedZone()
        {
            timestamp = 0;
            savedPlots = new Dictionary<Vec2i, SavedPlotInfo>();
        }
        public ClientSavedZone(Dictionary<Vec2i, SavedPlotInfo> dict)
        {
            timestamp = 0;
            savedPlots = dict;
        }
        public void addClientSavedPlots(Vec2i vec, SavedPlotInfo savedPlotInfo)
        {
            this.savedPlots[vec] = savedPlotInfo;
        }
        public bool removeClientSavedPlot(Vec2i vec)
        {
            return this.savedPlots.Remove(vec);
        }
    }
}
