using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.MathTools;

namespace claims.src.gui.playerGui.structures
{
    public class PrisonCellElement
    {
        public Vec3i SpawnPosition { get; set; }
        public HashSet<string> Players { get; set; }
        public PrisonCellElement(Vec3i spawn, HashSet<string> players)
        {
            SpawnPosition = spawn;
            Players = players;
        }
    }
}
