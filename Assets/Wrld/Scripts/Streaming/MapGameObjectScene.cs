using AOT;
using Wrld.Common.Maths;
using Wrld.Meshes;
using Wrld.Space;
using Wrld.Streaming;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Wrld.Utilities;
using Wrld.Interop;

namespace Wrld
{
    public class MapGameObjectScene
    {
        public delegate void AddMeshCallback(IntPtr sceneServiceHandle, [MarshalAs(UnmanagedType.LPStr)] string id);
        public delegate void DeleteMeshCallback(IntPtr sceneServiceHandle, [MarshalAs(UnmanagedType.LPStr)] string id);
        public delegate void VisibilityCallback(IntPtr sceneServiceHandle, [MarshalAs(UnmanagedType.LPStr)] string id, [MarshalAs(UnmanagedType.I1)] bool visible);
        internal delegate void SetScaleCallback(IntPtr sceneServiceHandle, [MarshalAs(UnmanagedType.LPStr)] string id, ref Vector3 scale);
        internal delegate void SetTranslationCallback(IntPtr sceneServiceHandle, [MarshalAs(UnmanagedType.LPStr)] string id, ref Vector3 translation);
        internal delegate void SetOrientationCallback(IntPtr sceneServiceHandle, [MarshalAs(UnmanagedType.LPStr)] string id, ref Quaternion orientation);
        internal delegate void SetColorCallback(IntPtr sceneServiceHandle, [MarshalAs(UnmanagedType.LPStr)] string id, ref ColorInterop color);
        
        GameObjectStreamer m_terrainStreamer;
        GameObjectStreamer m_roadStreamer;
        GameObjectStreamer m_buildingStreamer;
        GameObjectStreamer m_highlightStreamer;
        GameObjectStreamer m_indoorMapStreamer;
        MeshUploader m_meshUploader;
        bool m_enabled;
        IntPtr m_handleToSelf;

        internal MapGameObjectScene(GameObjectStreamer terrainStreamer, GameObjectStreamer roadStreamer, GameObjectStreamer buildingStreamer, GameObjectStreamer highlightStreamer, GameObjectStreamer indoorMapStreamer, MeshUploader meshUploader, IndoorMapScene indoorMapScene)
        {
            m_terrainStreamer = terrainStreamer;
            m_roadStreamer = roadStreamer;
            m_buildingStreamer = buildingStreamer;
            m_highlightStreamer = highlightStreamer;
            m_indoorMapStreamer = indoorMapStreamer;
            m_meshUploader = meshUploader;
            m_enabled = true;
            m_handleToSelf = NativeInteropHelpers.AllocateNativeHandleForObject(this);
        }

        internal IntPtr GetHandle()
        {
            return m_handleToSelf;
        }

        internal void Destroy()
        {
            NativeInteropHelpers.FreeNativeHandle(m_handleToSelf);
        }

        internal void SetEnabled(bool enabled)
        {
            m_enabled = enabled;
        }
        public void ChangeCollision(ConfigParams.CollisionConfig collisions)
        {
            var terrainCollision = (collisions.TerrainCollision) ? CollisionStreamingType.SingleSidedCollision : CollisionStreamingType.NoCollision;
            var roadCollision = (collisions.RoadCollision) ? CollisionStreamingType.DoubleSidedCollision : CollisionStreamingType.NoCollision;
            var buildingCollision = (collisions.BuildingCollision) ? CollisionStreamingType.SingleSidedCollision : CollisionStreamingType.NoCollision;
            m_terrainStreamer.ChangeCollision(terrainCollision);
            m_roadStreamer.ChangeCollision(roadCollision);
            m_buildingStreamer.ChangeCollision(buildingCollision);
        }
        
        public void UpdateTransforms(ITransformUpdateStrategy transformUpdateStrategy)
        {
            const float roadHeightOffset = 0.1f;
            m_terrainStreamer.UpdateTransforms(transformUpdateStrategy);
            m_roadStreamer.UpdateTransforms(transformUpdateStrategy, roadHeightOffset);
            m_buildingStreamer.UpdateTransforms(transformUpdateStrategy);
            m_highlightStreamer.UpdateTransforms(transformUpdateStrategy);
            m_indoorMapStreamer.UpdateTransforms(transformUpdateStrategy);
        }

        [MonoPInvokeCallback(typeof(DeleteMeshCallback))]
        public static void DeleteMesh(IntPtr sceneServiceHandle, [MarshalAs(UnmanagedType.LPStr)] string meshID)
        {
            var sceneService = sceneServiceHandle.NativeHandleToObject<MapGameObjectScene>();
            sceneService.DeleteMeshInternal(meshID);
        }

        private void DeleteMeshInternal(string id)
        {
            if (m_enabled)
            {
                var streamer = GetStreamerFromName(id);
                streamer.RemoveObjects(id);
            }
        }

