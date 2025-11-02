using ProtoBuf;

namespace claims.src.network.packets
{
    [ProtoContract]
    public class SavedPlotsPacket
    {
        [ProtoMember(1)]
        public PacketsContentEnum type;
        [ProtoMember(2)]
        public string data;
    }
}
