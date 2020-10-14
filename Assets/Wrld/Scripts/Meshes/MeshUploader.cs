using AOT;
using Wrld.Common.Maths;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Wrld.Meshes
{
    public class MeshUploader
    {
        class UnpackedMesh
        {
            public Vector3[] vertices;

            public Vector2[] uvs;

            public Vector2[] uv2s;

            public Vector3[] normals;

            public Color32[] colors;

            public int[] indices;

            public DoubleVector3[] originECEF;

            public string materialName;

            public string name;

            private List<GCHandle> gcHandles;
            public UnpackedMesh(int vertexCount, bool hasUVs, bool hasUV2s, bool hasNormals, bool hasColors, int indexCount, IntPtr _name, IntPtr material)
            {
                gcHandles = new List<GCHandle>();
                vertices = new Vector3[vertexCount];

                uvs = hasUVs ? new Vector2[vertexCount] : null;
                uv2s = hasUV2s ? new Vector2[vertexCount] : null;
                normals = hasNormals ? new Vector3[vertexCount] : null;
                colors = hasColors ? new Color32[vertexCount] : null;

                indices = new int[indexCount];
                originECEF = new DoubleVector3[1];
                name = Marshal.PtrToStringAnsi(_name);
                materialName = Marshal.PtrToStringAnsi(material);
            }
            private IntPtr PinAndTrackHandle(object member)
            {
                var handle = GCHandle.Alloc(member, GCHandleType.Pinned);
                gcHandles.Add(handle);
                return handle.AddrOfPinnedObject();
            }

            // Create a MarshalledMesh whose IntPtrs point at the (pinned) data arrays in this mesh
            public IntPtr CreatePointerToMarshalledMesh()
            {
                var marshalledMesh = new MarshalledMesh();

                marshalledMesh.vertices = PinAndTrackHandle(vertices);

                if (uvs != null)
                {
                    marshalledMesh.uvs = PinAndTrackHandle(uvs);
                }

                if (uv2s != null)
                {
                    marshalledMesh.uv2s = PinAndTrackHandle(uv2s);
                }

                if (normals != null)
                {
                    marshalledMesh.normals = PinAndTrackHandle(normals);
                }

                if (colors != null)
                {
                    marshalledMesh.colors = PinAndTrackHandle(colors);
                }

                marshalledMesh.indices = PinAndTrackHandle(indices);
                marshalledMesh.originEcef = PinAndTrackHandle(originECEF);

                GCHandle handle = GCHandle.Alloc(this);
                marshalledMesh.unpackedMeshHandle = GCHandle.ToIntPtr(handle);
                gcHandles.Add(handle);

                return PinAndTrackHandle(marshalledMesh);
            }

            public void FreeTrackedHandles()
            {
                foreach (var handle in gcHandles)
                {
                    handle.Free();
                }

                gcHandles.Clear();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MarshalledMesh
        {
            public IntPtr vertices;
            public IntPtr uvs;
            public IntPtr uv2s;
            public IntPtr normals;
            public IntPtr colors;
            public IntPtr indices;
            public IntPtr originEcef;
            public IntPtr name;
            public IntPtr unpackedMeshHandle;
        }

        public delegate IntPtr AllocateUnpackedMeshCallback(int vertexCount, [MarshalAs(UnmanagedType.I1)] bool hasUvs, [MarshalAs(UnmanagedType.I1)] bool hasUV2s, [MarshalAs(UnmanagedType.I1)] bool hasNormals, [MarshalAs(UnmanagedType.I1)] bool hasColours, int indexCount, IntPtr meshName, IntPtr materialName);
        public delegate void UploadUnpackedMeshCallback(IntPtr meshBuffer);
        public delegate void RemoveUnpackedMeshCallback([MarshalAs(UnmanagedType.LPStr)]string id);

        static PreparedMeshRepository m_preparedMeshes = new PreparedMeshRepository();

        public MeshUploader()
        {
        }

        [MonoPInvokeCallback(typeof(AllocateUnpackedMeshCallback))]
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        public static IntPtr AllocateUnpackedMesh(int vertexCount, [MarshalAs(UnmanagedType.I1)] bool hasUvs, [MarshalAs(UnmanagedType.I1)] bool hasUV2s, [MarshalAs(UnmanagedType.I1)] bool hasNormals, [MarshalAs(UnmanagedType.I1)] bool hasColors, int indexCount, IntPtr name, IntPtr material)
        {
            var unpackedMesh = new UnpackedMesh(vertexCount, hasUvs, hasUV2s, hasNormals, hasColors, indexCount, name, material);

            return unpackedMesh.CreatePointerToMarshalledMesh();
        }

        [MonoPInvokeCallback(typeof(UploadUnpackedMeshCallback))]
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        public static void UploadUnpackedMesh(IntPtr meshPtr)
        {
            MarshalledMesh marshalled = (MarshalledMesh)Marshal.PtrToStructure(meshPtr, typeof(MarshalledMesh));
            GCHandle unpackedMeshHandle = GCHandle.FromIntPtr(marshalled.unpackedMeshHandle);
            var result = unpackedMeshHandle.Target as UnpackedMesh;
            result.FreeTrackedHandles();

            // this is one of the points where we could create an identifier that we could use to delete the mesh (other than the string name)
            m_preparedMeshes.AddPreparedMeshRecord(CreatePreparedMeshRecordFromUnpackedMesh(result));
        }

        [MonoPInvokeCallback(typeof(RemoveUnpackedMeshCallback))]
        public static void RemoveUnpackedMesh([MarshalAs(UnmanagedType.LPStr)]string meshId)
        {
            m_preparedMeshes.TryRemovePreparedMesh(meshId);
        }

        private static PreparedMeshRecord CreatePreparedMeshRecordFromUnpackedMesh(UnpackedMesh meshData)
        {
            var meshes = MeshBuilder.CreatePreparedMeshes(meshData.vertices, meshData.uvs, meshData.uv2s, meshData.normals, meshData.colors, meshData.indices, meshData.name, meshData.materialName, meshData.originECEF[0], 65535);

            return new PreparedMeshRecord { MaterialName = meshData.materialName, Meshes = meshes, OriginECEF = meshData.originECEF[0] };
        }

        public bool TryGetUnityMeshesForID(string id, out Mesh[] meshes, out DoubleVector3 originECEF, out string materialName)
        {
            return m_preparedMeshes.TryGetUnityMeshesForID(id, out meshes, out originECEF, out materialName);
        }

        public bool RemoveUploadedMesh(string meshId)
        {
            return m_preparedMeshes.TryRemovePreparedMesh(meshId);
        }
    }
}