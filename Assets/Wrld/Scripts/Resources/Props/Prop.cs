using System;
using Wrld.Space;

namespace Wrld.Resources.Props
{

    /// <summary>
    /// A Prop is a 3d mesh, which can be displayed in an indoor map with a given position and orientation.  It may be picked, colored, have its 
    /// geometry changed, or be moved around the map.
    /// </summary>
    public class Prop
    {
        /// <summary>
        /// Uniquely identifies this object instance.
        /// </summary>
        public int Id { get; private set; }

        private static int InvalidId = 0;

        private PropsApiInternal m_propsApiInternal;

        private LatLong m_position;
        private double m_elevation;
        private ElevationMode m_elevationMode;
        private string m_indoorMapId;
        private int m_indoorMapFloorId;
        private string m_geometryId;
        private string m_name;
        private double m_headingDegrees;

        // Use Api.Instance.PropApi.CreateProp for public construction
        internal Prop(
            PropsApiInternal propsApiInternal,
            int id,
            PropOptions options)
        {
            if (propsApiInternal == null)
            {
                throw new ArgumentNullException("propsApiInternal");
            }

            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            if (id == InvalidId)
            {
                throw new ArgumentException("invalid id");
            }

            m_propsApiInternal = propsApiInternal;
            Id = id;
            m_elevationMode = options.GetElevationMode();
            m_position.SetLatitude(options.GetLatitudeDegrees());
            m_position.SetLongitude(options.GetLongitudeDegrees());
            m_elevation = options.GetElevation();
            m_indoorMapId = options.GetIndoorMapId();
            m_indoorMapFloorId = options.GetIndoorMapFloorId();
            m_name = options.GetName();
            m_geometryId = options.GetGeometryId();
            m_headingDegrees = options.GetHeadingDegrees();
        }

        /// <summary>
        /// Set the explicit latitude and longitude coordinates of this Prop.
        /// </summary>
        /// <param name="position">The position as a LatLong.</param>
        public void SetPosition(LatLong position)
        {
            m_position = position;
            m_propsApiInternal.SetLocation(this, m_position.GetLatitude(), m_position.GetLongitude());
        }

        /// <summary>
        /// Get the explicitly-set latitude and longitude coordinates of this Prop.
        /// </summary>
        /// <returns>The position as a LatLong.</returns>
        public LatLong GetPosition()
        {
            return m_position;
        }

        /// <summary>
        /// Set the elevation of this Prop, in meters. The behavior of this depends on the ElevationMode.
        /// </summary>
        /// <param name="elevation">The elevation, in meters.</param>
        public void SetElevation(double elevation)
        {
            m_propsApiInternal.SetElevation(this, elevation);
            m_elevation = elevation;
        }

        /// <summary>
        /// Get the elevation of this Prop, in meters.
        /// </summary>
        /// <returns>The elevation of this Prop, in meters.</returns>
        public double GetElevation()
        {
            return m_elevation;
        }

        /// <summary>
        /// Set the ElevationMode of this Prop. See the ElevationMode documentation for more details.
        /// </summary>
        /// <param name="elevationMode">The ElevationMode of this prop.</param>
        public void SetElevationMode(ElevationMode elevationMode)
        {
            m_propsApiInternal.SetElevationMode(this, elevationMode);
            m_elevationMode = elevationMode;
        }

        /// <summary>
        /// Get the ElevationMode of this Prop.
        /// </summary>
        /// <returns>The ElevationMode of this Prop.</returns>
        public ElevationMode GetElevationMode()
        {
            return m_elevationMode;
        }

        /// <summary>
        /// Sets the heading of the prop, in degrees, clockwise from North (0 degrees).
        /// </summary>
        /// <param name="headingDegrees">The heading of this prop in degrees.</param>
        public void SetHeadingDegrees(double headingDegrees)
        {
            m_propsApiInternal.SetHeadingDegrees(this, headingDegrees);
            m_headingDegrees = headingDegrees;
        }

        /// <summary>
        /// Gets the heading of the prop, in degrees, clockwise from North (0 degrees).
        /// </summary>
        /// <returns>The heading of this prop in degrees.</returns>
        public double GetHeadingDegrees()
        {
            return m_headingDegrees;
        }

        /// <summary>
        /// Get the Indoor Map Id string of this Prop.
        /// </summary>
        /// <returns>The Indoor Map Id, as a string.</returns>
        public string GetIndoorMapId()
        {
            return m_indoorMapId;
        }

        /// <summary>
        /// Get the Indoor Map Floor Id of this Prop.
        /// </summary>
        /// <returns>The Indoor Map Floor Id of this Prop.</returns>
        public int GetIndoorMapFloorId()
        {
            return m_indoorMapFloorId;
        }

        /// <summary>
        /// Get the geometry Id for this prop - this identifies the 3d model that we want to 
        /// render in the location described by the prop.  Available geometry is currently curated by 
        /// WRLD - please contact support@wrld3d.com for information about adding new models.
        /// </summary>
        /// <returns>The id of the geometry to be rendered in the prop's location</returns>
        public string GetGeometryId()
        {
            return m_geometryId;
        }

        /// <summary>
        /// Set the geometry id for this prop - this identifies the 3d model that we want to 
        /// render in the location described by the prop.  Available geometry is currently curated by 
        /// WRLD - please contact support@wrld3d.com for information about adding new models.
        /// </summary>
        /// <param name="geometryId">The id of the geometry to be rendered in the prop's location</param>
        public void SetGeometryId(string geometryId)
        {
            m_propsApiInternal.SetGeometryId(this, geometryId);
            m_geometryId = geometryId;
        }

        /// <summary>
        /// Get the name which was assigned to this prop on creation - this should be unique.
        /// </summary>
        /// <returns>The prop's name, as a string.</returns>
        public string GetName()
        {
            return m_name;
        }

        /// <summary>
        /// Destroys the Prop.
        /// </summary>
        public void Discard()
        {
            m_propsApiInternal.DestroyProp(this);
            InvalidateId();
        }

        private void InvalidateId()
        {
            Id = InvalidId;
        }
    }
}