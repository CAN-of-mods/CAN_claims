using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using claims.src.part.structure.conflict;

namespace claims.src.gui.playerGui.structures
{
    public class ClientConflictCellElement
    {
        public string Name { get; set; }
        public string FirstAllianceName { get; set; }
        public string SecondAllianceName { get; set; }
        public string StartedByAllianceName { get; set; }
        public ConflictState State { get; set; }
        public string Guid { get; set; }
        public int MinimumDaysBetweenBattles { get; set; } = 6;
        public DateTime LastBattleDateStart { get; set; } = DateTime.MinValue;
        public DateTime LastBattleDateEnd { get; set; } = DateTime.MinValue;
        public List<SelectedWarRange> WarRanges { get; set; } = new List<SelectedWarRange>();
        public long TimeStampCreated { get; set; }
        public ClientConflictCellElement(string name, string firstAllianceName, string secondAllianceName,
                                         string startedByAllianceName, ConflictState state, string guid, List<SelectedWarRange> warRanges, long timeStampCreated)
        {
            Name = name;
            FirstAllianceName = firstAllianceName;
            SecondAllianceName = secondAllianceName;
            StartedByAllianceName = startedByAllianceName;
            State = state;
            Guid = guid;
            WarRanges = warRanges ?? new List<SelectedWarRange>();
            TimeStampCreated = timeStampCreated;
        }
    }
}
