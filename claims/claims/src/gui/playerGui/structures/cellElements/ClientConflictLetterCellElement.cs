using claims.src.part.structure.conflict;

namespace claims.src.gui.playerGui.structures.cellElements
{
    public class ClientConflictLetterCellElement
    {
        public string From { get; set; }
        public string FromGuid { get; set; }
        public string To { get; set; }
        public string ToGuid { get; set; }
        public LetterPurpose Purpose { get; set; }
        public long TimeStampExpire { get; set; }
        public string Guid { get; set; }
        public ClientConflictLetterCellElement(string from, string fromGuid, string to, string toGuid, LetterPurpose purpose, long timeStampExpire, string guid)
        {
            From = from;
            To = to;
            Purpose = purpose;
            TimeStampExpire = timeStampExpire;
            Guid = guid;
            FromGuid = fromGuid;
            ToGuid = toGuid;
        }
    }
}
