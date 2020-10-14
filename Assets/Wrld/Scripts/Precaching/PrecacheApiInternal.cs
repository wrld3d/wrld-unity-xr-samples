using AOT;
using System;
using System.Runtime.InteropServices;
using Wrld.Interop;
using Wrld.Space;
using Wrld.Utilities;

namespace Wrld.Precaching
{
    internal class PrecacheApiInternal
    {
        internal delegate void PrecacheOperationCompletedHandler(IntPtr internalApiHandle, int operationId, [MarshalAs(UnmanagedType.I1)] bool succeeded);

        internal event Action<int, PrecacheOperationResult> OnPrecacheOperationCompleted;

        internal PrecacheApiInternal()
        {
            m_handleToSelf = NativeInteropHelpers.AllocateNativeHandleForObject(this);
        }

        internal int BeginPrecacheOperation(LatLong center, double radius)
        {
            var latLongInterop = LatLongInterop.FromLatLong(center);
            return NativePrecacheApi_BeginPrecacheOperation(NativePluginRunner.API, ref latLongInterop, radius);
        }

        internal void CancelPrecacheOperation(int operationId)
        {
            NativePrecacheApi_CancelPrecacheOperation(NativePluginRunner.API, operationId);
        }

        internal double GetMaximumPrecacheRadius()
        {
            return NativePrecacheApi_GetMaximumPrecacheRadius();
        }

        internal IntPtr GetHandle()
        {
            return m_handleToSelf;
        }

        internal void Destroy()
        {
            NativeInteropHelpers.FreeNativeHandle(m_handleToSelf);
        }

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern int NativePrecacheApi_BeginPrecacheOperation(IntPtr ptr, ref LatLongInterop center, double radius);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void NativePrecacheApi_CancelPrecacheOperation(IntPtr ptr, int precacheOperationId);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern double NativePrecacheApi_GetMaximumPrecacheRadius();

        [MonoPInvokeCallback(typeof(PrecacheOperationCompletedHandler))]
        internal static void OnPrecacheOperationCompletedCallback(IntPtr internalApiHandle, int operationId, [MarshalAs(UnmanagedType.I1)] bool succeeded)
        {
            var internalApi = internalApiHandle.NativeHandleToObject<PrecacheApiInternal>();
            internalApi.OnPrecacheOperationCompleted(operationId, new PrecacheOperationResult(succeeded));
        }

        IntPtr m_handleToSelf;
    }
}