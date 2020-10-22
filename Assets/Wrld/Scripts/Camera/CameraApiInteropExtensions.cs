using Wrld.Interop;

namespace Wrld.MapCamera
{
    internal static class CameraApiInteropExtensions
    {
        public static CameraUpdateInterop ToCameraUpdateInterop(this CameraUpdate cameraUpdate)
        {
            return new CameraUpdateInterop
            {
                target = cameraUpdate.target.ToLatLongInterop(),
                elevation = cameraUpdate.targetElevation,
                elevationMode = cameraUpdate.targetElevationMode,
                indoorMapId = cameraUpdate.targetIndoorMapId,
                indoorMapFloorId = cameraUpdate.targetIndoorMapFloorId,
                distance = cameraUpdate.distance,
                tilt = cameraUpdate.tilt,
                bearing = cameraUpdate.bearing,
                modifyTarget = cameraUpdate.modifyTarget,
                modifyElevation = cameraUpdate.modifyElevation,
                modifyElevationMode = cameraUpdate.modifyElevationMode,
                modifyIndoor = cameraUpdate.modifyIndoor,
                modifyDistance = cameraUpdate.modifyDistance,
                modifyTilt = cameraUpdate.modifyTilt,
                modifyBearing = cameraUpdate.modifyBearing

            };
        }

        public static CameraAnimationOptionsInterop ToCameraAnimationOptionsInterop(this CameraAnimationOptions cameraAnimationOptions)
        {
            return new CameraAnimationOptionsInterop
            {
                durationSeconds = cameraAnimationOptions.durationSeconds,
                preferredAnimationSpeed = cameraAnimationOptions.preferredAnimationSpeed,
                minDuration = cameraAnimationOptions.minDuration,
                maxDuration = cameraAnimationOptions.maxDuration,
                snapDistanceThreshold = cameraAnimationOptions.snapDistanceThreshold,
                snapIfDistanceExceedsThreshold = cameraAnimationOptions.snapIfDistanceExceedsThreshold,
                interruptByGestureAllowed = cameraAnimationOptions.interruptByGestureAllowed,
                hasExplicitDuration = cameraAnimationOptions.hasExplicitDuration,
                hasPreferredAnimationSpeed = cameraAnimationOptions.hasPreferredAnimationSpeed,
                hasMinDuration = cameraAnimationOptions.hasMinDuration,
                hasMaxDuration = cameraAnimationOptions.hasMaxDuration,
                hasSnapDistanceThreshold = cameraAnimationOptions.hasSnapDistanceThreshold
            };


        }
    }
}
