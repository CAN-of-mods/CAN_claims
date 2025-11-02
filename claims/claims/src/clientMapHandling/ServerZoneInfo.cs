using System.Collections.Generic;
using claims.src.part.structure;

namespace claims.src.clientMapHandling
{
    public class ServerZoneInfo
    {
        public long timestamp;
        public HashSet<Plot> zonePlots = new HashSet<Plot>();
    }
}
