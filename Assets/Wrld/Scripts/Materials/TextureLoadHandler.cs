using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using Wrld.Concurrency;
using Wrld.Utilities;

namespace Wrld.Materials
{
    public class TextureLoadHandler
    {
        const string DLL = NativePluginRunner.DLL;
        class TextureBuffer
        {
            public byte[] allocatedBuffer;
            public int width;
            public int height;
            public TextureFormat format;
            public bool mipMaps;
            public uint id;
            private List<GCHandle> gcHandles;

            public TextureBuffer(int _sizeInBytes, int _width, int _height, int _format, bool _mipmaps, uint _id)
            {
                allocatedBuffer = new byte[_sizeInBytes];
                gcHandles = new List<GCHandle>();
                width = _width;
                height = _height;
                format = (TextureFormat)_format;
                mipMaps = _mipmaps;
                id = _id;
            }
            public IntPtr PinAndTrackHandle(object member)
            {
                var handle = GCHandle.Alloc(member, GCHandleType.Pinned);
                gcHandles.Add(handle);
                return handle.AddrOfPinnedObject();
            }
            public void FreeTrackedHandles()
            {
                foreach (var handle in gcHandles)
                {
                    handle.Free();
                }

                gcHandles.Clear();
            }

            // Create a MarshalledMesh whose IntPtrs point at the (pinned) data arrays in this mesh
            public IntPtr CreatePointerToMarshalledTextureBuffer()
            {
                var marshalled = new MarshalledTextureBuffer();

                marshalled.format = (int)format;
                marshalled.height = height;
                marshalled.width = width;
                marshalled.mipMaps = mipMaps;
                marshalled.sizeInBytes = allocatedBuffer.Length;
                marshalled.data = PinAndTrackHandle(allocatedBuffer);

                GCHandle handle = GCHandle.Alloc(this);
                marshalled.textureBufferHandle = GCHandle.ToIntPtr(handle);
                gcHandles.Add(handle);

                return PinAndTrackHandle(marshalled);
            }
        };

        [StructLayout(LayoutKind.Sequential)]
        struct MarshalledTextureBuffer
        {
            public IntPtr data;
            public int sizeInBytes;
            public int width;
            public int height;
            public int format;
            private byte _mipMaps;
            public bool mipMaps
            {
                get { return _mipMaps != 0; }
                set { _mipMaps = (byte)(value ? 1 : 0); }
            }
            public IntPtr textureBufferHandle;
        };

        internal delegate IntPtr AllocateTextureBufferCallback(IntPtr textureServiceHandle, int size, int width, int height, int format, int hasMipMaps);
        internal delegate uint BeginUploadTextureBufferCallback(IntPtr textureServiceHandle, IntPtr buffer);
        internal delegate void ReleaseTextureCallback(IntPtr textureServiceHandle, uint texture);
        internal delegate uint CreateCubemapFromFacesCallback(IntPtr textureServiceHandle, [MarshalAs(UnmanagedType.LPArray, SizeConst = 6)] uint[] faceIDs);

        Dictionary<uint, Texture> m_builtTextures = new Dictionary<uint, Texture>();
        ConcurrentQueue<TextureBuffer> m_processQueue = new ConcurrentQueue<TextureBuffer>();
        IdGenerator m_idGenerator = new IdGenerator();
        IntPtr m_handleToSelf;

