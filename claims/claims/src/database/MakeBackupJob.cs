using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;

namespace claims.src.database
{
    public class MakeCronBackupJob: IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            string timeString = DateTime.Now.ToString("ss_mm_HH_dd_MM_yyyy");
            claims.getModInstance().getDatabaseHandler().makeBackup(timeString);
            return Task.CompletedTask;
        }

    }
}
