using System.Collections.Generic;
using claims.src.rights;

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
