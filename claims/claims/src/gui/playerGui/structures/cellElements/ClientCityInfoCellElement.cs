using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace claims.src.gui.playerGui.structures.cellElements
{
    public class ClientCityInfoCellElement
    {
        public int CitizensAmount { get; set; }
        public string MayorName { get; set; }
        public int ClaimedPlotsAmount { get; set; }
        public string AllianceName { get; set; }
        public long TimeStampCreated { get; set; }
        public string Name { get; set; }
        public bool Open { get; set; }
        public string InvMsg { get;set; }
        public string Guid { get; set; }
        public ClientCityInfoCellElement(int citizensAmount, string mayorName, int claimedPlotsAmount,
                                         string allianceName, long timeStampCreated, string name, bool open, string invMsg, string guid)
        {
            CitizensAmount = citizensAmount;
            MayorName = mayorName;
            ClaimedPlotsAmount = claimedPlotsAmount;
            AllianceName = allianceName;
            TimeStampCreated = timeStampCreated;
            Name = name;
            Open = open;
            InvMsg = invMsg;
            Guid = guid;
        }
    }
}
