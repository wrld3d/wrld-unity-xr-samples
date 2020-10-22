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
    private Camera m_streamingCamera = null;

    [Header("Setup")]
    [SerializeField]
    private string m_apiKey;

    [Tooltip("In degrees")]
    [Range(-90.0f, 90.0f)]
    [SerializeField]
    private double m_latitudeDegrees = 37.771092;

    [Tooltip("In degrees")]
    [Range(-180.0f, 180.0f)]
    [SerializeField]
    private double m_longitudeDegrees = -122.468385;

    [Tooltip("The distance of the camera from the interest point (meters)")]
    [SerializeField]
    [Range(300.0f, 7000000.0f)]
    private double m_distanceToInterest = 1781.0;

    [Tooltip("Direction you are facing")]
    [SerializeField]
    [Range(0, 360.0f)]
    private double m_headingDegrees = 0.0;

    [Header("Map Behaviour Settings")]
    [Tooltip("Coordinate System to be used. ECEF requires additional calculation and setup")]
    [SerializeField]
    private CoordinateSystem m_coordSystem = CoordinateSystem.UnityWorld;

    [Tooltip("Whether to determine streaming LOD based on distance, instead of altitude")]
    [SerializeField]
    private bool m_streamingLodBasedOnDistance = false;

    [Header("Theme Settings")]
    [Tooltip("Directory within the Resources folder to look for materials during runtime. Default is provided with the package")]
    [SerializeField]
    private string m_materialDirectory = "WrldMaterials/";

    [Tooltip("Directory within the Resources folder to look for interior materials during runtime. Default is provided with the package")]
    [SerializeField]
    private string m_indoorMapMaterialDirectory = "WrldMaterials/Archetypes";

    [Tooltip("The material to override all landmarks with. Uses standard diffuse if null.")]
    [SerializeField]
    private Material m_overrideLandmarkMaterial = null;

    [Tooltip("Set to true to use the default mouse & keyboard/touch controls, false if controlling the camera by some other means.")]
    [SerializeField]
    private bool m_useBuiltInCameraControls = true;

    [Header("Collision Settings")]
    [Tooltip("Set to true for Terrain collisions")]
    [SerializeField]
    private bool m_terrainCollisions = false;

    [Tooltip("Set to true for Road collision")]
    [SerializeField]
    private bool m_roadCollisions = false;

    [Tooltip("Set to true for Building collision")]
    [SerializeField]
    private bool m_buildingCollisions = false;

    [Header("Label Settings")]
    [Tooltip("Set to true to display text labels for road names, buildings, and other landmarks.")]
    [SerializeField]
    private bool m_enableLabels = false;

    [Tooltip("Set to true to allow the user to tap or click on an Interior Entry Marker to enter its associated Indoor Map.")]
    [SerializeField]
    private bool m_enableIndoorEntryMarkerEvents = true;

    [Tooltip("Provide a Unity UI Canvas object for text labels to be drawn upon. If this is blank, a new canvas will be instantiated instead.")]
    [SerializeField]
    private GameObject m_labelCanvas = null;

    [Tooltip("Set to false if you require read/write access to the meshes")]
    [SerializeField]
    private bool m_uploadMeshes = false;

    private Api m_api;
    private bool m_isExitingApp = false;
    void Awake()
    {
        SetupApi();
    }

    void OnEnable()
    {
        if (Api.Instance == null)
        {
            SetupApi();
        }

        m_api.SetEnabled(true);
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += EditorPlayModeChanged;
#endif
    }

    void OnDisable()
    {
        m_api.SetEnabled(false);
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged -= EditorPlayModeChanged;
#endif
    }

    string GetApiKey()
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

    void SetupApi()
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

    void LateUpdate()
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

    internal void OnDestroy()
    {
        if (m_api != null)
        {
            if (!m_isExitingApp)
            {
                m_api.ResetRootChilds();
            }
            m_api.Destroy();
            m_api = null;
        }
    }

    void OnApplicationPause(bool isPaused)
    {
        SetApplicationPaused(isPaused);
    }

    void OnApplicationFocus(bool hasFocus)
    {
        SetApplicationPaused(!hasFocus);
    }

    void SetApplicationPaused(bool isPaused)
    {
        if (isPaused)
        {
            m_api.OnApplicationPaused();
        }
        else
        {
            m_api.OnApplicationResumed();
        }
    }

#if !UNITY_EDITOR
    void OnApplicationQuit()
    {
        m_isExitingApp = true;
        m_api.ResetRootChilds();
    }
#endif

#if UNITY_EDITOR
    void EditorPlayModeChanged(PlayModeStateChange playModeState)
    {
        if (playModeState == PlayModeStateChange.ExitingPlayMode)
        {
            m_isExitingApp = true;
            m_api.ResetRootChilds();
        }
    }
#endif
}
