using UnityEngine;
using Wrld.Common.Maths;
using Wrld.Materials;
using Wrld.Streaming;
using Wrld.Space;
using Wrld.Common.Camera;
using Wrld.MapCamera;
using Wrld.Resources.Buildings;
using Wrld.Resources.IndoorMaps;
using Wrld.Space.Positioners;
using Wrld.Resources.Labels;
using Wrld.Space.EnvironmentFlattening;
using Wrld.Meshes;
using Wrld.Paths;
using Wrld.Precaching;
using Wrld.Transport;
using Wrld.Resources.Props;
using Wrld.Utilities;
using System;
using AOT;

namespace Wrld
{
    // :TODO: Feels like it might be more natural/usual to split this into ECEF & UnityWorld classes & have both implement same interface.
    public class ApiImplementation
    {
        private NativePluginRunner m_nativePluginRunner;
        private CoordinateSystem m_coordinateSystem;
        private CameraApiInternal m_cameraApiInternal;
        private CameraApi m_cameraApi;
        private BuildingsApi m_buildingsApi;
        private BuildingsApiInternal m_buildingsApiInternal;
        private IndoorMapsApi m_indoorMapsApi;
        private IndoorMapsApiInternal m_indoorMapsApiInternal;
        private IndoorMapEntityInformationApi m_indoorMapEntityInformationApi;
        private IndoorMapEntityInformationApiInternal m_indoorMapEntityInformationApiInternal;
        private GeographicApi m_geographicApi;
        private SpacesApi m_spacesApi;
        private PositionerApi m_positionerApi;
        private PositionerApiInternal m_positionerApiInternal;
        private EnvironmentFlatteningApi m_environmentFlatteningApi;
        private PathApi m_pathApi;
        private PathApiInternal m_pathApiInternal;
        private EnvironmentFlatteningApiInternal m_environmentFlatteningApiInternal;
        private PrecacheApi m_precacheApi;
        private PrecacheApiInternal m_precacheApiInternal;
        private PropsApi m_propsApi;
        private PropsApiInternal m_propsApiInternal;
        private TransportApi m_transportApi;
        private TransportApiInternal m_transportApiInternal;
        private UnityWorldSpaceCoordinateFrame m_frame;
        private DoubleVector3 m_originECEF;
        private InterestPointProvider m_interestPointProvider;
        private GameObjectStreamer m_terrainStreamer;
        private GameObjectStreamer m_roadStreamer;
        private GameObjectStreamer m_buildingStreamer;
        private GameObjectStreamer m_highlightStreamer;
        private GameObjectStreamer m_indoorMapStreamer;
        private MapGameObjectScene m_mapGameObjectScene;
        private LabelServiceInternal m_labelServiceInternal;
        private ITransformUpdateStrategy m_transformUpdateStrategy;
        private GameObject m_root;
        private IntPtr m_handleToSelf;
        public event Action OnInitialStreamingCompleteInternal;
        public delegate void NativeOnInitialStreamingCompleteDelegate(IntPtr internalApiHandle);

