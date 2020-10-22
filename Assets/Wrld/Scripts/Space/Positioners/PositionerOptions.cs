using System;

namespace Wrld.Space.Positioners
{
    /// <summary>
    /// Defines creation parameters for a Positioner.
    /// </summary>
    public class PositionerOptions
    {
        private double m_latitudeDegrees;
        private double m_longitudeDegrees;
        private double m_elevation;
        private ElevationMode m_elevationMode = ElevationMode.HeightAboveGround;
        private string m_indoorMapId = "";
        private int m_indoorMapFloorId;
        private bool m_usingFloorId = true;

        public PositionerOptions()
        {
        }

        /// <summary>
        /// Sets the latitude for the Positioner.
        /// </summary>
        /// <param name="latitudeDegrees">The latitude, in degrees.</param>
        /// <returns>This PositionerOptions instance, with the new latitude set.</returns>
        public PositionerOptions LatitudeDegrees(double latitudeDegrees)
        {
            m_latitudeDegrees = latitudeDegrees;
            return this;
        }

        /// <summary>
        /// Sets the longitude for the Positioner.
        /// </summary>
        /// <param name="longitudeDegrees">The longitude, in degrees.</param>
        /// <returns>This PositionerOptions instance, with the new longitude set.</returns>
        public PositionerOptions LongitudeDegrees(double longitudeDegrees)
        {
            m_longitudeDegrees = longitudeDegrees;
            return this;
        }

        /// <summary>
        /// Sets the elevation for the Positioner, relative to the altitude of the terrain at the Positioner's LatLong coordinate.
        /// </summary>
        /// <param name="elevation">The elevation, in meters.</param>
        /// <returns>This PositionerOptions instance, with the elevation set.</returns>
        public PositionerOptions ElevationAboveGround(double elevation)
        {
            m_elevation = elevation;
            m_elevationMode = ElevationMode.HeightAboveGround;
            return this;
        }

        /// <summary>
        /// Sets the elevation for the Positioner, relative to sea-level.
        /// </summary>
        /// <param name="elevation">The elevation, in meters.</param>
        /// <returns>This PositionerOptions instance, with the elevation set.</returns>
        public PositionerOptions ElevationAboveSeaLevel(double elevation)
        {
            m_elevation = elevation;
            m_elevationMode = ElevationMode.HeightAboveSeaLevel;
            return this;
        }

        /// <summary>
        /// Sets the indoor map for the Positioner. If this method is not called, or if indoorMapId is an empty string,
        /// PositionerOptions is initialized to create a positioner for display on an outdoor map.
        /// As a side-effect, the resultant Positioner object created with these options will treat
        /// the indoorMapFloorId parameter of Positioner.SetIndoorMap(string indoorMapId, int indoorMapFloorId) as
        /// an index into the zero-based array of floors for the specified indoor map.
        /// This method is retained for legacy compatibility reasons only, please use IndoorMapWithFloorId instead.
        /// </summary>
        /// <param name="indoorMapId">The identifier of the indoor map on which the Positioner should be displayed.</param>
        /// <returns>This PositionerOptions instance, with the Indoor Map Id set.</returns>
        [Obsolete("Deprecated, please use IndoorMapWithFloorId instead", false)]
        public PositionerOptions IndoorMap(string indoorMapId)
        {
            m_indoorMapId = indoorMapId;
            m_indoorMapFloorId = 0;
            m_usingFloorId = false;
            return this;
        }

        /// <summary>
        /// Sets the indoor map properties for the positioner. If this method is not called, or if indoorMapId is an empty string,
        /// PositionerOptions is initialized to create a positioner for display on an outdoor map.
        /// </summary>
        /// <param name="indoorMapId">The identifier of the indoor map on which the Positioner should be displayed.</param>
        /// <param name="indoorMapFloorId">The identifier of the indoor map floor on which the Positioner should be displayed.
        /// In the WRLD Indoor Map Format, this corresponds to the ‘z_order’ field of the Level object.</param>
        /// <returns>This PositionerOptions instance, with the new indoor map properties set.</returns>
        public PositionerOptions IndoorMapWithFloorId(string indoorMapId, int indoorMapFloorId)
        {
            m_indoorMapId = indoorMapId;
            m_indoorMapFloorId = indoorMapFloorId;
            m_usingFloorId = true;
            return this;
        }

        internal ElevationMode GetElevationMode()
        {
            return m_elevationMode;
        }

        internal double GetLatitudeDegrees()
        {
            return m_latitudeDegrees;
        }

        internal double GetLongitudeDegrees()
        {
            return m_longitudeDegrees;
        }

        internal double GetElevation()
        {
            return m_elevation;
        }

        internal string GetIndoorMapId()
        {
            return m_indoorMapId;
        }

        internal int GetIndoorMapFloorId()
        {
            return m_indoorMapFloorId;
        }

        internal bool IsUsingFloorId()
        {
            return m_usingFloorId;
        }
    }
}
