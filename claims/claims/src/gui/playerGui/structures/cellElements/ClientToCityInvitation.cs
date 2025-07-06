using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace claims.src.gui.playerGui.structures.cellElements
{
    public class ClientToCityInvitation
    {
        public string CityName { get; set; }
        public long TimeoutStamp { get; set; }
        public ClientToCityInvitation(string cityName, long timeoutStamp)
        {
            CityName = cityName;
            TimeoutStamp = timeoutStamp;
        }
    }
}
