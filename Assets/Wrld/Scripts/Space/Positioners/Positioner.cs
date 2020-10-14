using System;
using UnityEngine;
using Wrld.Common.Maths;

namespace Wrld.Space.Positioners
{
    /// <summary>
    /// A Positioner represents a point at a geographic location on an indoor or outdoor map, and provides a convenient
    /// means of positioning an object when its absolute altitude may be unknown.
    /// A Positioner's latitude and longitude are explicitly specified.
    /// However, its vertical position is defined implicitly, by specifying an elevation relative to one of:
    /// &lt;br/&gt;
    ///   - Mean Sea Level.
    /// &lt;br/&gt;
    ///   - The ground at the specified latitude and longitude.
    /// &lt;br/&gt;
    ///   - An indoor map floor.
    /// &lt;br/&gt;
    /// &lt;br/&gt;
    /// A resultant position is calculated for the specified relative elevation.
    /// This calculation may depend on streamed map resources being loaded - for example, terrain or indoor maps.
    /// As dependent map resources become available, the resultant position is re-calculated with an updated vertical component.
    /// &lt;br/&gt;
    /// This resultant position is also transformed by any current map animation - for example, when viewing an indoor map in 'expanded' view, or
    /// when viewing an outdoor map in 'map collapse' view.
    /// Change to this resultant transformed point is notified via OnTransformedPointChanged.
    /// &lt;br/&gt;
    /// In addition, a screen-space projection of the resultant position is provided, which is convenient for positioning a screen-space UI element
    /// so that it appears anchored relative to a geographic location.
    /// </summary>
    public class Positioner
    {
        /// <summary>
        /// Uniquely identifies this object instance.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Notification that the resultant transformed point of this Positioner instance has changed.
        /// An app may hook to this event in order to respond to a change to a Positioner by accessing the
        /// updated resultant transformed point via Positioner.TryGetECEFLocation or Positioner.TryGetLatLongAltitude.
        ///
        /// See also PositionerApi.OnPositionerTransformedPointChanged.
        /// </summary>
        public Action OnTransformedPointChanged;

        /// <summary>
        /// Notification that the screen projection of the resultant transformed point of this Positioner instance has changed.
        /// An app may hook to this event in order to respond to a change to a Positioner by accessing the
        /// updated projected screen-space point via Positioner.TryGetScreenPoint.
        ///
        /// See also PositionerApi.OnPositionerScreenPointChanged
        /// </summary>
        public Action OnScreenPointChanged;

        /// <summary>
        /// Deprecated - synonymous with OnScreenPointChanged. Alternatively, consider using OnTransformedPointChanged
        /// if responding to changes to the resultant world-space position of this Positioner.
        /// </summary>
        [Obsolete("Deprecated, please use OnScreenPointChanged or OnTransformedPointChanged as appropriate instead", false)]
        public Action OnPositionerPositionChangedDelegate;


        private static int InvalidId = 0;

        private PositionerApiInternal m_positionerApiInternal;

        private LatLong m_position;
        private double m_elevation;
        private ElevationMode m_elevationMode;
        private string m_indoorMapId;
        private int m_indoorMapFloorId;


        // Use Api.Instance.PositionerApi.CreatePositioner for public construction
        internal Positioner(
            PositionerApiInternal positionerApiInternal,
            int id,
            ElevationMode elevationMode)
            {
                if (positionerApiInternal == null)
                {
                    throw new ArgumentNullException("positionerApiInternal");
                }

                if (id == InvalidId)
                {
                    throw new ArgumentException("invalid id");
                }

                m_positionerApiInternal = positionerApiInternal;
                Id = id;
                m_elevationMode = elevationMode;
            }

