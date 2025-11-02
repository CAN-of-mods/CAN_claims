using System.Collections.Generic;
using claims.src.perms;

namespace claims.src.gui.playerGui.structures.cellElements
{
    public class PlotsGroupCellElement
    {
        public string Guid { get; set; }
        public string Name { get; set; }
        public string CityName { get; set; }
        public List<string> PlayersNames { get; set; }
        public PermsHandler PermsHandler { get; set; }
        public double PlotsGroupFee { get; set; }
        public PlotsGroupCellElement(string guid, string name, string cityName, List<string> playersNames, PermsHandler permsHandler, double plotsGroupFee)
        {
            Guid = guid;
            Name = name;
            CityName = cityName;
            PlayersNames = playersNames;
            PermsHandler = permsHandler;
            PlotsGroupFee = plotsGroupFee;
        }
    }
}
