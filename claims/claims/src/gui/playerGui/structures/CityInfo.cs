using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using claims.src.gui.playerGui.structures.cellElements;

namespace claims.src.gui.playerGui.structures
{
    public class CityInfo
    {
        public string Name { get; set; }
        public string MayorName { get; set; }
        public long TimeStampCreated { get; set; }
        public HashSet<string> PlayersNames;
        public int MaxCountPlots { get; set; }
        public int CountPlots { get; set; }
        public string Prefix { get; set; }
        public string AfterName { get; set; }
        public HashSet<string> CityTitles { get; set; } = new();
        public List<RankCellElement> CitizensRanks { get; set; } = new();
        public HashSet<string> PossibleCityRanks { get; set; }
        public int PlotsColor;
        public double CityBalance { get; set; }
        public HashSet<string> Criminals = new();
        public List<PrisonCellElement> PrisonCells { get; set; } = new();
        public List<SummonCellElement> SummonCells { get; set; } = new();
        public List<PlotsGroupCellElement> PlotsGroupCells { get; set; } = new();   
        public List<ClientToAllianceInvitationCellElement> ClientToAllianceInvitations { get; set; } = new();
        public List<ClientConflictCellElement> ClientConflictCellElements { get; set; } = new();
        public List<ClientConflictLetterCellElement> ClientConflictLetterCellElements { get; set; } = new();
        public List<ClientWarRangeCellElement> ClientWarRangeCellElements { get; set; } = new();
        public CityInfo()
        {
            Name = "";
            PossibleCityRanks = new HashSet<string>();
            this.ClientWarRangeCellElements = CreateDefaultWarRangeForWeek();
        }
        public CityInfo(string cityName, string mayorName, long timeStampCreated, HashSet<string> citizens, int maxCountPlots, int countPlots,
            string prefix, string afterName, HashSet<string> cityTitles, int plotsColor, double cityBalance, HashSet<string> criminals)
        {
            Name = cityName;
            MayorName = mayorName;
            TimeStampCreated = timeStampCreated;
            PlayersNames = citizens;
            MaxCountPlots = maxCountPlots;
            CountPlots = countPlots;
            Prefix = prefix;
            AfterName = afterName;
            CityTitles = cityTitles;
            PlotsColor = plotsColor;
            this.CityBalance = cityBalance;
            Criminals = criminals;
            this.ClientWarRangeCellElements = CreateDefaultWarRangeForWeek();
        }
        public static List<ClientWarRangeCellElement> CreateDefaultWarRangeForWeek()
        {
            var warRanges = new List<ClientWarRangeCellElement>();
            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                bool[] defaultRange = new bool[48];
                warRanges.Add(new ClientWarRangeCellElement(day, defaultRange));
            }
            return warRanges;
        }
    }
}
