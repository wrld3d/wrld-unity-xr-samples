using Wrld.MapCamera;
using Wrld.Materials;
using Wrld.Space;
using Wrld.Concurrency;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Wrld.Resources.Buildings;
using Wrld.Resources.IndoorMaps;
using Wrld.Space.Positioners;
using Wrld.Resources.Labels;
using System.Text;
using Wrld.Precaching;
using Wrld.Transport;

namespace Wrld
{
    public class NativePluginRunner
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        public const string DLL = "wrld-unity-android";
#elif UNITY_IOS && !UNITY_EDITOR
        public const string DLL = "__Internal";
#else
        public const string DLL = "StreamAlpha";
#endif

        [DllImport(DLL)]
        private static extern void Initialize(
            int screenWidth,
            int screenHeight,
            float screenDPI,
            [MarshalAs(UnmanagedType.LPStr)]string apiKey,
            [MarshalAs(UnmanagedType.LPArray)]byte[] assetPath,
            ref ConfigParams.NativeConfig config,
            ref ApiCallbacks apiCallbacks,
            [MarshalAs(UnmanagedType.LPStr)]string coverageTreeUrl,
            [MarshalAs(UnmanagedType.LPStr)]string themeUrl
            );

        [DllImport(DLL)]
        private static extern void Update(float t);

        [DllImport(DLL)]
        private static extern void Destroy();

        [DllImport(DLL)]
        private static extern IntPtr GetAppInterface();

        [DllImport(DLL)]
        private static extern void Pause();

        [DllImport(DLL)]
        private static extern void Resume();

        public static IntPtr API;

        MaterialRepository m_materialRepository;
        TextureLoadHandler m_textureLoadHandler;
        MapGameObjectScene m_mapGameObjectScene;
        StreamingUpdater m_streamingUpdater;
        ThreadService m_threadService;

        private bool m_isRunning = false;

        private static string GetStreamingAssetsDir()
        {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_WIN || UNITY_STANDALONE
            var path = Application.streamingAssetsPath + "/WrldResources";
#elif UNITY_IOS
            var path = "Data/Raw/WrldResources/";
#elif UNITY_ANDROID
            var path = "jar:file://" + Application.dataPath + "!/assets/";
#endif
            return path;
        }

        internal NativePluginRunner(
            string apiKey, 
            TextureLoadHandler textureLoadHandler, 
            MaterialRepository materialRepository, 
            MapGameObjectScene mapGameObjectScene, 
            ConfigParams config, 
            IndoorMapScene indoorMapScene,
            IndoorMapsApiInternal indoorMapsApiInternal, 
            IndoorMapMaterialService indoorMapMaterialService,
            LabelServiceInternal labelServiceInternal,
            PositionerApiInternal positionerApiInternal,
            CameraApiInternal cameraApiInternal,
            BuildingsApiInternal buildingsApiInternal,
            PrecacheApiInternal precacheApiInternal,
            TransportApiInternal transportApiInternal,
            IndoorMapEntityInformationApiInternal indoorMapEntityInformationApiInternal,
            IntPtr apiHandle
            )
        {
            m_threadService = new ThreadService();
            m_textureLoadHandler = textureLoadHandler;
            m_materialRepository = materialRepository;
            m_mapGameObjectScene = mapGameObjectScene;
            m_streamingUpdater = new StreamingUpdater();

            var nativeConfig = config.GetNativeConfig();
            var pathString = GetStreamingAssetsDir();
            var pathBytes = GetNullTerminatedUTF8Bytes(pathString);

            var indoorMapsApiHandle = indoorMapsApiInternal.GetHandle();
            var indoorMapMaterialServiceHandle = indoorMapMaterialService.GetHandle();

            var apiCallbacks = new ApiCallbacks(
                indoorMapsApiHandle,
                indoorMapMaterialServiceHandle,
                indoorMapScene.GetHandle(),
                cameraApiInternal.GetHandle(),
                buildingsApiInternal.GetHandle(),
                m_threadService.GetHandle(), 
                textureLoadHandler.GetHandle(), 
                mapGameObjectScene.GetHandle(), 
                labelServiceInternal.GetHandle(), 
                positionerApiInternal.GetHandle(),
                precacheApiInternal.GetHandle(),
                transportApiInternal.GetHandle(),
                indoorMapEntityInformationApiInternal.GetHandle(),
                apiHandle
                );

            Initialize(Screen.width, Screen.height, Screen.dpi,
                apiKey,
                pathBytes,
                ref nativeConfig,
                ref apiCallbacks,
                config.CoverageTreeManifestUrl,
                config.ThemeManifestUrl
                );

            API = GetAppInterface();
            Debug.Assert(API != IntPtr.Zero);

            m_isRunning = true;
        }

        public void StreamResourcesForCamera(CameraState cameraState)
        {
            m_streamingUpdater.Update(cameraState);
        }

        public void StreamResourcesForBuiltInCamera()
        {
            m_streamingUpdater.UpdateForBuiltInCamera();
        }

        public void Update()
        {
            if (m_isRunning)
            {
                Update(Time.deltaTime);
            }

            m_textureLoadHandler.Update();
            m_materialRepository.Update();
        }

        public void UpdateTransforms(ITransformUpdateStrategy transformUpdateStrategy)
        {
            m_mapGameObjectScene.UpdateTransforms(transformUpdateStrategy);
        }

        public void UpdateCollisions(ConfigParams.CollisionConfig collisions)
        {
            m_mapGameObjectScene.ChangeCollision(collisions);
        }

        public void OnDestroy()
        {
            Destroy();
            m_threadService.Destroy();
            m_textureLoadHandler.Destroy();
        }

        public void OnApplicationPaused()
        {
            if (m_isRunning)
            {
                Pause();
                m_isRunning = false;                
            }
        }

        public void OnApplicationResumed()
        {
            if (!m_isRunning)
            {
                Resume();
                m_isRunning = true;
            }
        }

        private static byte[] GetNullTerminatedUTF8Bytes(string input)
        {
            var byteArray = Encoding.UTF8.GetBytes(input);
            var zeroTerminatedByteArray = new byte[byteArray.Length + 1];
            Array.Copy(byteArray, zeroTerminatedByteArray, byteArray.Length);
            zeroTerminatedByteArray[byteArray.Length] = 0;
            return zeroTerminatedByteArray;
        }
    }
}

