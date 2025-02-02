using claims.src.auxialiry.claimAreas;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace claims.src.network.packets
{
    [ProtoContract]
    public class ClaimAreasPacket
    {
        [ProtoMember(1)]
        public List<ClaimArea> claims = new List<ClaimArea>();
        [ProtoMember(2)]
        public ClaimAreasPacketEnum type;
    }

    public enum ClaimAreasPacketEnum
    {
        Init, Add, Remove, Update
    }
}
