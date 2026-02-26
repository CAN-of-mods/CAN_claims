using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace claims.src
{
    public interface IConfig
    {
        double NEW_ALLIANCE_COST { get; }
        double ALLIANCE_RENAME_COST { get; }
        double ALLIANCE_MAX_FEE { get; }

        int HOUR_TIMEOUT_INVITATION_TO_ALLIANCE { get; }
        int SECONDS_ALLIANCE_RENAME_COOLDOWN { get; }

        bool NEED_AGREE_FOR_CONFLICT { get; }

        int DELAY_FOR_CONFLICT_ACTIVATED { get; }

        int MINIMUM_DAYS_BETWEEN_BATTLES { get; }
        string AGREEMENT_COMMAND { get; set; }
    }
}
