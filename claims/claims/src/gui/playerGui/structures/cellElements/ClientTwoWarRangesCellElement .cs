using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace claims.src.gui.playerGui.structures.cellElements
{
    public class ClientTwoWarRangesCellElement
    {
        public DayOfWeek DayOfWeek { get; set; }
        public bool[] OurWarRangeArray { get; set; } = new bool[48]; // 48 half-hour slots in a day
        public bool[] EnemyWarRangeArray { get; set; } = new bool[48]; // 48 half-hour slots in a day
        public ClientTwoWarRangesCellElement(DayOfWeek dayOfWeek, bool[] ourWarRangeArray, bool[] enemyWarRangeArray)
        {
            DayOfWeek = dayOfWeek;
            OurWarRangeArray = ourWarRangeArray ?? new bool[48];
            EnemyWarRangeArray = enemyWarRangeArray ?? new bool[48];
        }
    }
}
