using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Wrld.Common.Maths;

namespace Assets.Wrld.Scripts.Maths
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DoubleRay
    {
        public DoubleVector3 origin;
        public DoubleVector3 direction;
    }
}