        public ApiImplementation(string apiKey, CoordinateSystem coordinateSystem, Transform parentTransformForStreamedObjects, ConfigParams configParams)
        {
            var textureLoadHandler = new TextureLoadHandler();
            var materialRepository = new MaterialRepository(configParams.MaterialsDirectory, configParams.OverrideLandmarkMaterial, textureLoadHandler);

            var terrainCollision = (configParams.Collisions.TerrainCollision) ? CollisionStreamingType.SingleSidedCollision : CollisionStreamingType.NoCollision;
            var roadCollision = (configParams.Collisions.RoadCollision) ? CollisionStreamingType.DoubleSidedCollision : CollisionStreamingType.NoCollision;
            var buildingCollision = (configParams.Collisions.BuildingCollision) ? CollisionStreamingType.SingleSidedCollision : CollisionStreamingType.NoCollision;

            m_root = CreateRootObject(parentTransformForStreamedObjects);

            m_interestPointProvider = new InterestPointProvider(m_root.transform);

            m_terrainStreamer = new GameObjectStreamer("Terrain", materialRepository, m_root.transform, terrainCollision, true, configParams.UploadMeshesToGPU);
            m_roadStreamer = new GameObjectStreamer("Roads", materialRepository, m_root.transform, roadCollision, true, configParams.UploadMeshesToGPU);
            m_buildingStreamer = new GameObjectStreamer("Buildings", materialRepository, m_root.transform, buildingCollision, true, configParams.UploadMeshesToGPU);
            m_highlightStreamer = new GameObjectStreamer("Highlights", materialRepository, m_root.transform, CollisionStreamingType.NoCollision, false, configParams.UploadMeshesToGPU);
            m_indoorMapStreamer = new GameObjectStreamer("IndoorMaps", materialRepository, m_root.transform, CollisionStreamingType.NoCollision, false, configParams.UploadMeshesToGPU);

            var indoorMapMaterialRepository = new IndoorMapMaterialRepository();
            
            var indoorMapStreamedTextureObserver = new IndoorMapStreamedTextureObserver(indoorMapMaterialRepository);
            var indoorMapTextureStreamingService = new IndoorMapTextureStreamingService(textureLoadHandler, indoorMapStreamedTextureObserver);
            m_indoorMapsApiInternal = new IndoorMapsApiInternal(indoorMapTextureStreamingService, configParams.IndoorMapMaterialsDirectory);
            var indoorMapMaterialService = new IndoorMapMaterialService(indoorMapMaterialRepository, m_indoorMapsApiInternal);

            m_indoorMapsApi = new IndoorMapsApi(m_indoorMapsApiInternal);

            m_indoorMapEntityInformationApiInternal = new IndoorMapEntityInformationApiInternal();
            m_indoorMapEntityInformationApi = new IndoorMapEntityInformationApi(m_indoorMapEntityInformationApiInternal);

            var meshUploader = new MeshUploader();
            var indoorMapScene = new IndoorMapScene(m_indoorMapStreamer, meshUploader, indoorMapMaterialService, m_indoorMapsApiInternal);
            m_mapGameObjectScene = new MapGameObjectScene(m_terrainStreamer, m_roadStreamer, m_buildingStreamer, m_highlightStreamer, m_indoorMapStreamer, meshUploader, indoorMapScene);
            m_labelServiceInternal = new LabelServiceInternal(configParams.LabelCanvas, configParams.EnableLabels, textureLoadHandler);

            m_positionerApiInternal = new PositionerApiInternal();
            m_positionerApi = new PositionerApi(m_positionerApiInternal);

            m_cameraApiInternal = new CameraApiInternal();

            m_buildingsApiInternal = new BuildingsApiInternal(materialRepository);
            m_buildingsApi = new BuildingsApi(m_buildingsApiInternal);

            m_precacheApiInternal = new PrecacheApiInternal();
            m_precacheApi = new PrecacheApi(m_precacheApiInternal);

            m_propsApiInternal = new PropsApiInternal();
            m_propsApi = new PropsApi(m_propsApiInternal);

            m_transportApiInternal = new TransportApiInternal();
            m_transportApi = new TransportApi(m_transportApiInternal);

            m_handleToSelf = NativeInteropHelpers.AllocateNativeHandleForObject(this);

            m_nativePluginRunner = new NativePluginRunner(
                apiKey, 
                textureLoadHandler, 
                materialRepository, 
                m_mapGameObjectScene, 
                configParams, 
                indoorMapScene, 
                m_indoorMapsApiInternal, 
                indoorMapMaterialService, 
                m_labelServiceInternal, 
                m_positionerApiInternal,
                m_cameraApiInternal,
                m_buildingsApiInternal,
                m_precacheApiInternal,
                m_transportApiInternal,
                m_indoorMapEntityInformationApiInternal,
                m_handleToSelf
                );

            m_cameraApi = new CameraApi(this, m_cameraApiInternal);

            m_coordinateSystem = coordinateSystem;
            var defaultStartingLocation = LatLongAltitude.FromDegrees(
                configParams.LatitudeDegrees, 
                configParams.LongitudeDegrees, 
                coordinateSystem == CoordinateSystem.ECEF ? configParams.DistanceToInterest : 0.0);

            m_originECEF = defaultStartingLocation.ToECEF();

            if (coordinateSystem == CoordinateSystem.UnityWorld)
            {
                m_frame = new UnityWorldSpaceCoordinateFrame(defaultStartingLocation);
            }

            m_geographicApi = new GeographicApi(m_root.transform);
            
            m_environmentFlatteningApiInternal = new EnvironmentFlatteningApiInternal();
            m_environmentFlatteningApi = new EnvironmentFlatteningApi(m_environmentFlatteningApiInternal);

            m_pathApiInternal = new PathApiInternal();
            m_pathApi = new PathApi(m_pathApiInternal);

            m_spacesApi = new SpacesApi(this);

            if (m_coordinateSystem == CoordinateSystem.UnityWorld)
            {
                m_transformUpdateStrategy = new UnityWorldSpaceTransformUpdateStrategy(m_frame, m_environmentFlatteningApi.GetCurrentScale());
            }
            else
            {
                var cameraPosition = m_originECEF;
                m_transformUpdateStrategy = new ECEFTransformUpdateStrategy(
                    cameraPosition,
                    cameraPosition.normalized.ToSingleVector(),
                    m_environmentFlatteningApi.GetCurrentScale());
            }
        }

