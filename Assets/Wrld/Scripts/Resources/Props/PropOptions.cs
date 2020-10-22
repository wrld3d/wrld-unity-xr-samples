using Wrld.Space;

namespace Wrld.Resources.Props
{
    public class PropOptions
    {
        private ElevationMode m_elevationMode;
        private double m_latitudeDegrees;
        private double m_longitudeDegrees;
        private double m_elevation;
        private string m_indoorMapId;
        private int m_indoorMapFloorId;
        private string m_name;
        private string m_geometryId;
        private double m_headingDegrees;

        /// <summary>
        /// Sets the latitude for the Prop.
        /// </summary>
        /// <param name="latitudeDegrees">The latitude, in degrees.</param>
        /// <returns>This PropOptions instance, with the new latitude set.</returns>
        public PropOptions LatitudeDegrees(double latitudeDegrees)
        {
            m_latitudeDegrees = latitudeDegrees;
            return this;
        }

        /// <summary>
        /// Sets the longitude for the Prop.
        /// </summary>
        /// <param name="longitudeDegrees">The longitude, in degrees.</param>
        /// <returns>This PropOptions instance, with the new longitude set.</returns>
        public PropOptions LongitudeDegrees(double longitudeDegrees)
        {
            m_longitudeDegrees = longitudeDegrees;
            return this;
        }

        /// <summary>
        /// Sets the elevation for the Prop, relative to the altitude of the terrain at the Prop's LatLong coordinate.
        /// </summary>
        /// <param name="elevation">The elevation, in meters.</param>
        /// <returns>This PropOptions instance, with the elevation set.</returns>
        public PropOptions ElevationAboveGround(double elevation)
        {
            m_elevation = elevation;
            m_elevationMode = ElevationMode.HeightAboveGround;
            return this;
        }

        /// <summary>
        /// Sets the elevation for the Prop, relative to sea-level.
        /// </summary>
        /// <param name="elevation">The elevation, in meters.</param>
        /// <returns>This PropOptions instance, with the elevation set.</returns>
        public PropOptions ElevationAboveSeaLevel(double elevation)
        {
            m_elevation = elevation;
            m_elevationMode = ElevationMode.HeightAboveSeaLevel;
            return this;
        }

        /// <summary>
        /// Sets the indoor map properties for the prop. If this method is not called, or if indoorMapId is an empty string,
        /// PropOptions is initialized to create a prop for display on an outdoor map.
        /// </summary>
        /// <param name="indoorMapId">The identifier of the indoor map on which the Prop should be displayed.</param>
        /// <param name="indoorMapFloorId">The identifier of the indoor map floor on which the Prop should be displayed.
        /// In the WRLD Indoor Map Format, this corresponds to the 'z_order' field of the Level object.</param>
        /// <returns>This PropOptions instance, with the new indoor map properties set.</returns>
        public PropOptions IndoorMapWithFloorId(string indoorMapId, int indoorMapFloorId)
        {
            m_indoorMapId = indoorMapId;
            m_indoorMapFloorId = indoorMapFloorId;
            return this;
        }

        /// <summary>
        /// Sets the name for this prop - this should be a unique string id.
        /// </summary>
        /// <param name="_name">The new prop name</param>
        /// <returns>This PropOptions instance, with the new prop name set.</returns>
        public PropOptions Name(string _name)
        {
            m_name = _name;
            return this;
        }

        /// <summary>
        /// Set the geometry Id for this prop - this string specifies which 3d model should be rendered in place
        /// of this prop.  The available models are currently curates by WRLD, please contact support@wrld3d.com if 
        /// you have any queries about extending the range of available props.
        /// </summary>
        /// <param name="_geometryId">the new geometry Id</param>
        /// <returns>This PropOptions instance, with the new geometry Id set.</returns>
        public PropOptions GeometryId(string _geometryId) { m_geometryId = _geometryId; return this; }

        /// <summary>
        /// Sets the heading of the prop, in degrees, clockwise from North (0 degrees).
        /// </summary>
        /// <param name="_headingDegrees">The heading of this prop in degrees.</param>
        /// <returns>This PropOptions instance, with the new heading set.</returns>
        public PropOptions HeadingDegrees(double _headingDegrees) { m_headingDegrees = _headingDegrees; return this; }

        /// <summary>
        /// Returns the mode specifying how the Elevation property is interpreted.
        /// </summary>
        /// <returns>An enumerated value indicating whether Elevation is specified as a height above terrain, or an absolute altitude above sea level.</returns>
        public ElevationMode GetElevationMode()
        {
            return m_elevationMode;
        }

        /// <summary>
        /// Gets the latitude at which a prop created with these options would appear.
        /// </summary>
        /// <returns>The latitude of the prop, in degrees</returns>
        public double GetLatitudeDegrees()
        {
            return m_latitudeDegrees;
        }

        /// <summary>
        /// Gets the longitude at which a prop created with these options would appear.
        /// </summary>
        /// <returns>The longitude of the prop, in degrees</returns>
        public double GetLongitudeDegrees()
        {
            return m_longitudeDegrees;
        }

        /// <summary>
        ///  Returns the current elevation of the prop to be created. The property is interpreted differently, depending on the ElevationMode property.
        /// </summary>
        /// <returns>A height, in meters</returns>
        public double GetElevation()
        {
            return m_elevation;
        }

        /// <summary>
        /// Gets the identifier of an indoor map on which this prop should be displayed, if any.
        /// </summary>
        /// <returns>For a prop on an indoor map, the string identifier of the indoor map; otherwise an empty string.</returns>
        public string GetIndoorMapId()
        {
            return m_indoorMapId;
        }

        /// <summary>
        /// Gets the identifier of an indoor map floor on which this prop should be displayed, if any.
        /// </summary>
        /// <returns>The indoor map floor id.</returns>
        public int GetIndoorMapFloorId()
        {
            return m_indoorMapFloorId;
        }

        /// <summary>
        /// Gets the name to be assigned to the prop created with these parameters, this should be unique.
        /// </summary>
        /// <returns>The name to be assigned to the prop.</returns>
        public string GetName()
        {
            return m_name;
        }

        /// <summary>
        /// Returns the geometry identifier for this PropOptions object.
        /// </summary>
        /// <returns>A string containing id of the geometry that will be displayed for this prop.</returns>
        public string GetGeometryId()
        {
            return m_geometryId;
        }

        /// <summary>
        /// Returns The heading indicating the direction in which the prop will face, in degrees, clockwise from North (0 degrees).
        /// </summary>
        /// <returns>The heading in degrees.</returns>
        public double GetHeadingDegrees()
        {
            return m_headingDegrees;
        }
    }
}