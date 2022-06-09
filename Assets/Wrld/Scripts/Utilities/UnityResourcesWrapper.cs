#if UNITY_WEBGL
using AOT;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Wrld
{
    public class UnityResourcesWrapper
    {
        public delegate bool UnityResourcesWrapperLoadResourceCallback([MarshalAs(UnmanagedType.LPStr)] string resource, IntPtr bufferPointerPointer, IntPtr sizePointer);

        [MonoPInvokeCallback(typeof(UnityResourcesWrapperLoadResourceCallback))]
        public static bool LoadResource([MarshalAs(UnmanagedType.LPStr)] string resource, IntPtr bufferPointerPointer, IntPtr sizePointer)
        {
            var resourceAsset = UnityEngine.Resources.Load<TextAsset>(resource);
            if (!resourceAsset)
                return false;
            var resourceBytes = resourceAsset.bytes;
            Marshal.WriteInt32(sizePointer, resourceBytes.Length);
            IntPtr buffer = Marshal.AllocHGlobal(resourceBytes.Length);
            Marshal.WriteIntPtr(bufferPointerPointer, buffer);
            Marshal.Copy(resourceBytes, 0, buffer, resourceBytes.Length);
            return true;
        }

        public delegate void UnityResourcesWrapperFreeResourceBufferCallback(IntPtr buffer);

        [MonoPInvokeCallback(typeof(UnityResourcesWrapperFreeResourceBufferCallback))]
        public static void FreeResourceBuffer(IntPtr buffer)
        {
            Marshal.FreeHGlobal(buffer);
        }

        public delegate bool UnityResourcesWrapperResourceExistsCallback([MarshalAs(UnmanagedType.LPStr)] string resource);

        [MonoPInvokeCallback(typeof(UnityResourcesWrapperResourceExistsCallback))]
        public static bool ResourceExists([MarshalAs(UnmanagedType.LPStr)] string resource)
        {
            return UnityEngine.Resources.Load<TextAsset>(resource);
        }
    }
}
#endif
