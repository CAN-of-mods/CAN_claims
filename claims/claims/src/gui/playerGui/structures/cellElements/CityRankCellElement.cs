using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using claims.src.part;
using claims.src.perms;
using claims.src.rights;
using Vintagestory.API.MathTools;

namespace claims.src.gui.playerGui.structures.cellElements
{
    public class CityRankCellElement
    {
        public string Name { get; set; }
        public HashSet<string> Citizens { get; set; }
        public HashSet<EnumPlayerPermissions> Permissions { get; set; }

        public CityRankCellElement(string name, HashSet<string> playersNames, HashSet<EnumPlayerPermissions> permissions)
        { 
            Name = name;
            Citizens = playersNames;
            Permissions = permissions;
        }
    }
}
