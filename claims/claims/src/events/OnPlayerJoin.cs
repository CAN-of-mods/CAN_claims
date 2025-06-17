using claims.src.auxialiry;
using claims.src.gui.playerGui.structures;
using claims.src.messages;
using claims.src.network.packets;
using claims.src.part;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace claims.src.events
{
    public class OnPlayerJoin
    {
        public static void Event_OnPlayerJoin(IServerPlayer player)
        {
            //ADD CHAT WINDOWS
            PlayerGroup modChatGroup = claims.sapi.Groups.GetPlayerGroupByName(claims.config.CHAT_WINDOW_NAME);
            PlayerGroupMembership playerClaimsGroup = player.GetGroup(modChatGroup.Uid);
            if (playerClaimsGroup == null)
            {
                PlayerGroupMembership newChatGroup = new PlayerGroupMembership()
                {
                    GroupName = modChatGroup.Name,
                    GroupUid = modChatGroup.Uid,
                    Level = EnumPlayerGroupMemberShip.Member
                };
                player.ServerData.PlayerGroupMemberships.Add(modChatGroup.Uid, newChatGroup);
                modChatGroup.OnlinePlayers.Add(player);
            }

            claims.dataStorage.addToPlayerChatDict(player.PlayerUID, ClaimsChatType.NONE);
            claims.dataStorage.getPlayerByUid(player.PlayerUID, out PlayerInfo playerInfo);

            //NEW PLAYERINFO
            if (playerInfo == null)
            {
                playerInfo = new PlayerInfo(player.PlayerName, player.PlayerUID);
                claims.dataStorage.addPlayer(playerInfo);
                playerInfo.TimeStampLasOnline = TimeFunctions.getEpochSeconds();
                playerInfo.TimeStampFirstJoined = TimeFunctions.getEpochSeconds();
                //MessageHandler.sendMsgToPlayer(player, Lang.Get("claims:new_player_greetings", player.PlayerName));
            }
            else
            {
                processExistedPlayerInfoOnLogin(playerInfo, player);
            }
            
            RightsHandler.reapplyRights(playerInfo);
            playerInfo.PlayerCache.Reset();
            playerInfo.saveToDatabase();

            UsefullPacketsSend.sendAllCitiesColorsToPlayer(player);
            UsefullPacketsSend.SendPlayerCityRelatedInfo(player);
            UsefullPacketsSend.SendUpdatedConfigValues(player);
            if(playerInfo.HasAlliance())
            {
                UsefullPacketsSend.AddToQueuePlayerInfoUpdate(playerInfo.Guid, new Dictionary<string, object> { { "value", playerInfo.Alliance.Guid } }, EnumPlayerRelatedInfo.NEW_ALLIANCE_ALL);
            }

            Dictionary<string, ClientCityInfoCellElement> CityStatsCashe =
                ObjectCacheUtil.GetOrCreate<Dictionary<string, ClientCityInfoCellElement>>(claims.sapi,
                "claims:cityinfocache", () => new Dictionary<string, ClientCityInfoCellElement>());
            UsefullPacketsSend.AddToQueuePlayerInfoUpdate(playerInfo.Guid, new Dictionary<string, object> { { "value", CityStatsCashe.Values.ToList() } }, EnumPlayerRelatedInfo.CITY_LIST_ALL);

            if (claims.config.NO_ACCESS_WITH_FOR_NOT_CLAIMED_AREA)
            {
                if (claims.dataStorage.serverClaimAreaHandler.GetAllClaimAreas() != null)
                {
                    claims.serverChannel.SendPacket(new ClaimAreasPacket()
                    {
                        type = ClaimAreasPacketEnum.Init,
                        claims = claims.dataStorage.serverClaimAreaHandler.GetAllClaimAreas().ToList()

                    }, (IServerPlayer)player);
                }
            }
        }
        public static void processExistedPlayerInfoOnLogin(PlayerInfo playerInfo, IServerPlayer player)
        {
            playerInfo.TimeStampLasOnline = TimeFunctions.getEpochSeconds();
            if(player.PlayerName.Equals(playerInfo.GetPartName()))
            {
                playerInfo.SetPartName(player.PlayerName);
            }
        }
    }
}
