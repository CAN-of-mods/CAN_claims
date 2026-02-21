using System;
using System.Collections.Generic;
using Vintagestory.API.Server;

namespace claims.src.timers
{
    public class BackupScheduler
    {
        private readonly ICoreServerAPI sapi;
        private readonly HashSet<string> executedToday = new HashSet<string>();
        private DateTime? lastBackupTime = null;
        private int lastDay = -1;
        public BackupScheduler(ICoreServerAPI sapi)
        {
            this.sapi = sapi;
        }

        public void Tick()
        {
            DateTime now = DateTime.Now;

            if (now.Day != lastDay)
            {
                executedToday.Clear();
                lastDay = now.Day;
            }

            foreach (var it in claims.config.DAYTIME_MAKE_BACKUP)
            {
                var parts = it.Split(':');
                if (parts.Length != 2) continue;

                if (!int.TryParse(parts[0], out int hour)) continue;
                if (!int.TryParse(parts[1], out int minute)) continue;

                DateTime scheduled =
                    new DateTime(now.Year, now.Month, now.Day, hour, minute, 0);

                if (now >= scheduled &&
                    (lastBackupTime == null || lastBackupTime < scheduled))
                {
                    MakeBackup();
                    lastBackupTime = now;
                }
            }
        }

        private void MakeBackup()
        {
            string timeString = DateTime.Now.ToString("ss_mm_HH_dd_MM_yyyy");
            claims.getModInstance()
                  .getDatabaseHandler()
                  .makeBackup(timeString);

            sapi.Logger.Notification("[Claims] Backup created at " + timeString);
        }
    }
}
