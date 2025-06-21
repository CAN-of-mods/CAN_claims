using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace claims.src.part.structure.conflict
{
    public class SelectedWarRange
    {
        public DayOfWeek StartDay { get; set; }
        public DayOfWeek EndDay { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan EndTime => StartTime + Duration;
        public SelectedWarRange(DayOfWeek startDay, DayOfWeek endDay, TimeSpan startTime, TimeSpan duration)
        {
            StartDay = startDay;
            EndDay = endDay;
            StartTime = startTime;
            Duration = duration;
        }
    }
}
