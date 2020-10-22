using System.Runtime.InteropServices;
using Wrld.Space;

namespace Wrld.Resources.Props
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct PropCreateParamsInterop
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string IndoorMapId;

        [MarshalAs(UnmanagedType.LPStr)]
        public string Name;

        [MarshalAs(UnmanagedType.LPStr)]
        public string GeometryId;

        public double LatitudeDegrees;
        public double LongitudeDegrees;
        public double Elevation;
        public double HeadingDegrees;

        public int IndoorMapFloorId;

        public ElevationMode ElevationMode;
    };
}