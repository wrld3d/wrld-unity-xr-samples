using System;
using System.Runtime.InteropServices;


namespace Wrld.Space.EnvironmentFlattening
{
    internal class EnvironmentFlatteningApiInternal 
    {
        public void SetIsFlattened(bool isFlattened)
        {
            NativeEnvironmentFlattening_SetIsFlattened(NativePluginRunner.API, isFlattened);
        }

        public bool IsFlattened()
        {
            return NativeEnvironmentFlattening_IsFlattened(NativePluginRunner.API);
        }

        public float GetCurrentScale()
        {
            return NativeEnvironmentFlattening_GetCurrentScale(NativePluginRunner.API);
        }

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeEnvironmentFlattening_SetIsFlattened(IntPtr ptr, [MarshalAs(UnmanagedType.I1)] bool isFlattened);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool NativeEnvironmentFlattening_IsFlattened(IntPtr ptr);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern float NativeEnvironmentFlattening_GetCurrentScale(IntPtr ptr);
    }
}