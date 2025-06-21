using System;
using System.Collections.Generic;

namespace claims.src.part.structure.conflict
{
    public class Conflict : Part
    {
        public Conflict(string val, string guid) : base(val, guid)
        {
        }
        public Alliance First { get; set; }
        public Alliance Second { get; set; }
        public Alliance StartedBy { get; set; }
        public ConflictState State { get; set; }
        public List<SelectedWarRange> WarRanges { get; set; } = new List<SelectedWarRange>();
        public int MinimumDaysBetweenBattles { get; set; } = 6;
        public DateTime LastBattleDateStart { get; set; } = DateTime.MinValue;
        public DateTime LastBattleDateEnd { get; set; } = DateTime.MinValue;
        public long TimeStampStarted { get; set; }
        public int[] SelectedRanges { get; set; }
        public override bool saveToDatabase(bool update = true)
        {
            claims.getModInstance().getDatabaseHandler().saveConflict(this, update);
            return true;
        }
        public bool MinimumDaysHasPassed()
        {
            if (LastBattleDateStart == DateTime.MinValue)
            {
                return true;
            }
            TimeSpan timeSinceLastBattle = DateTime.Now - LastBattleDateStart;
            return timeSinceLastBattle.TotalDays >= MinimumDaysBetweenBattles;
        }
        public int MinutesTillMinPassed()
        {
            if (LastBattleDateStart == DateTime.MinValue)
            {
                return 0;
            }
            TimeSpan timeSinceLastBattle = DateTime.Now - LastBattleDateStart;
            int minutesPassed = (int)timeSinceLastBattle.TotalMinutes;
            return Math.Max(0, MinimumDaysBetweenBattles * 24 * 60 - minutesPassed);
        }
    }
}
