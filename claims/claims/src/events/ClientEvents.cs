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
            if (claims.config.NO_ACCESS_WITH_FOR_NOT_CLAIMED_AREA)
            {
                if (accessType == EnumBlockAccessFlags.Use)
                {
                    EnumAreaClaimsWorldAccessResponse response1 = claims.clientDataStorage.clientClaimAreaHandler.CheckAccess(player, blockSel, auxialiry.EnumCANBlockAccessFlags.Use);
                    if (response1 == EnumAreaClaimsWorldAccessResponse.GrantedByFlag)
                    {
                        return EnumWorldAccessResponse.Granted;
                    }
                }
                else if (accessType == EnumBlockAccessFlags.BuildOrBreak)
                {
                    EnumAreaClaimsWorldAccessResponse response1 = claims.clientDataStorage.clientClaimAreaHandler.CheckAccess(player, blockSel, auxialiry.EnumCANBlockAccessFlags.Break);
                    EnumAreaClaimsWorldAccessResponse response2 = claims.clientDataStorage.clientClaimAreaHandler.CheckAccess(player, blockSel, auxialiry.EnumCANBlockAccessFlags.Build);
                    if (response1 == EnumAreaClaimsWorldAccessResponse.GrantedByFlag && response2 == EnumAreaClaimsWorldAccessResponse.GrantedByFlag)
                    {
                        return EnumWorldAccessResponse.Granted;
                    }
                    else if (response1 == EnumAreaClaimsWorldAccessResponse.AreaNotFound || response2 == EnumAreaClaimsWorldAccessResponse.AreaNotFound)
                    {
                        if (player.InventoryManager.ActiveHotbarSlot?.Itemstack?.Block != null &&
                        (claims.config.POSSIBLE_BROKEN_BLOCKS_IN_WILDERNESS.Contains(player.InventoryManager.ActiveHotbarSlot?.Itemstack?.Block.Id ?? 0) ||
                        (blockSel.Position != null && claims.config.POSSIBLE_BROKEN_BLOCKS_IN_WILDERNESS.Contains(player.Entity.World.BlockAccessor.GetBlock(blockSel.Position)?.Id ?? 0))) ||
                        (player.InventoryManager.ActiveHotbarSlot?.Itemstack?.Item != null && claims.config.POSSIBLE_BUILD_ITEMS_IN_WILDERNESS.Contains(player.InventoryManager.ActiveHotbarSlot?.Itemstack?.Item.Id ?? 0)))
                        {
                            return EnumWorldAccessResponse.Granted;
                        }
                    }
                }
                if (claims.clientDataStorage.getFlagValue(blockSel, accessType, out _))
                {
                    return EnumWorldAccessResponse.Granted;
                }
                return EnumWorldAccessResponse.DeniedByMod;
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
