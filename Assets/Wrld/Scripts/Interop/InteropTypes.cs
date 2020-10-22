using Wrld.Space;

namespace Wrld.Interop
{ 
    internal struct LatLongInterop
    {
        public double LatitudeDegrees;
        public double LongitudeDegrees;

        public static LatLongInterop Zero()
        {
            return new LatLongInterop
            {
                LatitudeDegrees = 0.0,
                LongitudeDegrees = 0.0
            };
        }

        public static LatLongInterop FromLatLong(LatLong ll)
        {
            return new LatLongInterop
            {
                LatitudeDegrees = ll.GetLatitude(),
                LongitudeDegrees = ll.GetLongitude(),
            };
        }
    }

    internal struct LatLongAltitudeInterop
    {
        public double LatitudeDegrees;
        public double LongitudeDegrees;
        public double Altitude;

        public static LatLongAltitudeInterop FromLatLongAltitude(LatLongAltitude lla)
        {
            return new LatLongAltitudeInterop
            {
                LatitudeDegrees = lla.GetLatitude(),
                LongitudeDegrees = lla.GetLongitude(),
                Altitude = lla.GetAltitude()
            };
        }
    }

    // Not everything needs an interop, as structs are laid out sequentially by default - unity use ARGB rather than RGBA layout though, so an interop class is helpful here:
    //  "C#, Visual Basic, and C++ compilers apply the Sequential layout value to structures by default. For classes, you must apply the LayoutKind.Sequential value explicitly."
    // https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.structlayoutattribute?view=netframework-4.7.1
    internal struct ColorInterop
    {
        public float R;
        public float G;
        public float B;
        public float A;
    }
}
