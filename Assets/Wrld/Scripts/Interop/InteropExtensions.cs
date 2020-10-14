using UnityEngine;

namespace Wrld.Interop
{
    internal static class InteropExtensions
    {
        public static Space.LatLong ToLatLong(this LatLongInterop interop)
        {
            return new Space.LatLong(interop.LatitudeDegrees, interop.LongitudeDegrees);
        }

        public static LatLongInterop ToLatLongInterop(this Space.LatLong ll)
        {
            return new LatLongInterop
            {
                LatitudeDegrees = ll.GetLatitude(),
                LongitudeDegrees = ll.GetLongitude(),
            };
        }

        public static Space.LatLongAltitude ToLatLongAltitude(this LatLongAltitudeInterop interop)
        {
            return new Space.LatLongAltitude(interop.LatitudeDegrees, interop.LongitudeDegrees, interop.Altitude);
        }

        public static LatLongAltitudeInterop ToLatLongAltitudeInterop(this Space.LatLongAltitude lla)
        {
            return new LatLongAltitudeInterop
            {
                LatitudeDegrees = lla.GetLatitude(),
                LongitudeDegrees = lla.GetLongitude(),
                Altitude = lla.GetAltitude()
            };
        }

        public static ColorInterop ToColorInterop(this UnityEngine.Color color)
        {
            return new ColorInterop
            {
                R = color.r,
                G = color.g,
                B = color.b,
                A = color.a
            };
        }

        public static Color ToColor(this ColorInterop color)
        {
            return new Color(color.R, color.G, color.B, color.A);
        }
    }
}