        private static GameObject CreateRootObject(Transform parentTransform)
        {
            var result = new GameObject("Root");
            const bool preserveWorldSpacePosition = false; 


            for (int childIndex = parentTransform.childCount - 1; childIndex >= 0; --childIndex)
            {
                var child = parentTransform.GetChild(childIndex);
                child.SetParent(result.transform, preserveWorldSpacePosition);
            }

            result.transform.SetParent(parentTransform, preserveWorldSpacePosition);

            return result;
        }

        public void SetOriginPoint(LatLongAltitude lla)
        {
            m_originECEF = lla.ToECEF();

            if (m_coordinateSystem == CoordinateSystem.ECEF)
            {
                if (m_cameraApi.HasControlledCamera)
                {
                    m_cameraApi.MoveTo(lla.GetLatLong());
                }
                else
                {   
                    UpdateTransforms();
                }
            }
            else
            {
                m_frame.SetCentralPoint(lla);
                UpdateTransforms();
            }
        }

        internal void CalculateCameraParameters(NativeCameraState nativeCameraState, out DoubleVector3 cameraPositionECEF, out DoubleVector3 interestPointECEF, out Vector3 cameraUpECEF)
        {
            var scaledInterestPoint = ScaleInterestPointWithEnvironmentFlattening(nativeCameraState.interestPointECEF);

            Vector3 forward, up;
            DoubleVector3 positionECEF;
            CameraHelpers.CalculateLookAt(
                scaledInterestPoint,
                nativeCameraState.interestBasisForwardECEF,
                nativeCameraState.pitchDegrees * Mathf.Deg2Rad,
                nativeCameraState.distanceToInterestPoint,
                out positionECEF, out forward, out up);

            interestPointECEF = scaledInterestPoint;
            cameraPositionECEF = positionECEF;
            cameraUpECEF = up;
        }

        internal void ApplyNativeCameraState(NativeCameraState nativeCameraState, UnityEngine.Camera controlledCamera)
        {
            // assuming uniform scale
            var mapScale = m_root.transform.lossyScale;

            Debug.Assert(Mathf.Approximately(mapScale.x, mapScale.y) && Mathf.Approximately(mapScale.x, mapScale.z),
                "When using built-in camera controls the map must have a uniform scale (identical along all axes).");

            controlledCamera.fieldOfView = nativeCameraState.fieldOfViewDegrees;
            controlledCamera.nearClipPlane = nativeCameraState.nearClipPlaneDistance * mapScale.x;
            controlledCamera.farClipPlane = nativeCameraState.farClipPlaneDistance * mapScale.x;

            DoubleVector3 cameraPositionECEF, interestPointECEF;
            Vector3 cameraUpECEF;
            CalculateCameraParameters(nativeCameraState, out cameraPositionECEF, out interestPointECEF, out cameraUpECEF);
            m_interestPointProvider.UpdateFromNative(interestPointECEF);

            var cameraRotation = new Quaternion();

            if (m_coordinateSystem == CoordinateSystem.ECEF)
            {
                var position = (cameraPositionECEF - m_originECEF).ToSingleVector();                
                var interestPointPosition = (interestPointECEF - m_originECEF).ToSingleVector();
                var cameraToInterestPoint = interestPointPosition - position;

                controlledCamera.transform.position = m_root.transform.TransformPoint(position);                
                cameraRotation.SetLookRotation(cameraToInterestPoint.normalized, cameraUpECEF);
                controlledCamera.transform.rotation = m_root.transform.rotation * cameraRotation;
            }
            else // if (m_coordinateSystem == CoordinateSystem.UnityWorld)
            {
                var localCameraPosition = m_frame.ECEFToLocalSpace(cameraPositionECEF);
                controlledCamera.transform.position = m_root.transform.TransformPoint(localCameraPosition);
                var localUp = m_frame.ECEFToLocalRotation * cameraUpECEF;
                var cameraToInterestPointECEF = (interestPointECEF - cameraPositionECEF).ToSingleVector();
                var localViewDirection = m_frame.ECEFToLocalRotation * cameraToInterestPointECEF;
                cameraRotation.SetLookRotation(localViewDirection.normalized, localUp);
                controlledCamera.transform.rotation = m_root.transform.rotation * cameraRotation;
            }
        }

