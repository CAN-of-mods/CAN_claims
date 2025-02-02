using claims.src.auxialiry;
using claims.src.delayed.teleportation;
using claims.src.part;
using Vintagestory.API.Server;

namespace claims.src.events
{
    public class OnPlayerDisconnect
    {
        public static void Event_OnPlayerDisconnect(IServerPlayer player)
        {
            if(claims.modInstance == null)
            {
                return;
            }
            claims.dataStorage.getPlayerByUid(player.PlayerUID, out PlayerInfo playerInfo);
            if(playerInfo == null)
            {
                return;
            }
            if (playerInfo.AwaitForTeleporation)
            {
                TeleportationHandler.removeTeleportation(playerInfo);
            }
            playerInfo.TimeStampLasOnline = TimeFunctions.getEpochSeconds();
            playerInfo.PlayerCache.Reset();
            playerInfo.saveToDatabase();

            //for future we want to save already sent plots info on client
            if(claims.serverPlayerMovementListener.alreadySentZonesToPlayers.ContainsKey(playerInfo.GetPartName()))
            {
                claims.serverPlayerMovementListener.alreadySentZonesToPlayers.Remove(playerInfo.GetPartName());
            }
        }
    }
}
