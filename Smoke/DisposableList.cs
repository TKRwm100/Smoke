using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BveEx.Toukaitetudou.Smoke
{
    internal class DisposableList<_Ty> : List<_Ty>, IDisposable where _Ty : IDisposable
    {
        public DisposableList(List<_Ty> src) : base(src) { }
        public void Dispose()
        {
            foreach (_Ty _t in this)
            {
                _t.Dispose();
            }
        }
    }
}
