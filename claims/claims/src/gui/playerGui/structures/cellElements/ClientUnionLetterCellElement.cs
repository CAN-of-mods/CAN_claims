using claims.src.part.structure.conflict;

namespace claims.src.gui.playerGui.structures.cellElements
{
    public class ClientUnionLetterCellElement
    {
        public string From { get; set; }
        public string FromGuid { get; set; }
        public string To { get; set; }
        public string ToGuid { get; set; }
        public long TimeStampExpire { get; set; }
        public string Guid { get; set; }
        public ClientUnionLetterCellElement(string from, string fromGuid, string to, string toGuid, long timeStampExpire, string guid)
        {
            From = from;
            To = to;
            TimeStampExpire = timeStampExpire;
            Guid = guid;
            FromGuid = fromGuid;
            ToGuid = toGuid;
        }
    }
}
