using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Wrld
{
    /// <summary>
    /// These are parameters required during initialization and construction of the map.
    /// They are only used for startup. Please use the [API endpoints]({{ site.baseurl }}/docs/api) to interact with the map.
    /// </summary>
    public struct ConfigParams
    {
        //only used for types that are sent across to the native side
        [StructLayout(LayoutKind.Sequential)]
        public struct NativeConfig
        {
            public double m_latitudeDegrees;
            public double m_longitudeDegrees;
            public double m_distanceToInterest;
            public double m_headingDegrees;
            [MarshalAs(UnmanagedType.I1)]
            public bool m_streamingLodBasedOnDistance;
            public double m_viewportWidth;
            public double m_viewportHeight;

            [MarshalAs(UnmanagedType.I1)]
            public bool m_enableLabels;
            [MarshalAs(UnmanagedType.I1)]
            public bool m_enableIndoorEntryMarkerEvents;
            public int m_resourceWebRequestTimeoutSeconds;
        }

        NativeConfig m_nativeConfig;

        /// <summary>
        /// Get the native config which can be used across language boundaries.
        /// For internal use only.
        /// </summary>
        public NativeConfig GetNativeConfig()
        {
            return m_nativeConfig;
        }

        /// <summary>
        /// Starting Latitude in degrees.
        /// </summary>
        public double LatitudeDegrees
        {
            get { return m_nativeConfig.m_latitudeDegrees; }
            set { m_nativeConfig.m_latitudeDegrees = value; }
        }

        /// <summary>
        /// Starting Longitude in degrees.
        /// </summary>
        public double LongitudeDegrees
        {
            get { return m_nativeConfig.m_longitudeDegrees; }
            set { m_nativeConfig.m_longitudeDegrees = value; }
        }

        /// <summary>
        /// Starting distance of the camera from the interest point, in meters.
        /// </summary>
        public double DistanceToInterest
        {
            get { return m_nativeConfig.m_distanceToInterest; }
            set { m_nativeConfig.m_distanceToInterest = value; }
        }

        /// <summary>
        /// Starting heading / direction in degrees.
        /// </summary>
        public double HeadingDegrees
        {
            get { return m_nativeConfig.m_headingDegrees; }
            set { m_nativeConfig.m_headingDegrees = value; }
        }

        /// <summary>
        /// Whether to determine streaming LOD based on distance, instead of altitude.
        /// </summary>
        public bool StreamingLodBasedOnDistance
        {
            get { return m_nativeConfig.m_streamingLodBasedOnDistance; }
            set { m_nativeConfig.m_streamingLodBasedOnDistance = value; }
        }

        /// <summary>
        /// Default materials lookup directory with existing semantically named materials required for runtime assignment.
        /// </summary>
        public string MaterialsDirectory;

        /// <summary>
        /// Default interiors materials lookup directory with existing semantically named materials required for runtime assignment.
        /// </summary>
        public string IndoorMapMaterialsDirectory;

        /// <summary>
        /// Default material to be assigned to Wrld Landmarks.
        /// Landmarks are special buildings which are dotted around the globe. They have a custom texture
        /// which will be automatically assigned to this material's diffuse value. Setting this value to null uses
        /// a standard diffuse material.
        /// </summary>
        public Material OverrideLandmarkMaterial;

        /// <summary>
        /// The override URL pointing to a valid coverage tree binary file.
        /// </summary>
        public string CoverageTreeManifestUrl;

        /// <summary>
        /// The override URL pointing to a valid manifest with theme information, also a binary file.
        /// </summary>
        public string ThemeManifestUrl;

        /// <summary>
        /// The Collision structure that holds whether to create collision meshes for different types of map geometry.
        /// </summary>
        public struct CollisionConfig
        {
            /// <summary>
            /// Will cause collision geometry to be generated for terrain meshes if true.
            /// </summary>
            public bool TerrainCollision { get; set; }
            
            /// <summary>
            /// Will cause collision geometry to be generated for road meshes if true.
            /// </summary>
            public bool RoadCollision { get; set; }

            /// <summary>
            /// Will cause collision geometry to be generated for building meshes if true.
            /// </summary>
            public bool BuildingCollision { get; set; }
        }

        /// <summary>
        /// Contains information on whether to enable or disable generation of collision meshes for different types of map geometry.
        /// </summary>
        public CollisionConfig Collisions;

        /// <summary>
        /// This is automatically set using Unity's viewport resolution.
        /// </summary>
        public double ViewportWidth
        {
            get { return m_nativeConfig.m_viewportWidth; }
            set { m_nativeConfig.m_viewportWidth = value; }
        }

        /// <summary>
        /// This is automatically set using Unity's viewport resolution.
        /// </summary>
        public double ViewportHeight
        {
            get { return m_nativeConfig.m_viewportHeight; }
            set { m_nativeConfig.m_viewportHeight = value; }
        }

        /// <summary>
        /// This controls whether or not Labels are enabled for roads, buildings and other landmarks. 
        /// This is set automatically from the Unity Inspector panel for the WrldMap object.
        /// </summary>
        public bool EnableLabels
        {
            get { return m_nativeConfig.m_enableLabels; }
            set { m_nativeConfig.m_enableLabels = value; }
        }

        /// <summary>
        /// This is the Unity UI Canvas that Labels will be drawn upon, if enabled.
        /// This is set automatically from the Unity Inspector panel for the WrldMap object; if left blank, a new one will be instantiated automatically.
        /// </summary>
        public GameObject LabelCanvas;

        /// <summary>
        /// This controls whether or not Indoor Entry Markers can be clicked or tapped on to enter a building.
        /// This is set automatically from the Unity Inspector panel for the WrldMap object.
        /// </summary>
        public bool EnableIndoorEntryMarkerEvents
        {
            get { return m_nativeConfig.m_enableIndoorEntryMarkerEvents; }
            set { m_nativeConfig.m_enableIndoorEntryMarkerEvents = value; }
        }

        /// <summary>
        /// Allows configuration of the timeout for obtaining response for map tile resource web requests.
        /// </summary>
        public int ResourceWebRequestTimeoutSeconds
        {
            get { return m_nativeConfig.m_resourceWebRequestTimeoutSeconds; }
            set { m_nativeConfig.m_resourceWebRequestTimeoutSeconds = value; }
        }

        /// <summary>
        /// Defines whether the meshes generated at runtime should be uploaded to the GPU and discarded from RAM. Only applied for meshes with collision disabled.
        /// </summary>
        public bool UploadMeshesToGPU;


        /// <summary>
        /// Create and return a default configuration which sets up all parameters to default values.
        /// The map starts up at San Francisco (37.771092, -122.468385) at an altitude above sea level of
        /// 1781 meters while facing North (0 degrees heading). The default directory is set to WrldMaterials, which
        /// points to Assets/Wrld/Resources/WrldMaterials. And default landmark material is set to null
        /// </summary>
        public static ConfigParams MakeDefaultConfig()
        {
            var config = new ConfigParams();

            config.m_nativeConfig = new NativeConfig();

            config.LatitudeDegrees = 37.771092;
            config.LongitudeDegrees = -122.468385;
            config.DistanceToInterest = 1781.0;
            config.HeadingDegrees = 0.0;
            config.StreamingLodBasedOnDistance = false;

            config.MaterialsDirectory = "WrldMaterials/";
            config.IndoorMapMaterialsDirectory = "WrldMaterials/Achetypes";
            config.OverrideLandmarkMaterial = null;

            config.CoverageTreeManifestUrl = "";
            config.ThemeManifestUrl = "";

            config.Collisions = new CollisionConfig { TerrainCollision = false, RoadCollision = false, BuildingCollision = false };

            config.ViewportWidth = UnityEngine.Screen.width;
            config.ViewportHeight = UnityEngine.Screen.height;

            config.EnableLabels = false;
            config.EnableIndoorEntryMarkerEvents = true;
            config.LabelCanvas = null;
            config.ResourceWebRequestTimeoutSeconds = 60;

            config.UploadMeshesToGPU = false;

            return config;
        }
    }
}
