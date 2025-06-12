using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BveEx.Toukaitetudou.Smoke
{
    internal static class ExtentionFunction
    {
        public static DisposableList<_Ty> ToDisposableList<_Ty>(this IEnumerable<_Ty> src) where _Ty : IDisposable
        {
            return new DisposableList<_Ty>(src.ToList());
        }
    }
}
