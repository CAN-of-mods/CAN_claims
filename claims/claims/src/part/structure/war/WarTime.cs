using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using claims.src.auxialiry;

namespace claims.src.part.structure.war
{
    public class WarTime
    {
        public string ConflictGuid { get; set; }
        public DateTime BattleDateStart { get; set; } = DateTime.UnixEpoch;
        public DateTime BattleDateEnd { get; set; } = DateTime.UnixEpoch;
        public Dictionary<PlotPosition, PlotAttack> PlotAttacks { get; set; }
        public WarTime(string ConflictGuid, DateTime start, DateTime end) 
        {
            this.ConflictGuid = ConflictGuid;
            PlotAttacks = new Dictionary<PlotPosition, PlotAttack>();
            BattleDateStart = start;
            BattleDateEnd = end;
        }
    }
}
