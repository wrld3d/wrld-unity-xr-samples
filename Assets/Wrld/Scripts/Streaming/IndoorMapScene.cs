using AOT;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Wrld.Common.Maths;
using Wrld.Meshes;
using Wrld.Resources.IndoorMaps;
using Wrld.Space.Positioners;
using Wrld.Streaming;
using Wrld.Utilities;

namespace Wrld
{
    internal class IndoorMapScene
    {
        private GameObjectStreamer m_indoorMapStreamer;
        private MeshUploader m_meshUploader;
        private IndoorMapMaterialService m_materialRepository;
        private IndoorMapsApiInternal m_indoorMapsApiInternal;

        private IntPtr m_handleToSelf;

        internal IndoorMapScene(GameObjectStreamer indoorMapStreamer, MeshUploader meshUploader, IndoorMapMaterialService materialRepository, IndoorMapsApiInternal indoorMapsApiInternal)
        {
            m_indoorMapStreamer = indoorMapStreamer;
            m_meshUploader = meshUploader;
            m_materialRepository = materialRepository;
            m_indoorMapsApiInternal = indoorMapsApiInternal;

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

        internal delegate void SetMaterialCallback(IntPtr indoorMapSceneHandle, [MarshalAs(UnmanagedType.LPStr)] string objectID, IntPtr material);

        [MonoPInvokeCallback(typeof(SetMaterialCallback))]
        internal static void SetMaterial(IntPtr indoorMapSceneHandle, [MarshalAs(UnmanagedType.LPStr)] string objectID, IntPtr materialHandle)
        {
            var indoorMapScene = indoorMapSceneHandle.NativeHandleToObject<IndoorMapScene>();
            indoorMapScene.SetMaterialInternal(objectID, materialHandle);
        }

        [MonoPInvokeCallback(typeof(SetMaterialCallback))]
        internal static void SetMaterialForInstancedRenderable(IntPtr indoorMapSceneHandle, [MarshalAs(UnmanagedType.LPStr)] string objectID, IntPtr materialHandle)
        {
            var indoorMapScene = indoorMapSceneHandle.NativeHandleToObject<IndoorMapScene>();
            indoorMapScene.SetMaterialForInstancedRenderableInternal(objectID, materialHandle);
        }

        internal void SetMaterialInternal(string id, IntPtr materialHandle)
        {
            var gameObject = m_indoorMapStreamer.GetObject(id);
            var renderables = gameObject.GetComponentsInChildren<IndoorMapRenderable>(true);
            var materialInstance = m_materialRepository.InstantiateMaterial(materialHandle);

            foreach (var renderable in renderables)
            {
                renderable.Material = materialInstance;
            }
        }

        internal void SetMaterialForInstancedRenderableInternal(string id, IntPtr materialHandle)
        {
            var gameObject = m_indoorMapStreamer.GetObject(id);
            var renderables = gameObject.GetComponentsInChildren<InstancedIndoorMapRenderable>(true);

            foreach (var renderable in renderables)
            {
                var materialInstance = m_materialRepository.InstantiateMaterial(materialHandle);
                renderable.Material = materialInstance;
            }
        }

        public delegate void AddRenderableCallback(IntPtr indoorMapSceneHandle, [MarshalAs(UnmanagedType.LPStr)] string id, [MarshalAs(UnmanagedType.LPStr)] string indoorMapName, IntPtr nativeInstance);
        public delegate void AddInstancedRenderableCallback(
            IntPtr indoorMapSceneHandle,
            [MarshalAs(UnmanagedType.LPStr)] string id,
            [MarshalAs(UnmanagedType.LPStr)] string renderableId,
            [MarshalAs(UnmanagedType.LPStr)] string indoorMapName,
            ref DoubleVector3 originEcef,
            int transformCount,
            IntPtr positionOffsetsPtr,
            IntPtr orientationsPtr,
            IntPtr nativeInstance,
            int indexOffset,
            [MarshalAs(UnmanagedType.I1)] bool ownsMesh);

        [MonoPInvokeCallback(typeof(AddRenderableCallback))]
        public static void AddRenderable(IntPtr indoorMapSceneHandle, [MarshalAs(UnmanagedType.LPStr)] string meshID, [MarshalAs(UnmanagedType.LPStr)] string indoorMapName, IntPtr nativeRenderablePtr)
        {
            var indoorMapScene = indoorMapSceneHandle.NativeHandleToObject<IndoorMapScene>();
            indoorMapScene.AddRenderableInternal(meshID, indoorMapName, nativeRenderablePtr);
        }

        private void AddRenderableInternal(string id, string indoorMapName, IntPtr nativeRenderablePtr)
        {
            Mesh[] meshes;
            DoubleVector3 originECEF;
            string materialName;

            if (m_meshUploader.TryGetUnityMeshesForID(id, out meshes, out originECEF, out materialName))
            {
                m_meshUploader.RemoveUploadedMesh(id);
                var gameObjects = m_indoorMapStreamer.AddObjectsForMeshes(id, meshes, originECEF, Vector3.zero, Quaternion.identity, materialName);
                var descriptor = new IndoorMaterialDescriptor(indoorMapName, materialName);
                var material = m_indoorMapsApiInternal.IndoorMapMaterialFactory.CreateMaterialFromDescriptor(descriptor);

                foreach (var gameObject in gameObjects)
                {
                    gameObject.AddComponent<IndoorMapRenderable>().Init(nativeRenderablePtr, material, m_indoorMapsApiInternal);
                }
            }
            else
            {
                Debug.LogErrorFormat("ERROR: Could not get mesh for ID {0}.", id);
            }
        }

        [MonoPInvokeCallback(typeof(AddRenderableCallback))]
        public static void AddHighlightRenderable(IntPtr indoorMapSceneHandle, [MarshalAs(UnmanagedType.LPStr)] string meshID, [MarshalAs(UnmanagedType.LPStr)] string indoorMapName, IntPtr nativeRenderablePtr)
        {
            var indoorMapScene = indoorMapSceneHandle.NativeHandleToObject<IndoorMapScene>();
            indoorMapScene.AddHighlightRenderableInternal(meshID, indoorMapName, nativeRenderablePtr);
        }

        private void AddHighlightRenderableInternal(string id, string indoorMapName, IntPtr nativeRenderablePtr)
        {
            Mesh[] meshes;
            DoubleVector3 originECEF;
            string materialName;

            if (m_meshUploader.TryGetUnityMeshesForID(id, out meshes, out originECEF, out materialName))
            {
                m_meshUploader.RemoveUploadedMesh(id);
                var gameObjects = m_indoorMapStreamer.AddObjectsForMeshes(id, meshes, originECEF, Vector3.zero, Quaternion.identity, materialName);
                var descriptor = new IndoorMaterialDescriptor(indoorMapName, materialName);
                var material = m_indoorMapsApiInternal.IndoorMapMaterialFactory.CreateMaterialFromDescriptor(descriptor);

                foreach (var gameObject in gameObjects)
                {
                    gameObject.AddComponent<IndoorMapHighlightRenderable>().Init(nativeRenderablePtr, material, m_indoorMapsApiInternal);
                }
            }
            else
            {
                Debug.LogErrorFormat("ERROR: Could not get mesh for ID {0}.", id);
            }
        }

        [MonoPInvokeCallback(typeof(AddInstancedRenderableCallback))]
        public static void AddInstancedRenderable(
            IntPtr indoorMapSceneHandle,
            [MarshalAs(UnmanagedType.LPStr)] string meshID,
            [MarshalAs(UnmanagedType.LPStr)] string renderableId,
            [MarshalAs(UnmanagedType.LPStr)] string indoorMapName, 
            ref DoubleVector3 originEcef,
            int transformCount,
            IntPtr positionOffsetsPtr,
            IntPtr orientationsPtr,
            IntPtr nativeRenderablePtr,
            int indexOffset,
            [MarshalAs(UnmanagedType.I1)] bool ownsMesh)
        {
            var indoorMapScene = indoorMapSceneHandle.NativeHandleToObject<IndoorMapScene>();
            var positionFloats = new float[transformCount * 3];
            Marshal.Copy(positionOffsetsPtr, positionFloats, 0, positionFloats.Length);
            var orientationFloats = new float[transformCount * 4];
            Marshal.Copy(orientationsPtr, orientationFloats, 0, orientationFloats.Length);

            var positionOffsets = new Vector3[transformCount];
            var orientations = new Quaternion[transformCount];

            for (int transformIndex = 0; transformIndex < transformCount; ++transformIndex)
            {
                positionOffsets[transformIndex].Set(
                    positionFloats[transformIndex * 3 + 0], 
                    positionFloats[transformIndex * 3 + 1], 
                    positionFloats[transformIndex * 3 + 2]);
                orientations[transformIndex].Set(
                    orientationFloats[transformIndex * 4 + 0], 
                    orientationFloats[transformIndex * 4 + 1],
                    orientationFloats[transformIndex * 4 + 2],
                    orientationFloats[transformIndex * 4 + 3]);
            }

            indoorMapScene.AddInstancedRenderableInternal(meshID, renderableId, originEcef, indoorMapName, transformCount, positionOffsets, orientations, nativeRenderablePtr, indexOffset, ownsMesh);
        }

        private void AddInstancedRenderableInternal(string meshId, string renderableId, DoubleVector3 originECEF, string interiorName, int transformCount, Vector3[] positionOffsets, Quaternion[] orientations, IntPtr nativeRenderablePtr, int indexOffset, bool ownsMesh)
        {
            Mesh[] meshes;
            DoubleVector3 meshRecordOriginECEF;
            string materialName;

            if (m_meshUploader.TryGetUnityMeshesForID(meshId, out meshes, out meshRecordOriginECEF, out materialName))
            {
                if (ownsMesh)
                {
                    m_meshUploader.RemoveUploadedMesh(meshId);
                }

                var descriptor = new IndoorMaterialDescriptor(interiorName, materialName);                

                for (int transformIndex = 0; transformIndex < transformCount; ++transformIndex)
                {
                    string instanceId = renderableId + string.Format("_instance_{0}", transformIndex + indexOffset);
                    
                    var gameObjects = m_indoorMapStreamer.AddObjectsForMeshes(instanceId, meshes, originECEF, positionOffsets[transformIndex], orientations[transformIndex], materialName);

                    var material = m_indoorMapsApiInternal.IndoorMapMaterialFactory.CreateMaterialFromDescriptor(descriptor);

                    foreach (var gameObject in gameObjects)
                    {
                        gameObject.AddComponent<InstancedIndoorMapRenderable>().Init(transformIndex + indexOffset, nativeRenderablePtr, material, m_indoorMapsApiInternal);
                    }
                }
            }
            else
            {
                Debug.LogErrorFormat("ERROR: Could not get mesh for ID {0}.", meshId);
            }
        }

        public delegate void OnRenderStateUpdatedCallback(IntPtr indoorMapSceneHandle, [MarshalAs(UnmanagedType.LPStr)] string objectID);

        [MonoPInvokeCallback(typeof(OnRenderStateUpdatedCallback))]
        internal static void OnRenderStateUpdated(IntPtr indoorMapSceneHandle, [MarshalAs(UnmanagedType.LPStr)] string objectID)
        {
            var indoorMapScene = indoorMapSceneHandle.NativeHandleToObject<IndoorMapScene>();
            indoorMapScene.OnRenderStateUpdatedInternal(objectID);
        }

        [MonoPInvokeCallback(typeof(OnRenderStateUpdatedCallback))]
        internal static void OnRenderStateUpdatedForHighlightRenderable(IntPtr indoorMapSceneHandle, [MarshalAs(UnmanagedType.LPStr)] string objectID)
        {
            var indoorMapScene = indoorMapSceneHandle.NativeHandleToObject<IndoorMapScene>();
            indoorMapScene.OnRenderStateUpdatedForHighlightRenderableInternal(objectID);
        }

        [MonoPInvokeCallback(typeof(OnRenderStateUpdatedCallback))]
        internal static void OnRenderStateUpdatedForInstancedRenderable(IntPtr indoorMapSceneHandle, [MarshalAs(UnmanagedType.LPStr)] string objectID)
        {
            var indoorMapScene = indoorMapSceneHandle.NativeHandleToObject<IndoorMapScene>();
            indoorMapScene.OnRenderStateUpdatedForInstancedRenderableInternal(objectID);
        }

        private void OnRenderStateUpdatedInternal(string objectID)
        {
            var gameObject = m_indoorMapStreamer.GetObject(objectID);
            var indoorMapRenderables = gameObject.GetComponentsInChildren<IndoorMapRenderable>(true);

            foreach (var indoorMapRenderable in indoorMapRenderables)
            {
                indoorMapRenderable.OnRenderStateUpdated();
            }
        }

        private void OnRenderStateUpdatedForHighlightRenderableInternal(string objectID)
        {
            var gameObject = m_indoorMapStreamer.GetObject(objectID);
            var indoorMapRenderables = gameObject.GetComponentsInChildren<IndoorMapHighlightRenderable>(true);

            foreach (var indoorMapRenderable in indoorMapRenderables)
            {
                indoorMapRenderable.OnRenderStateUpdated();
            }
        }

        private void OnRenderStateUpdatedForInstancedRenderableInternal(string objectID)
        {
            var gameObject = m_indoorMapStreamer.GetObject(objectID);
            var indoorMapRenderables = gameObject.GetComponentsInChildren<InstancedIndoorMapRenderable>(true);

            foreach (var instancedIndoorMapRenderable in indoorMapRenderables)
            {
                instancedIndoorMapRenderable.OnRenderStateUpdated();
            }
        }
    }
}

