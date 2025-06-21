using claims.src.auxialiry;
using claims.src.delayed.cooldowns;
using claims.src.delayed.teleportation;
using claims.src.part.structure.conflict;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vintagestory.API.Server;

namespace claims.src.timers
{
    public class TimerGeneral
    {
        public static void StartServerTimers(ICoreServerAPI sapi)
        {
            //Start new day sequence
            var returnId = sapi.Event.RegisterCallback((dt =>
            {
                new Thread(new ThreadStart(() =>
                {
                    new DayTimer().Run(true);
                })).Start();
            }), (int)TimeFunctions.getSecondsBeforeNextDayStart() * 1000);

            sapi.Logger.Debug("StartTimers:newdayTimer, handlerID " + returnId);

            //Start new hour sequence
            returnId = sapi.Event.RegisterCallback((dt =>
            {
                new Thread(new ThreadStart(() =>
                {
                    new HourTimer().Run();
                })).Start();
            }), (int)TimeFunctions.getSecondsBeforeNextHourStart() * 1000);

            //Start timer for cooldown processing
            sapi.Event.Timer((() =>
            {
                CooldownHandler.processCooldowns();
            }
            ), 1);
            sapi.Event.Timer((() =>
            {
                TeleportationHandler.UpdateTeleportations();
            }
            ), 1);

            sapi.Event.Timer((() =>
            {
                UsefullPacketsSend.SendAllCollectedCityUpdatesToCitizens();
            }
            ), 20);

            //Start timer for conflicts handler
            claims.sapi.Event.Timer((() =>
            {
                ConflictHandler.updateConflictLetters();
            }
            ), 300);
        }
    }
}
