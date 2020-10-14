using System;
using UnityEngine;
using Wrld;
using Wrld.Common.Maths;


namespace Wrld.Space
{
    /// <summary>
    /// A GeographicTransform behaviour is used to place a GameObject somewhere on the globe.
    /// It will keep the object correctly positioned and oriented regardless of the coordinate system or camera location used by the map.
    /// This GameObject can then serve as a coordinate frame for its children which can be placed and moved as normal. 
    /// In order for a GeographicTransform's position to be updated, the API must be made aware of it via the GeographicApi.RegisterGeographicTransform method.
    /// This is called automatically OnEnable, but can also be called manually if more control over updating is required.
    /// </summary>
    public class GeographicTransform: MonoBehaviour
    {
        [SerializeField]
        /// <summary>
        /// The initial latitude of the object in degrees.
        /// </summary>
        private double InitialLatitude = 37.771092;

        [SerializeField]
        /// <summary>
        /// The initial longitude of the object in degrees.
        /// </summary>
        private double InitialLongitude = -122.468385;

        [SerializeField]
        /// <summary>
        /// The initial elevation of the object in meters.
        /// </summary>
        private double InitialElevation = 0.0f;

        [SerializeField]
        /// <summary>
        /// The elevation mode of the object; this controls how the Elevation parameter is used.
        /// </summary>
        private ElevationMode ElevationMode = ElevationMode.HeightAboveSeaLevel;

        [SerializeField]
        /// <summary>
        /// The initial heading of the object in degrees, clockwise, relative to north.
        /// </summary>
        private float InitialHeadingInDegrees = 0.0f;

        [SerializeField]
        /// <summary>
        /// The indoor map ID that this object is positioned inside. This is optional.
        /// </summary>
        private string IndoorMapID = "";

        [SerializeField]
        /// <summary>
        /// The indoor map floor that this object is positioned upon. This is optional, and depends on IndoorMapID being set.
        /// </summary>
        private int IndoorMapFloorId = 0;

        [SerializeField]
        /// <summary>
        /// True if the environment flattening transform should be applied to this object.
        /// </summary>
        private bool ApplyFlattening = false;

        private EcefTangentBasis m_tangentBasis;

        private Positioners.Positioner m_positioner;

        bool m_hasEverBeenRegistered = false;
        GameObject m_geolocatedParent;

        void RegisterSelf()
        {
            if (!m_hasEverBeenRegistered && Api.Instance != null)
            {
                var positionerOptions = new Positioners.PositionerOptions()
                                                .LatitudeDegrees(InitialLatitude)
                                                .LongitudeDegrees(InitialLongitude);

                if(ElevationMode == ElevationMode.HeightAboveSeaLevel)
                {
                    positionerOptions = positionerOptions.ElevationAboveSeaLevel(InitialElevation);
                }
                else
                {
                    positionerOptions = positionerOptions.ElevationAboveGround(InitialElevation);
                }

                if(IndoorMapID != "")
                {
                    positionerOptions = positionerOptions.IndoorMapWithFloorId(IndoorMapID, IndoorMapFloorId);
                }
                                            
                m_positioner = Api.Instance.PositionerApi.CreatePositioner(positionerOptions);

                m_positioner.OnTransformedPointChanged += UpdateECEFLocation;

                Api.Instance.GeographicApi.RegisterGeographicTransform(this);
                m_hasEverBeenRegistered = true;
            }
        }

        void UnregisterSelf()
        {
            if (m_hasEverBeenRegistered && Api.Instance != null)
            {
                if (m_positioner != null)
                {
                    m_positioner.OnTransformedPointChanged -= UpdateECEFLocation;
                    m_positioner.Discard();
                    m_positioner = null;
                }

                Api.Instance.GeographicApi.UnregisterGeographicTransform(this);

                m_hasEverBeenRegistered = false;
            }
        }

        void AddGeolocatedParent()
        {
            if (m_geolocatedParent == null)
            {
                m_geolocatedParent = new GameObject("Geolocator");
                m_geolocatedParent.transform.SetParent(transform.parent, false);
                transform.SetParent(m_geolocatedParent.transform, false);
            }
        }

        void RemoveGeolocatedParent()
        {
            if (m_geolocatedParent != null)
            {
                Destroy(m_geolocatedParent);
                m_geolocatedParent = null;
            }
        }

        void OnEnable()
        {
            AddGeolocatedParent();
            RegisterSelf();
        }

        internal void OnDestroy()
        {
            RemoveGeolocatedParent();
            UnregisterSelf();
        }

        void Awake()
        {
            var ecefPoint = LatLong.FromDegrees(InitialLatitude, InitialLongitude).ToECEF();
            var heading = InitialHeadingInDegrees;
            m_tangentBasis = EcefHelpers.EcefTangentBasisFromPointAndHeading(ecefPoint, heading);
            AddGeolocatedParent();
        }

        void Start()
        {
            RegisterSelf();
        }

        void UpdateECEFLocation()
        {
            var ecefLocation = DoubleVector3.zero;
            if (m_positioner.TryGetECEFLocation(out ecefLocation))
            {
                m_tangentBasis.SetPoint(ecefLocation);
            }
        }

