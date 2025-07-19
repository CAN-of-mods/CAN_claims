using claims.src.auxialiry;
using claims.src.database;
using claims.src.delayed.cooldowns;
using claims.src.delayed.teleportation;
using claims.src.part.structure.conflict;
using Quartz;
using Quartz.Impl;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vintagestory.API.Server;

namespace claims.src.timers
{
    public class TimerGeneral
    {
        public static async Task StartServerTimers(ICoreServerAPI sapi)
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

            var cronBuilder = await StdSchedulerFactory.GetDefaultScheduler();
            await cronBuilder.Start();

            var job = JobBuilder.Create<MakeCronBackupJob>()
                .WithIdentity("dayTimer", "claims")
                .StoreDurably()
                .Build();
            await cronBuilder.AddJob(job, true);

            foreach (var it in claims.config.DAYTIME_MAKE_BACKUP)
            {
                var parts = it.Split(':');
                if (parts.Length > 2 || parts.Length < 1)
                {
                    continue;
                }
                
                try
                {
                    int hour = int.Parse(parts[0]);
                    int minute = int.Parse(parts[1]);
                    string dayTime = $"0 {minute} {hour} * * ?";
                    var trigger = TriggerBuilder.Create()
                        .WithIdentity($"dayTimerTrigger_{hour}_{minute}" + it, "claims")
                        .WithSchedule(CronScheduleBuilder.CronSchedule(dayTime))
                        .ForJob(job)
                        .Build();
                    await cronBuilder.ScheduleJob(trigger);
                }
                catch (Exception e)
                {
                    sapi.Logger.Error("Error creating cron trigger for backup: " + e.Message);
                    continue;
                }
                
            }

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
            ), claims.config.CHECK_FOR_PACKETS_TO_SEND_EVERY_N_SECONDS);

            //Start timer for conflicts handler
            claims.sapi.Event.Timer((() =>
            {
                ConflictHandler.updateConflictLetters();
            }
            ), 300);
        }
    }
}
