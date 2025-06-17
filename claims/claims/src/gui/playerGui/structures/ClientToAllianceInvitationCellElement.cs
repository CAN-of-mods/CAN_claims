using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace claims.src.gui.playerGui.structures
{
    public class ClientToAllianceInvitationCellElement
    {
        public string AllianceName { get; set; }
        public string AllianceGuid { get; set; }
        public long TimeoutStamp { get; set; }
        public ClientToAllianceInvitationCellElement(string allianceName, string allianceGuid, long timeoutStamp)
        {
            AllianceName = allianceName;
            AllianceGuid = allianceGuid;
            TimeoutStamp = timeoutStamp;
        }
    }
}
