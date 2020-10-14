using Assets.Wrld.Scripts.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using Wrld.Common.Maths;
using Wrld.Space.Positioners;

namespace Wrld.Space.Positioners
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct PositionerCreateParamsInterop
    {
        public ElevationMode ElevationMode;
        public double LatitudeDegrees;
        public double LongitudeDegrees;
        public double Elevation;
        public int IndoorMapFloorId;

        [MarshalAs(UnmanagedType.LPStr)]
        public string IndoorMapId;

        [MarshalAs(UnmanagedType.I1)]
        public bool UsingFloorId;
    };

    [StructLayout(LayoutKind.Sequential)]
    internal struct DoubleVector3Interop
    {
        public double x;
        public double y;
        public double z;
    };
}
