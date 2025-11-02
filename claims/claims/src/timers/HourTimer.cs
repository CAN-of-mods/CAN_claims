using System.Threading;
using claims.src.auxialiry;
using claims.src.delayed.invitations;
using claims.src.part;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;

namespace claims.src.timers
{
    public class HourTimer
    {
        public void Run()
        {
            //process invites
            InvitationHandler.findAndDeleteOverdueInvitations();

            //prison hours decrease and tp freed players
            foreach(PlayerInfo player in claims.dataStorage.getPlayersDict().Values)
            {
                if(player.PrisonHoursLeft == 1)
                {
                    EntityPos ep = claims.sapi.World.DefaultSpawnPosition;
                    (claims.sapi.World.PlayerByUid(player.Guid) as IServerPlayer).
                        Entity.TeleportToDouble(ep.X, ep.Y, ep.Z);
                    (claims.sapi.World.PlayerByUid(player.Guid) as IServerPlayer).SetSpawnPosition(new PlayerSpawnPos((int)ep.X, (int)ep.Y, (int)ep.Z));
                    player.PrisonHoursLeft = 0;
                }
                else if(player.PrisonHoursLeft > 1)
                {
                    player.PrisonHoursLeft = player.PrisonHoursLeft - 1;
                }
                player.saveToDatabase();
            }

            claims.sapi.Event.RegisterCallback((dt =>
            {
                new Thread(new ThreadStart(() =>
                {
                    new HourTimer().Run();
                })).Start();
            }), (int)TimeFunctions.getSecondsBeforeNextHourStart() * 1000);
        }
    }
}
