using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace claims.src.gui.playerGui.structures.cellElements
{
    public class RankCellElement
    {
        public string RankName { get; set; }
        public List<string> CitizensRanks { get; set; }
        public RankCellElement(string rankName, List<string> citizensRanks)
        {
            RankName = rankName;
            CitizensRanks = citizensRanks;
        }
    }
}
