using System;

namespace claims.src.gui.playerGui.structures.cellElements
{
    public class ClientWarRangeCellElement
    {
        public DayOfWeek DayOfWeek { get; set; }
        public bool[] WarRangeArray { get; set; } = new bool[48]; // 48 half-hour slots in a day
        public ClientWarRangeCellElement(DayOfWeek dayOfWeek, bool[] warRangeArray)
        {
            DayOfWeek = dayOfWeek;
            WarRangeArray = warRangeArray ?? new bool[48];
        }
    }
}
