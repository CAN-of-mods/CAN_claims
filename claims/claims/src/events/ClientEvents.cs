using claims.src.auxialiry;
using claims.src.playerMovements;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using static claims.src.harmony.harmonyPatches;

namespace claims.src.events
{
    public class ClientEvents
    {
        public static void AddEvents(ICoreClientAPI capi, PlayerMovementListnerClient pmlc)
        {
            capi.Event.RegisterGameTickListener(pmlc.checkPlayerMove, claims.config.DELTA_TIME_PLAYER_POSITION_CHECK_CLIENT);
            capi.Event.RegisterEventBusListener(pmlc.onPlayerChangePlotEvent, 0.5, "claimsPlayerChangePlot");
            capi.Event.LevelFinalize += pmlc.onPlayerJoin;
            capi.Event.OnTestBlockAccess += TestBlockAccessDelegate_1;
        }
        //IPlayer player, BlockSelection blockSel, EnumBlockAccessFlags accessType, ref string claimant, EnumWorldAccessResponse response
        public static EnumWorldAccessResponse TestBlockAccessDelegate_1(IPlayer player, BlockSelection blockSel, EnumBlockAccessFlags accessType, ref string claimant, EnumWorldAccessResponse response)
        {
            if (player.WorldData.CurrentGameMode == EnumGameMode.Creative)
            {
                return EnumWorldAccessResponse.Granted;
            }
            if (claims.clientDataStorage.getFlagValue(blockSel, accessType, out string localClaimant))
            {               
                return EnumWorldAccessResponse.Granted;                 
            }
            else
            {
                return EnumWorldAccessResponse.LandClaimed;
            }

        }
    }
}
