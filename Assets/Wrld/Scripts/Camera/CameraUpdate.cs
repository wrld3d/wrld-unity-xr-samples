using System;
using System.Runtime.InteropServices;

namespace Wrld.MapCamera
{
    internal class CameraUpdate
    {
        public readonly Space.LatLong target;
        public readonly double targetElevation;
        public readonly Space.ElevationMode targetElevationMode;
        public readonly string targetIndoorMapId;
        public readonly int targetIndoorMapFloorId;
        public readonly double distance;
        public readonly double tilt;
        public readonly double bearing;

        public readonly bool modifyTarget;
        public readonly bool modifyElevation;
        public readonly bool modifyElevationMode;
        public readonly bool modifyIndoor;
        public readonly bool modifyDistance;
        public readonly bool modifyTilt;
        public readonly bool modifyBearing;

        private CameraUpdate(
            Space.LatLong target,
            double targetElevation,
            Space.ElevationMode targetElevationMode,
            string targetIndoorMapId,
            int targetIndoorMapFloorId,
            double distance,
            double tilt,
            double bearing,
            bool modifyTarget,
            bool modifyElevation,
            bool modifyElevationMode,
            bool modifyIndoor,
            bool modifyDistance,
            bool modifyTilt,
            bool modifyBearing
        )
        {
            this.target = target;
            this.targetElevation = targetElevation;
            this.targetElevationMode = targetElevationMode;
            this.targetIndoorMapId = targetIndoorMapId;
            this.targetIndoorMapFloorId = targetIndoorMapFloorId;
            this.distance = distance;
            this.tilt = tilt;
            this.bearing = bearing;
            this.modifyTarget = modifyTarget;
            this.modifyElevation = modifyElevation;
            this.modifyElevationMode = modifyElevationMode;
            this.modifyIndoor = modifyIndoor;
            this.modifyDistance = modifyDistance;
            this.modifyTilt = modifyTilt;
            this.modifyBearing = modifyBearing;
        }


        public class Builder
        {
            private Space.LatLong? m_target;
            private double? m_targetElevation;
            private Space.ElevationMode? m_targetElevationMode;
            private String m_targetIndoorMapId;
            private int? m_targetIndoorMapFloorId;
            private double? m_distance;
            private double? m_tilt;
            private double? m_bearing;

            public Builder()
            {
            }

            public Builder Target(double latitude, double longitude)
            {
                this.m_target = new Space.LatLong(latitude, longitude);
                return this;
            }

            public Builder Target(double latitude, double longitude, double altitude)
            {
                this.m_target = new Space.LatLong(latitude, longitude);
                this.m_targetElevation = altitude;
                this.m_targetElevationMode = Space.ElevationMode.HeightAboveSeaLevel;
                return this;
            }

            public Builder Target(Space.LatLong latLon)
            {
                this.m_target = new Space.LatLong(latLon.GetLatitude(), latLon.GetLongitude());
                return this;
            }

            public Builder Target(Space.LatLongAltitude latLonAlt)
            {
                this.m_target = latLonAlt.GetLatLong();
                this.m_targetElevation = latLonAlt.GetAltitude();
                this.m_targetElevationMode = Space.ElevationMode.HeightAboveSeaLevel;
                return this;
            }

            public Builder Elevation(double elevation)
            {
                this.m_targetElevation = elevation;
                return this;
            }

            public Builder ElevationMode(Space.ElevationMode elevationMode)
            {
                this.m_targetElevationMode = elevationMode;
                return this;
            }

            public Builder Indoor(String indoorMapId, int indoorMapFloorId)
            {
                this.m_targetIndoorMapId = indoorMapId;
                this.m_targetIndoorMapFloorId = indoorMapFloorId;
                return this;
            }

            public Builder Tilt(double? tilt)
            {
                if (tilt.HasValue)
                {
                    return Tilt(tilt.Value);
                }
                m_tilt = null;
                return this;
            }

            public Builder Tilt(double tilt)
            {
                this.m_tilt = Math.Min(Math.Max(tilt, 0.0), 90.0);
                return this;
            }

            public Builder Bearing(double? bearing)
            {
                if (bearing.HasValue)
                {
                    return Bearing(bearing.Value);
                }
                m_bearing = null;
                return this;
            }

            public Builder Bearing(double bearing)
            {                
                while (bearing >= 360.0)
                {
                    bearing -= 360.0;
                }
                while (bearing < 0.0)
                {
                    bearing += 360.0;
                }
                this.m_bearing = bearing;
                return this;
            }

            public Builder Distance(double? distance)
            {
                this.m_distance = distance;
                return this;
            }

            public CameraUpdate Build()
            {
                return new CameraUpdate(
                    m_target ?? new Space.LatLong(),
                    m_targetElevation ?? 0.0,
                    m_targetElevationMode ?? default(Space.ElevationMode),
                    m_targetIndoorMapId,
                    m_targetIndoorMapFloorId ?? 0,
                    m_distance ?? 0.0,
                    m_tilt ?? 0.0,
                    m_bearing ?? 0.0,
                    m_target.HasValue,
                    m_targetElevation.HasValue,
                    m_targetElevationMode.HasValue,
                    m_targetIndoorMapFloorId.HasValue,
                    m_distance.HasValue,
                    m_tilt.HasValue,
                    m_bearing.HasValue
                );
            }

        }
    }
}
