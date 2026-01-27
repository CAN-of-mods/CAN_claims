using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using claims.src.part;

namespace claims.src.auxialiry.ClaimLimiter
{
    public abstract class ClaimLimiter
    {
        public abstract bool CanClaimHere(PlayerInfo playerInfo, PlotPosition plotPosition);

    }
}
