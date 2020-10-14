using Wrld.Interop;
using System.Runtime.InteropServices;

namespace Wrld.MapCamera
{
    internal struct CameraUpdateInterop
    {
        public LatLongInterop target;
        public double elevation;
        public Space.ElevationMode elevationMode;
        [MarshalAs(UnmanagedType.LPStr)]
        public string indoorMapId;
        public int indoorMapFloorId;
        public double distance;
        public double tilt;
        public double bearing;

        [MarshalAs(UnmanagedType.I1)]
        public bool modifyTarget;
        [MarshalAs(UnmanagedType.I1)]
        public bool modifyElevation;
        [MarshalAs(UnmanagedType.I1)]
        public bool modifyElevationMode;
        [MarshalAs(UnmanagedType.I1)]
        public bool modifyIndoor;
        [MarshalAs(UnmanagedType.I1)]
        public bool modifyDistance;
        [MarshalAs(UnmanagedType.I1)]
        public bool modifyTilt;
        [MarshalAs(UnmanagedType.I1)]
        public bool modifyBearing;

    }

    internal struct CameraAnimationOptionsInterop
    {
        public double durationSeconds;
        public double preferredAnimationSpeed;
        public double minDuration;
        public double maxDuration;
        public double snapDistanceThreshold;
        [MarshalAs(UnmanagedType.I1)]
        public bool snapIfDistanceExceedsThreshold;
        [MarshalAs(UnmanagedType.I1)]
        public bool interruptByGestureAllowed;
        [MarshalAs(UnmanagedType.I1)]
        public bool hasExplicitDuration;
        [MarshalAs(UnmanagedType.I1)]
        public bool hasPreferredAnimationSpeed;
        [MarshalAs(UnmanagedType.I1)]
        public bool hasMinDuration;
        [MarshalAs(UnmanagedType.I1)]
        public bool hasMaxDuration;
        [MarshalAs(UnmanagedType.I1)]
        public bool hasSnapDistanceThreshold;
    }
}
