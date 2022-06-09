using UnityEngine;
using Wrld;
using Wrld.Scripts.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class WrldMap : MonoBehaviour
{
    [Header("Camera/View Settings")]
    [Tooltip("Camera used to request streamed resources")]
    [SerializeField]
    protected Camera m_streamingCamera = null;

    [Header("Setup")]
    [SerializeField]
    protected string m_apiKey;

    [Tooltip("In degrees")]
    [Range(-90.0f, 90.0f)]
    [SerializeField]
    protected double m_latitudeDegrees = 37.771092;

    [Tooltip("In degrees")]
    [Range(-180.0f, 180.0f)]
    [SerializeField]
    protected double m_longitudeDegrees = -122.468385;

    [Tooltip("The distance of the camera from the interest point (meters)")]
    [SerializeField]
    [Range(300.0f, 7000000.0f)]
    protected double m_distanceToInterest = 1781.0;

    [Tooltip("Direction you are facing")]
    [SerializeField]
    [Range(0, 360.0f)]
    protected double m_headingDegrees = 0.0;

    [Header("Map Behaviour Settings")]
    [Tooltip("Coordinate System to be used. ECEF requires additional calculation and setup")]
    [SerializeField]
    protected CoordinateSystem m_coordSystem = CoordinateSystem.UnityWorld;

    [Tooltip("Whether to determine streaming LOD based on distance, instead of altitude")]
    [SerializeField]
    protected bool m_streamingLodBasedOnDistance = false;

    [Header("Theme Settings")]
    [Tooltip("Directory within the Resources folder to look for materials during runtime. Default is provided with the package")]
    [SerializeField]
    protected string m_materialDirectory = "WrldMaterials/";

    [Tooltip("Directory within the Resources folder to look for interior materials during runtime. Default is provided with the package")]
    [SerializeField]
    protected string m_indoorMapMaterialDirectory = "WrldMaterials/Archetypes";

    [Tooltip("The material to override all landmarks with. Uses standard diffuse if null.")]
    [SerializeField]
    protected Material m_overrideLandmarkMaterial = null;

    [Tooltip("Set to true to use the default mouse & keyboard/touch controls, false if controlling the camera by some other means.")]
    [SerializeField]
    protected bool m_useBuiltInCameraControls = true;

    [Header("Collision Settings")]
    [Tooltip("Set to true for Terrain collisions")]
    [SerializeField]
    protected bool m_terrainCollisions = false;

    [Tooltip("Set to true for Road collision")]
    [SerializeField]
    protected bool m_roadCollisions = false;

    [Tooltip("Set to true for Building collision")]
    [SerializeField]
    protected bool m_buildingCollisions = false;

    [Header("Manifest Settings")]
    [Tooltip("The override URL pointing to a valid coverage tree binary file. Leave empty to use the default.")]
    [SerializeField]
    protected string m_coverageTreeManifestUrl = "";

    [Tooltip("The override URL pointing to a valid manifest with theme information, also a binary file. Leave empty to use the default.")]
    [SerializeField]
    protected string m_themeManifestUrl = "";

    [Header("Label Settings")]
    [Tooltip("Set to true to display text labels for road names, buildings, and other landmarks.")]
    [SerializeField]
    protected bool m_enableLabels = false;

    [Tooltip("Set to true to allow the user to tap or click on an Interior Entry Marker to enter its associated Indoor Map.")]
    [SerializeField]
    protected bool m_enableIndoorEntryMarkerEvents = true;

    [Tooltip("Provide a Unity UI Canvas object for text labels to be drawn upon. If this is blank, a new canvas will be instantiated instead.")]
    [SerializeField]
    protected GameObject m_labelCanvas = null;

    [Tooltip("Set to false if you require read/write access to the meshes")]
    [SerializeField]
    protected bool m_uploadMeshes = false;

    protected Api m_api;

    protected internal void Awake()
    {
        SetupApi();
    }

    protected internal void OnEnable()
    {
        if (Api.Instance == null)
        {
            SetupApi();
        }

        m_api.SetEnabled(true);
    }

    protected internal void OnDisable()
    {
        m_api.SetEnabled(false);
    }

    protected internal string GetApiKey()
    {
        if (APIKeyHelpers.AppearsValid(m_apiKey))
        {
            APIKeyHelpers.CacheAPIKey(m_apiKey);
        }
        else
        {
            var cachedAPIKey = APIKeyHelpers.GetCachedAPIKey();

            if (cachedAPIKey != null)
            {
                m_apiKey = cachedAPIKey;
            }
        }

        return m_apiKey;
    }

    protected internal void SetupApi()
    {
        var config = ConfigParams.MakeDefaultConfig();
        config.DistanceToInterest = m_distanceToInterest;
        config.LatitudeDegrees = m_latitudeDegrees;
        config.LongitudeDegrees = m_longitudeDegrees;
        config.HeadingDegrees = m_headingDegrees;
        config.StreamingLodBasedOnDistance = m_streamingLodBasedOnDistance;
        config.MaterialsDirectory = m_materialDirectory;
        config.IndoorMapMaterialsDirectory = m_indoorMapMaterialDirectory;
        config.OverrideLandmarkMaterial = m_overrideLandmarkMaterial;
        config.Collisions.TerrainCollision = m_terrainCollisions;
        config.Collisions.RoadCollision = m_roadCollisions;
        config.Collisions.BuildingCollision = m_buildingCollisions;
        config.CoverageTreeManifestUrl = m_coverageTreeManifestUrl;
        config.ThemeManifestUrl = m_themeManifestUrl;
        config.EnableLabels = m_enableLabels;
        config.EnableIndoorEntryMarkerEvents = m_enableIndoorEntryMarkerEvents;
        config.LabelCanvas = m_labelCanvas;
        config.UploadMeshesToGPU = m_uploadMeshes;

        try
        {
            Api.Create(GetApiKey(), m_coordSystem, transform, config);
        }
        catch (InvalidApiKeyException)
        {
#if UNITY_EDITOR
            EditorUtility.DisplayDialog(
                "Error: Invalid WRLD API Key",
                string.Format("Please enter a valid WRLD API Key (see the WrldMap script on GameObject \"{0}\" in the Inspector).",
                    gameObject.name),
                "OK");
#endif
            throw;
        }

        m_api = Api.Instance;

        if (m_useBuiltInCameraControls)
        {
            m_api.CameraApi.SetControlledCamera(m_streamingCamera);
        }

        // Ensure that Camera API input handler uses the initial value given by the Unity Inspector.
        // This can be changed later by a developer, but should start off consistent with other Inspector properties.
        m_api.CameraApi.IsCameraDrivenFromInput = m_useBuiltInCameraControls;
    }

    protected internal void LateUpdate()
    {
        if (m_useBuiltInCameraControls && (m_streamingCamera == m_api.CameraApi.GetControlledCamera()))
        {
            m_api.StreamResourcesForBuiltInCamera(m_streamingCamera);
        }
        else
        {
            m_api.StreamResourcesForCamera(m_streamingCamera);
        }

        m_api.Update();
    }

    protected internal void OnDestroy()
    {
        if (m_api != null)
        {
            m_api.Destroy();
            m_api = null;
        }
    }

    protected internal void OnValidate()
    {
        GetApiKey();
    }

    protected internal void OnApplicationPause(bool isPaused)
    {
        SetApplicationPaused(isPaused);
    }

    protected internal void OnApplicationFocus(bool hasFocus)
    {
        SetApplicationPaused(!hasFocus);
    }

    protected internal void SetApplicationPaused(bool isPaused)
    {
        if (isPaused)
        {
            m_api?.OnApplicationPaused();
        }
        else
        {
            m_api?.OnApplicationResumed();
        }
    }
}