        private DoubleVector3 ScaleInterestPointWithEnvironmentFlattening(DoubleVector3 interestPoint)
        {
            var environmentFlatteningScale = 1.0f;

            if(m_indoorMapsApi.GetActiveIndoorMap() == null)
            {
                environmentFlatteningScale = m_environmentFlatteningApi.GetCurrentScale();
            }

            var scaledPointEcef = DoubleVector3.Lerp(interestPoint.normalized * EarthConstants.Radius, interestPoint, environmentFlatteningScale);

            return scaledPointEcef;
        }

        public void StreamResourcesForBuiltInCamera(Camera streamingCamera)
        {
            m_nativePluginRunner.StreamResourcesForBuiltInCamera();

            // re-center the camera for ECEF
            if (m_coordinateSystem == CoordinateSystem.ECEF)
            {
                var mapSpaceCameraPosition = m_root.transform.InverseTransformPoint(streamingCamera.transform.position);
                m_originECEF += mapSpaceCameraPosition;
                streamingCamera.transform.position = m_root.transform.position;
            }
        }

        public void StreamResourcesForCamera(Camera streamingCamera)
        {
            if (IsValidStreamingCamera(streamingCamera))
            {
                var cameraState = GetStateForCustomCamera(streamingCamera);
                m_nativePluginRunner.StreamResourcesForCamera(cameraState);
            }
        }

        private bool IsValidStreamingCamera(Camera camera)
        {
            bool hasZeroSize = camera.orthographic && camera.orthographicSize == 0.0f;

            if (hasZeroSize)
            {
                Debug.LogError("Camera Orthographic Size must be greater than 0 for correct frustum calculation");
            }

            return !hasZeroSize;
        }

        private void UpdateTransforms()
        {
            m_transformUpdateStrategy.UpdateStrategy(m_originECEF, m_environmentFlatteningApi.GetCurrentScale());
            m_nativePluginRunner.UpdateTransforms(m_transformUpdateStrategy);
            m_geographicApi.UpdateTransforms(m_transformUpdateStrategy);
        }

        [MonoPInvokeCallback(typeof(NativeOnInitialStreamingCompleteDelegate))]
        public static void OnNativeInitialStreamingComplete(IntPtr internalApiHandle)
        {
            var apiImplementation = internalApiHandle.NativeHandleToObject<ApiImplementation>();
            apiImplementation.OnInitialStreamingCompleteInternal();
        }

        public CameraApi CameraApi
        {
            get
            {
                return m_cameraApi;
            }
        }

        public BuildingsApi BuildingsApi
        {
            get
            {
                return m_buildingsApi;
            }
        }

        public IndoorMapsApi IndoorMapsApi
        {
            get
            {
                return m_indoorMapsApi;
            }
        }

        public IndoorMapEntityInformationApi IndoorMapEntityInformationApi
        {
            get
            {
                return m_indoorMapEntityInformationApi;
            }
        }

        public GeographicApi GeographicApi
        {
            get
            {
                return m_geographicApi;
            }
        }

        public SpacesApi SpacesApi
        {
            get
            {
                return m_spacesApi;
            }
        }

        public PositionerApi PositionerApi
        {
            get
            {
                return m_positionerApi;
            }
        }

        public EnvironmentFlatteningApi EnvironmentFlatteningApi
        {
            get
            {
                return m_environmentFlatteningApi;
            }
        }

        public PathApi PathApi
        {
            get
            {
                return m_pathApi;
            }
        }

        public PrecacheApi PrecacheApi
        {
            get
            {
                return m_precacheApi;
            }
        }

        public PropsApi PropsApi
        {
            get
            {
                return m_propsApi;
            }
        }

        public TransportApi TransportApi
        {
            get
            {
                return m_transportApi;
            }
        }


        public void Update()
        {
            m_cameraApi.UpdateInput();

            if (m_cameraApiInternal.CustomRenderCamera != null)
            {
                var cameraState = GetStateForCustomCamera(m_cameraApiInternal.CustomRenderCamera);
                m_cameraApiInternal.SetCustomRenderCameraState(cameraState);
            }

            m_nativePluginRunner.Update();

            if (m_cameraApi.HasControlledCamera)
            {
                var cameraState = m_cameraApiInternal.GetNativeCameraState();
                ApplyNativeCameraState(cameraState, m_cameraApi.GetControlledCamera());
            }

            UpdateTransforms();
        }

        public void UpdateCollision(ConfigParams.CollisionConfig collisions)
        {
            m_nativePluginRunner.UpdateCollisions(collisions);
        }

