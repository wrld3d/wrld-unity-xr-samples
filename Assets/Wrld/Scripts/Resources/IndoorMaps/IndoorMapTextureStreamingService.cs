using AOT;
using System;
using System.Runtime.InteropServices;
using Wrld.Materials;
using Wrld.Utilities;

namespace Wrld.Resources.IndoorMaps
{
    public class IndoorMapTextureStreamingService : IIndoorMapTextureStreamingService
    {
        private TextureLoadHandler m_textureLoadHandler;
        private IIndoorMapStreamedTextureObserver m_textureObserver;

        public IndoorMapTextureStreamingService(TextureLoadHandler textureLoadHandler, IIndoorMapStreamedTextureObserver textureObserver)
        {
            m_textureLoadHandler = textureLoadHandler;
            m_textureObserver = textureObserver;
        }

        public void RequestTextureForMaterial(IIndoorMapMaterial material, string interiorName, string textureKey, string texturePath, bool isCubemap)
        {
            var request = new StreamedTextureRequest(material, textureKey, m_textureObserver, this);
            var requestIntPtr = NativeInteropHelpers.AllocateNativeHandleForObject(request);

            NativeInteriorMaterials_IssueStreamedTextureRequest(NativePluginRunner.API, requestIntPtr, interiorName, material.MaterialInstance.name, texturePath, isCubemap);
        }

        internal delegate void OnTextureReceivedCallback(IntPtr requestHandle, [MarshalAs(UnmanagedType.LPStr)] string texturePath, uint textureId);

        [MonoPInvokeCallback(typeof(OnTextureReceivedCallback))]
        internal static void OnTextureReceived(IntPtr requestHandle, [MarshalAs(UnmanagedType.LPStr)] string texturePath, uint textureId)
        {
            var request = requestHandle.NativeHandleToObject<StreamedTextureRequest>();
            NativeInteropHelpers.FreeNativeHandle(requestHandle);

            request.Originator.m_textureLoadHandler.Update();
            var texture = request.Originator.m_textureLoadHandler.GetTexture(textureId);
            texture.name = texturePath;

            request.Observer.OnStreamedTextureReceived(request.Material, request.TextureKey, texture);
        }

        [DllImport(NativePluginRunner.DLL)]
        private static extern void NativeInteriorMaterials_IssueStreamedTextureRequest(IntPtr API, IntPtr requestHandle, [MarshalAs(UnmanagedType.LPStr)] string interiorName, [MarshalAs(UnmanagedType.LPStr)] string materialName, [MarshalAs(UnmanagedType.LPStr)] string texturePath, [MarshalAs(UnmanagedType.I1)] bool isCubemap);
    }
}