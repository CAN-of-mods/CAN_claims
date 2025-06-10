using claims.src.auxialiry;
using claims.src.part.structure;
using claims.src.part;
using Vintagestory.API.Server;

namespace claims.src.events
{
    public static class OnPVP
    {
        public static bool canPVPAttackHere(IServerPlayer attacker, IServerPlayer defend)
        {
            claims.dataStorage.getPlot(PlotPosition.fromEntityyPos(defend.Entity.ServerPos), out Plot defendPlot);

            claims.dataStorage.getPlayerByUid(attacker.PlayerUID, out PlayerInfo attackerPlayerInfo);
            claims.dataStorage.getPlayerByUid(defend.PlayerUID, out PlayerInfo defendPlayerInfo);

            if (/*attackerPlot != null && */defendPlot != null && defendPlot.hasCity())
            {
                if (defendPlot.Type == PlotType.TOURNAMENT || defendPlot.getPermsHandler().pvpFlag
                    || (defendPlot.getCity().criminals.Contains(defendPlayerInfo) && defendPlot.getCity().isCitizen(attackerPlayerInfo)
                    || (defendPlot.getCity().criminals.Contains(attackerPlayerInfo) && defendPlot.getCity().isCitizen(defendPlayerInfo))))
                {
                    return true;
                }
                if (claims.dataStorage.getWorldInfo().pvpEverywhere)
                {
                    return true;
                }
                if (claims.dataStorage.getWorldInfo().pvpForbidden)
                {
                    return false;
                }
                if (defendPlot.getCity().getPermsHandler().pvpFlag)
                {
                    return true;
                }
                if (defendPlot.getPermsHandler().pvpFlag)
                {
                    return true;
                }
                return false;
            }

            //No plot
            if (claims.dataStorage.getWorldInfo().pvpEverywhere)
            {
                return false;
            }
            if (claims.dataStorage.getWorldInfo().pvpForbidden)
            {
                return false;
            }
            return true;
        }
    }
}