        internal void SetEnabled(bool enabled)
        {
            m_mapGameObjectScene.SetEnabled(enabled);
        }

        public void Destroy()
        {
            m_geographicApi.Destroy();
            m_nativePluginRunner.OnDestroy();
            m_terrainStreamer.Destroy();
            m_roadStreamer.Destroy();
            m_buildingStreamer.Destroy();
            m_highlightStreamer.Destroy();
            m_indoorMapStreamer.Destroy();
            m_labelServiceInternal.Destroy();
            m_positionerApiInternal.Destroy();
            m_mapGameObjectScene.Destroy();
            m_cameraApiInternal.Destroy();
            m_precacheApiInternal.Destroy();
            m_transportApiInternal.Destroy();

            GameObject.Destroy(m_root);
            m_root = null;
        }

        public void ResetRootChilds()
        {
            var parent = m_root.transform.parent;
            int childCount = m_root.transform.childCount;

            for (int childIndex = childCount - 1; childIndex >= 0; --childIndex)
            {
                var child = m_root.transform.GetChild(childIndex);
                child.SetParent(parent.transform, false);
            }
        }

        internal Vector3 GeographicToWorldPoint(LatLongAltitude position)
        {
            Vector3 mapSpacePoint;

            if (m_coordinateSystem == CoordinateSystem.UnityWorld)
            {
                mapSpacePoint = m_frame.ECEFToLocalSpace(position.ToECEF());
            }
            else
            {
                mapSpacePoint = (position.ToECEF() - m_originECEF).ToSingleVector();
            }

            return m_root.transform.TransformPoint(mapSpacePoint);
        }

        internal LatLongAltitude WorldToGeographicPoint(Vector3 position)
        {
            var mapSpacePoint = m_root.transform.InverseTransformPoint(position);

            if (m_coordinateSystem == CoordinateSystem.UnityWorld)
            {
                return m_frame.LocalSpaceToLatLongAltitude(mapSpacePoint);
            }
            else
            {
                var ecefPosition = m_originECEF + mapSpacePoint;
                return LatLongAltitude.FromECEF(ecefPosition);
            }
        }

        internal Vector3 GeographicToViewportPoint(LatLongAltitude position, Camera camera)
        {
            var unityWorldSpacePosition = GeographicToWorldPoint(position);
            return camera.WorldToViewportPoint(unityWorldSpacePosition);
        }

        internal LatLongAltitude ViewportToGeographicPoint(Vector3 viewportSpacePosition, Camera camera)
        {
            var unityWorldSpacePosition = camera.ViewportToWorldPoint(viewportSpacePosition);
            return WorldToGeographicPoint(unityWorldSpacePosition);
        }

        internal CameraState GetStateForCustomCamera(Camera camera)
        {
            var mapTransform = m_root.transform;
            var savedRotation = camera.transform.rotation;
            var savedPosition = camera.transform.position;
            DoubleVector3 finalOriginECEF;
            Matrix4x4 viewMatrixECEF;
            var mapSpaceCameraPosition = mapTransform.InverseTransformPoint(camera.transform.position);

            if (m_coordinateSystem == CoordinateSystem.ECEF)
            {
                finalOriginECEF = m_originECEF + mapSpaceCameraPosition;

                camera.transform.position = mapTransform.position;
                viewMatrixECEF = camera.worldToCameraMatrix * mapTransform.localToWorldMatrix;
                viewMatrixECEF.SetColumn(3, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
            }
            else // if (m_coordinateSystem == CoordinateSystem.UnityWorld)
            {
                finalOriginECEF = m_frame.LocalSpaceToECEF(mapSpaceCameraPosition);

                var frameMatrix = Matrix4x4.TRS (Vector3.zero, m_frame.ECEFToLocalRotation, Vector3.one);
                viewMatrixECEF = camera.worldToCameraMatrix * mapTransform.localToWorldMatrix * frameMatrix;
                viewMatrixECEF.SetColumn (3, new Vector4 (0.0f, 0.0f, 0.0f, 1.0f));
            }

            DoubleVector3 interestPointECEF = m_interestPointProvider.CalculateInterestPoint(camera, finalOriginECEF);

            camera.transform.rotation = savedRotation;
            camera.transform.position = savedPosition;

            return new CameraState(finalOriginECEF, interestPointECEF, viewMatrixECEF, camera.projectionMatrix);
        }

        public void OnApplicationPaused()
        {
            m_nativePluginRunner.OnApplicationPaused();
        }

        public void OnApplicationResumed()
        {
            m_nativePluginRunner.OnApplicationResumed();
        }
    }
}
