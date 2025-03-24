using Meadow.Foundation.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace F7Watch
{
    public class TimeApiResponse
    {
        public string timeZone { get; set; }
        public DateTime currentLocalTime { get; set; }
        public CurrentUtcOffset currentUtcOffset { get; set; } = new CurrentUtcOffset();
        public StandardUtcOffset standardUtcOffset { get; set; } = new StandardUtcOffset();
        public bool hasDayLightSaving { get; set; }
        public bool isDayLightSavingActive { get; set; }
        public DstInterval dstInterval { get; set; } = new DstInterval();
    }

    public class CurrentUtcOffset
    {
        public int seconds { get; set; }
        public int milliseconds { get; set; }
        public long ticks { get; set; }
        public long nanoseconds { get; set; }
    }

    public class DstDuration
    {
        public int days { get; set; }
        public int nanosecondOfDay { get; set; }
        public int hours { get; set; }
        public int minutes { get; set; }
        public int seconds { get; set; }
        public int milliseconds { get; set; }
        public int subsecondTicks { get; set; }
        public int subsecondNanoseconds { get; set; }
        public long bclCompatibleTicks { get; set; }
        public int totalDays { get; set; }
        public int totalHours { get; set; }
        public int totalMinutes { get; set; }
        public int totalSeconds { get; set; }
        public long totalMilliseconds { get; set; }
        public long totalTicks { get; set; }
        public long totalNanoseconds { get; set; }
    }

    public class DstInterval
    {
        public string dstName { get; set; }
        public DstOffsetToUtc dstOffsetToUtc { get; set; } = new DstOffsetToUtc();
        public DstOffsetToStandardTime dstOffsetToStandardTime { get; set; } = new DstOffsetToStandardTime();
        public DateTime dstStart { get; set; }
        public DateTime dstEnd { get; set; }
        public DstDuration dstDuration { get; set; } = new DstDuration();
    }

    public class DstOffsetToStandardTime
    {
        public int seconds { get; set; }
        public int milliseconds { get; set; }
        public long ticks { get; set; }
        public long nanoseconds { get; set; }
    }

    public class DstOffsetToUtc
    {
        public int seconds { get; set; }
        public int milliseconds { get; set; }
        public long ticks { get; set; }
        public long nanoseconds { get; set; }
    }

    public class StandardUtcOffset
    {
        public int seconds { get; set; }
        public int milliseconds { get; set; }
        public long ticks { get; set; }
        public long nanoseconds { get; set; }
    }

}
