using System;
using System.Collections.Generic;
using System.Text;
using CoronaBuster.Services;
using static System.Math;

namespace CoronaBuster {
    public static class Helpers {
        public static readonly DateTime EPOCH = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static TimeSpan GetExactTime() => (DateTime.UtcNow - EPOCH);

        public static TimeSpan GetApproximateMinute() => TimeSpan.FromMinutes(Floor(GetExactTime().TotalMinutes));

        public static TimeSpan GetApproximateHour() => TimeSpan.FromHours(Floor(GetExactTime().TotalHours));

        public static TimeSpan GetApproximateDay() => TimeSpan.FromDays(Floor(GetExactTime().TotalDays));
        public static DateTime ToDateTime(TimeSpan relativeTime) => EPOCH + relativeTime;
    }
}