        [MonoPInvokeCallback(typeof(AddMeshCallback))]
        public static void AddMesh(IntPtr sceneServiceHandle, [MarshalAs(UnmanagedType.LPStr)] string meshID)
        {
            var sceneService = sceneServiceHandle.NativeHandleToObject<MapGameObjectScene>();
            sceneService.AddMeshInternal(meshID);
        }

        private void AddMeshInternal(string id)
        {
            if (m_enabled)
            {
                Mesh[] meshes;
                DoubleVector3 originECEF;
                string materialName;

                if (m_meshUploader.TryGetUnityMeshesForID(id, out meshes, out originECEF, out materialName))
                {
                    m_meshUploader.RemoveUploadedMesh(id);
                    var streamer = GetStreamerFromName(id);
                    streamer.AddObjectsForMeshes(id, meshes, originECEF, Vector3.zero, Quaternion.identity, materialName);
                }
                else
                {
                    Debug.LogErrorFormat("ERROR: Could not get mesh for ID {0}.", id);
                }
            }
        }

        [MonoPInvokeCallback(typeof(VisibilityCallback))]
        public static void SetVisible(IntPtr sceneServiceHandle, [MarshalAs(UnmanagedType.LPStr)] string meshID, [MarshalAs(UnmanagedType.I1)] bool visible)
        {
            var sceneService = sceneServiceHandle.NativeHandleToObject<MapGameObjectScene>();
            sceneService.SetVisibleInternal(meshID, visible);
        }

        private void SetVisibleInternal(string id, bool visible)
        {
            if (m_enabled)
            {
                var streamer = GetStreamerFromName(id);
                streamer.SetVisible(id, visible);
            }
        }

        [MonoPInvokeCallback(typeof(SetScaleCallback))]
        internal static void SetScale(IntPtr sceneServiceHandle, [MarshalAs(UnmanagedType.LPStr)] string objectID, ref Vector3 scale)
        {
            var sceneService = sceneServiceHandle.NativeHandleToObject<MapGameObjectScene>();
            sceneService.SetScale(objectID, scale);
        }

        private void SetScale(string objectID, Vector3 scale)
        {
            var streamer = GetStreamerFromName(objectID);
            streamer.SetScale(objectID, scale);
        }

        [MonoPInvokeCallback(typeof(SetTranslationCallback))]
        internal static void SetTranslation(IntPtr sceneServiceHandle, [MarshalAs(UnmanagedType.LPStr)] string objectID, ref Vector3 translation)
        {
            var sceneService = sceneServiceHandle.NativeHandleToObject<MapGameObjectScene>();
            sceneService.SetTranslationInternal(objectID, translation);
        }

        private void SetTranslationInternal(string objectID, Vector3 translation)
        {
            var streamer = GetStreamerFromName(objectID);
            streamer.SetTranslation(objectID, translation);            
        }

        [MonoPInvokeCallback(typeof(SetOrientationCallback))]
        internal static void SetOrientation(IntPtr sceneServiceHandle, [MarshalAs(UnmanagedType.LPStr)] string meshID, ref Quaternion orientation)
        {
            var sceneService = sceneServiceHandle.NativeHandleToObject<MapGameObjectScene>();
            sceneService.SetOrientationInternal(meshID, orientation);
        }

        private void SetOrientationInternal(string meshID, Quaternion orientation)
        {
            var streamer = GetStreamerFromName(meshID);
            streamer.SetOrientation(meshID, orientation);
        }

        [MonoPInvokeCallback(typeof(SetColorCallback))]
        internal static void SetColor(IntPtr sceneServiceHandle, [MarshalAs(UnmanagedType.LPStr)] string objectID, ref ColorInterop color)
        {
            var sceneService = sceneServiceHandle.NativeHandleToObject<MapGameObjectScene>();
            sceneService.SetColorInternal(objectID, color.ToColor());            
        }

        private void SetColorInternal(string objectID, Color color)
        {
            var streamer = GetStreamerFromName(objectID);
            streamer.SetColor(objectID, color);
        }

        private GameObjectStreamer GetStreamerFromName(string name)
        {
            // :TODO: replace this string lookup with a shared type enum or similar
            if (name.StartsWith("Raster") || name.StartsWith("Terrain"))
            {
                return m_terrainStreamer;
            }

            switch (name[0])
            {
                case 'M':
                case 'L':
                    return m_terrainStreamer;
                case 'R':
                    return m_roadStreamer;
                case 'B':
                    return m_buildingStreamer;
                case 'H':
                    return m_highlightStreamer;
                case 'I':
                    return m_indoorMapStreamer;
                default:
                    throw new ArgumentException(string.Format("Unknown streamer for name: {0}", name), "name");
            }
        }
    }
}