        public TextureLoadHandler()
        {
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

        // Update is called once per frame
        public void Update()
        {
            while (CreateTexture());
        }

        [MonoPInvokeCallback(typeof(AllocateTextureBufferCallback))]
        internal static IntPtr AllocateTextureBuffer(IntPtr textureServiceHandle, int size, int width, int height, int format, int hasMipMaps)
        {
            var textureService = textureServiceHandle.NativeHandleToObject<TextureLoadHandler>();
            return textureService.AllocateTextureBufferInternal(size, width, height, format, hasMipMaps != 0);
        }

        [MonoPInvokeCallback(typeof(BeginUploadTextureBufferCallback))]
        internal static uint BeginUploadTextureBuffer(IntPtr textureServiceHandle, IntPtr bufferPtr)
        {
            var textureService = textureServiceHandle.NativeHandleToObject<TextureLoadHandler>();
            return textureService.BeginUploadTextureBufferInternal(bufferPtr);
        }

        [MonoPInvokeCallback(typeof(ReleaseTextureCallback))]
        internal static void ReleaseTexture(IntPtr textureServiceHandle, uint id)
        {
            var textureService = textureServiceHandle.NativeHandleToObject<TextureLoadHandler>();
            textureService.ReleaseTextureInternal(id);
        }

        [MonoPInvokeCallback(typeof(CreateCubemapFromFacesCallback))]
        internal static uint CreateCubemapFromFaces(IntPtr textureServiceHandle, [MarshalAs(UnmanagedType.LPArray, SizeConst = 6)] uint[] faceIDs)
        {
            var textureService = textureServiceHandle.NativeHandleToObject<TextureLoadHandler>();
            return textureService.CreateCubemapFromFacesInternal(faceIDs);
        }

        private uint CreateCubemapFromFacesInternal(uint[] faceIDs)
        {
            Update();
            uint resultID = m_idGenerator.GetNextID();

            var faces = new[] {
                CubemapFace.PositiveX, CubemapFace.NegativeX,
                CubemapFace.PositiveY, CubemapFace.NegativeY,
                CubemapFace.PositiveZ, CubemapFace.NegativeZ };
            Cubemap cubemap = null;
            
            for (int faceIndex = 0; faceIndex < 6; ++faceIndex)
            {
                var texture = (Texture2D)GetTexture(faceIDs[faceIndex]);

                if (cubemap == null)
                {
                    cubemap = new Cubemap(texture.width, texture.format, false);
                }

                cubemap.SetPixels(texture.GetPixels(), faces[faceIndex]);
            }

            cubemap.Apply(false, true);
            m_builtTextures[resultID] = cubemap;

            return resultID;
        }

        private bool CreateTexture()
        {
            TextureBuffer buffer;

            if (!m_processQueue.TryDequeue(out buffer))
            {
                return false;
            }

            Debug.Assert(!m_builtTextures.ContainsKey(buffer.id));

            var texture = new Texture2D(buffer.width, buffer.height, buffer.format, buffer.mipMaps);
            texture.wrapMode = buffer.format == TextureFormat.RGB565 ? TextureWrapMode.Clamp : TextureWrapMode.Repeat;
            texture.LoadRawTextureData(buffer.allocatedBuffer);
            texture.Apply(updateMipmaps: true, makeNoLongerReadable: false);

            m_builtTextures.Add(buffer.id, texture);

            return true;
        }

        public Texture GetTexture(uint id)
        {
            return m_builtTextures.ContainsKey(id) ? m_builtTextures[id] : null;
        }
        private IntPtr AllocateTextureBufferInternal(int size, int width, int height, int format, bool hasMipMaps)
        {
            // :TODO: do we need to know if linear?
            TextureBuffer textureBuffer = new TextureBuffer(size, width, height, format, hasMipMaps, m_idGenerator.GetNextID());
            return textureBuffer.CreatePointerToMarshalledTextureBuffer();
        }

        private uint BeginUploadTextureBufferInternal(IntPtr bufferPtr)
        {
            MarshalledTextureBuffer marshalled = (MarshalledTextureBuffer)Marshal.PtrToStructure(bufferPtr, typeof(MarshalledTextureBuffer));
            GCHandle textureBufferHandle = GCHandle.FromIntPtr(marshalled.textureBufferHandle);
            var result = textureBufferHandle.Target as TextureBuffer;
            result.FreeTrackedHandles();

            // this is one of the points where we could create an identifier that we could use to delete (other than the string name)
            m_processQueue.Enqueue(result);

            return result.id;
        }

        private void ReleaseTextureInternal(uint id)
        {
            Texture value;

            if (m_builtTextures.TryGetValue(id, out value))
            {
                m_idGenerator.FreeID(id);
                m_builtTextures.Remove(id);
                GameObject.DestroyImmediate(value);
            }
        }
    }
}