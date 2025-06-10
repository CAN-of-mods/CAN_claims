using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace claims.src.gui.playerGui.structures
{
    public class ClientToPlotsGroupInvitation
    {
        public string CityName { get; set; }
        public string PlotsGroupName { get; set; }
        public long TimeoutStamp { get; set; }
        public ClientToPlotsGroupInvitation(string cityName, string plotsGroupName, long timeoutStamp)
        {
            CityName = cityName;
            PlotsGroupName = plotsGroupName;
            TimeoutStamp = timeoutStamp;
        }
    }
}
