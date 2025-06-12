using BveTypes.ClassWrappers;
using SlimDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BveEx.Toukaitetudou.Smoke
{
    internal class Sprite
    {
        public BveTypes.ClassWrappers.Structure Structure { get; }

        public TimeSpan TotalElapsed { get; private set; }
        public TimeSpan MaxElapsed { get; }
        private Func<TimeSpan, Vector3> Func { get; }

        public Vector3 GetTranslation()
        {
            return Func?.Invoke(TotalElapsed)??Vector3.Zero;
        }

        public bool IsDestruct => TotalElapsed>MaxElapsed;

        public Sprite(BveTypes.ClassWrappers.Structure structure ,TimeSpan maxElapsed,Func<TimeSpan,Vector3> func) 
        {
            Structure = structure;
            TotalElapsed = new TimeSpan(0);
            MaxElapsed = maxElapsed;
            Func = func;
        }
        public void Tick(TimeSpan elapsed)
        {
            TotalElapsed += elapsed;
        }
    }
}