        /// <summary>
        /// Try to get the resultant transformed position of this Positioner as an ECEF coordinate.
        /// The method returns false if the resultant transformed position is not currently defined - in which
        /// case IsTransformedPointDefined would also return false.
        /// </summary>
        /// <param name="out_positionerECEFLocation">If the return value is true, the resultant transformed position of
        /// this Positioner, represented as an ECEF coordinate; else a zero value.</param>
        /// <returns>True if the Positioner's ECEF location could be determined, false otherwise.</returns>
        public bool TryGetECEFLocation(out DoubleVector3 out_positionerECEFLocation)
        {
            if(m_positionerApiInternal.TryFetchECEFLocationForPositioner(this, out out_positionerECEFLocation))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to get the resultant transformed position of this Positioner as a LatLongAltitude.
        /// The method returns false if the resultant transformed position is not currently defined - in which
        /// case IsTransformedPointDefined would also return false.
        /// To transform out_latLongAlt into a world-space translation, see SpacesApi.GeographicToWorldPoint.
        /// </summary>
        /// <param name="out_latLongAlt">If the return value is true, the resultant transformed position of this Positioner,
        /// represented as a LatLongAltitude; else a zero value</param>
        /// <returns>True if out_latLongAlt was successfully set; else false.</returns>
        public bool TryGetLatLongAltitude(out LatLongAltitude out_latLongAlt)
        {
            if (m_positionerApiInternal.TryFetchLatLongAltitudeForPositioner(this, out out_latLongAlt))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to get the projected screen-space position of the resultant transformed position.
        /// Try to get the on-screen position of this Positioner.
        /// Note that the screen point value obtained via this method may change every frame.
        /// </summary>
        /// <param name="out_screenPoint">The screen point of this Positioner. The value is only valid if the returned result is true.</param>
        /// <returns>True if the Positioner's screen point could be determined, false otherwise.</returns>
        public bool TryGetScreenPoint(out Vector3 out_screenPoint)
        {
            if (m_positionerApiInternal.TryFetchScreenPointForPositioner(this, out out_screenPoint))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Set the explicit latitude and longitude coordinates of this Positioner.
        /// </summary>
        /// <param name="latitudeDegrees">The latitude, in degrees.</param>
        /// <param name="longitudeDegrees">The longitude, in degrees.</param>
        [Obsolete("Please use Positioner.SetPosition(LatLong position) in the future.")]
        public void SetLocation(double latitudeDegrees, double longitudeDegrees)
        {
            m_position = LatLong.FromDegrees(latitudeDegrees, longitudeDegrees);
            SetPosition(m_position);
        }

        /// <summary>
        /// Set the explicit latitude and longitude coordinates of this Positioner.
        /// </summary>
        /// <param name="position">The position as a LatLong.</param>
        public void SetPosition(LatLong position)
        {
            m_position = position;
            m_positionerApiInternal.SetPositionerLocation(this, m_position.GetLatitude(), m_position.GetLongitude());
        }

        /// <summary>
        /// Get the explicitly-set latitude and longitude coordinates of this Positioner.
        /// </summary>
        /// <returns>The position as a LatLong.</returns>
        public LatLong GetPosition()
        {
            return m_position;
        }

        /// <summary>
        /// Set the elevation of this Positioner, in meters. The behaviour of this depends on the ElevationMode.
        /// </summary>
        /// <param name="elevation">The elevation, in meters.</param>
        public void SetElevation(double elevation)
        {
            m_positionerApiInternal.SetPositionerElevation(this, elevation);
            m_elevation = elevation;
        }

        /// <summary>
        /// Get the elevation of this Positioner, in meters.
        /// </summary>
        /// <returns>The elevation of this Positioner, in meters.</returns>
        public double GetElevation()
        {
            return m_elevation;
        }

        /// <summary>
        /// Set the ElevationMode of this Positioner. See the ElevationMode documentation for more details.
        /// </summary>
        /// <param name="elevationMode">The ElevationMode of this positioner.</param>
        public void SetElevationMode(ElevationMode elevationMode)
        {
            m_positionerApiInternal.SetPositionerElevationMode(this, elevationMode);
            m_elevationMode = elevationMode;
        }

        /// <summary>
        /// Get the ElevationMode of this Positioner.
        /// </summary>
        /// <returns>The ElevationMode of this Positioner.</returns>
        public ElevationMode GetElevationMode()
        {
            return m_elevationMode;
        }

        /// <summary>
        /// Sets the Indoor Map of this Positioner. If this is unset, the Positioner will be outside instead.
        /// </summary>
        /// <param name="indoorMapId">The identifier of the indoor map on which the positioner should be displayed.
        /// See the IndoorMapApi documentation for more details.</param>
        /// <param name="indoorMapFloorId">The identifier of the indoor map floor on which the Positioner should be displayed.</param>
        public void SetIndoorMap(string indoorMapId, int indoorMapFloorId)
        {
            m_positionerApiInternal.SetPositionerIndoorMap(this, indoorMapId, indoorMapFloorId);
            m_indoorMapId = indoorMapId;
            m_indoorMapFloorId = indoorMapFloorId;
        }

        /// <summary>
        /// Get the Indoor Map Id string of this Positioner.
        /// </summary>
        /// <returns>The Indoor Map Id, as a string.</returns>
        public string GetIndoorMapId()
        {
            return m_indoorMapId;
        }

        /// <summary>
        /// Get the Indoor Map Floor Id of this Positioner.
        /// </summary>
        /// <returns>The Indoor Map Floor Id of this Positioner.</returns>
        public int GetIndoorMapFloorId()
        {
            return m_indoorMapFloorId;
        }

        /// <summary>
        /// Query whether the resultant transformed point of this positioner is currently defined.
        /// May return false if, for example, this Positioner is on an indoor map, and the indoor map is not currently being displayed.
        /// If true, then getting the transformed point via TryGetECEFLocation or TryGetLatLongAltitude will succeed; else they will fail.
        /// </summary>
        /// <returns>True if the resultant transformed point of this positioner is currently defined; else false.</returns>
        public bool IsTransformedPointDefined()
        {
            return m_positionerApiInternal.IsTransformedPointDefined(this);
        }

        /// <summary>
        /// Returns true if the screen projection of this Positioner would appear beyond the horizon for the current viewpoint.
        /// For example, when viewing the map zoomed out so that the entire globe is visible, calling this method on a
        /// Positioner that is located on the opposite side of the Earth from the camera would return true.
        /// </summary>
        /// <returns>Whether or not this Positioner is beyond the horizon.</returns>
        public bool IsBehindGlobeHorizon()
        {
            return m_positionerApiInternal.IsPositionerBehindGlobeHorizon(this);
        }

        /// <summary>
        /// Destroys the Positioner.
        /// </summary>
        public void Discard()
        {
            m_positionerApiInternal.DestroyPositioner(this);
            InvalidateId();
        }

        private void InvalidateId()
        {
            Id = InvalidId;
        }
    }
}
