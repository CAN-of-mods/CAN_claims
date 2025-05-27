using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.MathTools;

namespace claims.src.gui.playerGui.structures
{
    public class SummonCellElement
    {
        public Vec3i SpawnPosition { get; set; }
        public string Name { get; set; }
        public SummonCellElement(Vec3i spawnPosition, string name)
        {
            this.SpawnPosition = spawnPosition;
            this.Name = name;
        }
    }
}
