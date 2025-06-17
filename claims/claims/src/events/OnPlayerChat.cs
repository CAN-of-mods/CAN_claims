using claims.src.auxialiry;
using claims.src.messages;
using claims.src.part;
using System.Text.RegularExpressions;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;

namespace claims.src.events
{
    public class OnPlayerChat
    {
        public static void onPlayerChat(IServerPlayer player, int channelId, ref string message, ref string data, BoolRef consumed)
        {

            claims.dataStorage.getPlayerByUid(player.PlayerUID, out PlayerInfo playerInfo);
            ClaimsChatType chat;
            claims.dataStorage.getPlayerChatDict().TryGetValue(player.PlayerUID, out chat);
            if (chat == ClaimsChatType.LOCAL)
            {
                MessageHandler.sendLocalMsg(player.Entity.ServerPos.XYZ, message);
                consumed.value = true;
                return;
            }

            if (channelId != claims.dataStorage.getModChatGroup().Uid && chat == ClaimsChatType.NONE)
            {
                if (playerInfo.hasCity())
                {
                    //message = "<font color=#FFFFFF><strong>PREFIX</strong></font>" + message;
                    Match somePrefix = Regex.Match(message, "<font(.+)/font>");
                    
                    Match onlyMsg = Regex.Match(message, "> (.+)");



                    //prefix[cityName] playerName message
                    message = string.Format("{0}{1}{2}{3}{4}{5} {6}", 
                        somePrefix.Success
                            ? somePrefix.Value
                            : "",
                        playerInfo.HasAlliance() && playerInfo.Alliance.Prefix.Length > 0
                            ? StringFunctions.setStringColor("[", ColorsClaims.WHITE) +
                                StringFunctions.setBold(StringFunctions.setStringColor(playerInfo.Alliance.Prefix, claims.config.ALLIANCE_COLOR_NAME)) +
                                StringFunctions.setStringColor("]", ColorsClaims.WHITE)
                            : "",
                        StringFunctions.setStringColor("[", ColorsClaims.WHITE),
                        StringFunctions.setBold(StringFunctions.setStringColor(StringFunctions.replaceUnderscore(playerInfo.City.GetPartName()), claims.config.CITY_COLOR_NAME)),
                        StringFunctions.setStringColor("]", ColorsClaims.WHITE),
                        playerInfo.getNameForChat(),
                        onlyMsg.Groups[1].Value
                        );
                }
                else
                {

                }
                return;
            }

            if (chat == ClaimsChatType.CITY)
            {
                MessageHandler.sendMsgInCity(playerInfo.City, message, false);
                consumed.value = true;
            }
            else if (chat == ClaimsChatType.ALLIANCE)
            {
                MessageHandler.SendMsgInAlliance(playerInfo.Alliance, message);
                consumed.value = true;
            }

            if (channelId == claims.dataStorage.getModChatGroup()?.Uid)
            {
                consumed.value = true;
                return;
            }
        }
    }
}