        internal void UpdateTransform(ITransformUpdateStrategy updateStrategy)
        {
            var rotation = Quaternion.LookRotation(m_tangentBasis.Forward, m_tangentBasis.Up);
            updateStrategy.UpdateTransform(m_geolocatedParent.transform, m_tangentBasis.PointEcef, Vector3.zero, rotation, 0.0f, ApplyFlattening);
        }

        /// <summary>
        /// Set the location of this transform on the map to the specified latitude and longitude.
        /// </summary>
        /// <param name="latLong">The new position of the transform.</param>
        public void SetPosition(LatLong latLong)
        {
            m_positioner.SetPosition(latLong);
        }

        /// <summary>
        /// Get the current latitude and longitude of this object.
        /// </summary>
        /// <returns>A LatLong representing this object's location.</returns>
        [Obsolete("Please use GeographicTransform.GetPostion() in the future.")]
        public LatLong GetLatLong()
        {
            return GetPosition();
        }

        /// <summary>
        /// Get the current latitude and longitude of this object.
        /// </summary>
        /// <returns>A LatLong representing this object's location.</returns>
        public LatLong GetPosition()
        {
            return m_positioner.GetPosition();
        }

        /// <summary>
        /// Set the heading in degrees of this transform, relative to north.
        /// </summary>
        /// <param name="headingInDegrees">The new heading of the transform.</param>
        public void SetHeading(float headingInDegrees)
        {
            m_tangentBasis = EcefHelpers.EcefTangentBasisFromPointAndHeading(m_tangentBasis.PointEcef, headingInDegrees);
        }

        /// <summary>
        /// Set the desired elevation of this transform, in meters. The behaviour of this depends on the ElevationMode.
        /// </summary>
        /// <param name="elevation">The desired elevation of the transform, in meters.</param>
        public void SetElevation(double elevation)
        {
            m_positioner.SetElevation(elevation);
        }

        /// <summary>
        /// Get the elevation of this transform, in meters.
        /// </summary>
        /// <returns>The elevation of this transform, in meters.</returns>
        public double GetElevation()
        {
            return m_positioner.GetElevation();
        }

        /// <summary>
        /// Set the desired ElevationMode of this transform. See the ElevationMode documentation for details.
        /// </summary>
        /// <param name="elevationMode">The desired ElevationMode of the transform.</param>
        public void SetElevationMode(ElevationMode elevationMode)
        {
            m_positioner.SetElevationMode(elevationMode);
        }

        /// <summary>
        /// Get the ElevationMode of this transform.
        /// </summary>
        /// <returns>The ElevationMode of this transform.</returns>
        public ElevationMode GetElevationMode()
        {
            return m_positioner.GetElevationMode();
        }

        /// <summary>
        /// Sets the Indoor Map of this transform. If this is unset, the transform will be outside instead.
        /// </summary>
        /// <param name="indoorMapId">The Indoor Map id string for the desired Indoor Map. See the IndoorMapApi documentation for more details.</param>
        /// <param name="indoorMapFloorId">The floor of the Indoor Map that this transform should be placed upon.</param>
        public void SetIndoorMap(string indoorMapId, int indoorMapFloorId)
        {
            m_positioner.SetIndoorMap(indoorMapId, indoorMapFloorId);
        }


        /// <summary>
        /// Get the Indoor Map Id string of this transform.
        /// </summary>
        /// <returns>The Indoor Map Id, as a string.</returns>
        public string GetIndoorMapId()
        {
            return m_positioner.GetIndoorMapId();
        }

        /// <summary>
        /// Get the Indoor Map Floor Id of this transform.
        /// </summary>
        /// <returns>The Indoor Map Floor Id of this transform.</returns>
        public int GetIndoorMapFloorId()
        {
            return m_positioner.GetIndoorMapFloorId();
        }

        /// <summary>
        /// Try to get the transformed position as a LatLongAltitude of this GeographicTransform. This can be used with SpacesApi.GeographicToWorldPoint to calculate a Vector3 translation for this GeographicTransform.
        /// </summary>
        /// <param name="out_latLongAlt">The LatLongAltitude that represents the GeographicTransform's position with the desired elevation and ElevationMode applied. The value is only valid if this function returns true.</param>
        /// <returns>Whether or not this function was successful.</returns>
        public bool TryGetLatLongAltitude(out LatLongAltitude out_latLongAlt)
        {
            return m_positioner.TryGetLatLongAltitude(out out_latLongAlt);
        }

        /// <summary>
        /// Try to get the on-screen position of this GeographicTransform. 
        /// </summary>
        /// <param name="out_screenPoint">The screen point of this GeographicTransform. The value is only valid if the returned result is true.</param>
        /// <returns>True if the GeographicTransform's screen point could be determined, false otherwise.</returns>
        public bool TryGetScreenPoint(out Vector3 out_screenPoint)
        {
            return m_positioner.TryGetScreenPoint(out out_screenPoint);
        }

        /// <summary>
        /// Returns true if the screen projection of this GeographicTransform would appear beyond the horizon for the current viewpoint. For example, when viewing the map zoomed out so that the entire globe is visible, calling this method on a GeographicTransform that is located on the opposite side of the Earth from the camera would return true. 
        /// </summary>
        /// <returns>Whether or not this GeographicTransform is beyond the horizon.</returns>
        public bool IsBehindGlobeHorizon()
        {
            return m_positioner.IsBehindGlobeHorizon();
        }

    }
}

