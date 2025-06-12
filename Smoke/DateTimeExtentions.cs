using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BveEx.Toukaitetudou.Smoke
{
    internal static class DateTimeExtentions
    {
        public static TimeSpan GetTimeSpan(this DateTime src)
        {
            return new TimeSpan(src.Day-1,src.Hour, src.Minute, src.Second, src.Millisecond);
        }
    }
}
