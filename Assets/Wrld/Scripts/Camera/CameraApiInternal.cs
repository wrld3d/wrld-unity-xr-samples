using AOT;
using System;
using System.Runtime.InteropServices;
using Wrld.Utilities;

namespace Wrld.MapCamera
{
    internal class CameraApiInternal
    {
        internal enum CameraEventType
        {
            Move,
            MoveStart,
            MoveEnd,
            Drag,
            DragStart,
            DragEnd,
            Pan,
            PanStart,
            PanEnd,
            Rotate,
            RotateStart,
            RotateEnd,
            Tilt,
            TiltStart,
            TiltEnd,
            Zoom,
            ZoomStart,
            ZoomEnd,
            TransitionStart,
            TransitionEnd
        };

        public event Action OnTransitionStartInternal;
        public event Action OnTransitionEndInternal;
        private IntPtr m_handleToSelf;

        public UnityEngine.Camera ControlledCamera { get; set; }
        public UnityEngine.Camera CustomRenderCamera { get; set; }

        internal CameraApiInternal()
        {
            m_handleToSelf = NativeInteropHelpers.AllocateNativeHandleForObject(this);
        }

        internal delegate void CameraEventCallback(IntPtr cameraApiInternalHandle, CameraEventType eventId);

        [MonoPInvokeCallback(typeof(CameraEventCallback))]
        public static void OnCameraEvent(IntPtr cameraApiInternalHandle, CameraEventType eventID)
        {
            var cameraApiInternal = cameraApiInternalHandle.NativeHandleToObject<CameraApiInternal>();

            if (eventID == CameraEventType.TransitionStart)
            {
                var startEvent = cameraApiInternal.OnTransitionStartInternal;

                if (startEvent != null)
                {
                    startEvent();
                }
            }
            else if (eventID == CameraEventType.TransitionEnd)
            {
                var endEvent = cameraApiInternal.OnTransitionEndInternal;

                if (endEvent != null)
                {
                    endEvent();
                }
            }
            // :TODO: handle other events
        }

        public IntPtr GetHandle()
        {
            return m_handleToSelf;
        }

        public void Destroy()
        {
            NativeInteropHelpers.FreeNativeHandle(m_handleToSelf);
        }

        public void SetCustomRenderCameraState(CameraState cameraState)
        {
            NativeCameraApi_SetCustomRenderCameraState(NativePluginRunner.API, ref cameraState);
        }

        public void ClearCustomRenderCamera()
        {
            NativeCameraApi_ClearCustomRenderCamera(NativePluginRunner.API);
        }


        public void MoveTo(CameraUpdate cameraUpdate)
        {
            var cameraUpdateInterop = cameraUpdate.ToCameraUpdateInterop();

            NativeCameraApi_MoveCamera(NativePluginRunner.API, ref cameraUpdateInterop);
        }

        public void AnimateTo(CameraUpdate cameraUpdate, CameraAnimationOptions cameraAnimationOptions)
        {
            var cameraUpdateInterop = cameraUpdate.ToCameraUpdateInterop();
            var cameraAnimationOptionsInterop = cameraAnimationOptions.ToCameraAnimationOptionsInterop();

            NativeCameraApi_AnimateCamera(NativePluginRunner.API, ref cameraUpdateInterop, ref cameraAnimationOptionsInterop);
        }

        public NativeCameraState GetNativeCameraState()
        {
            return NativeCameraApi_GetCurrentCameraState(NativePluginRunner.API);
        }

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeCameraApi_MoveCamera(
            IntPtr ptr,
            ref CameraUpdateInterop cameraUpdate);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeCameraApi_AnimateCamera(
            IntPtr ptr,
            ref CameraUpdateInterop cameraUpdate,
            ref CameraAnimationOptionsInterop cameraAnimationOptions);

        [DllImport(NativePluginRunner.DLL)]
        private static extern NativeCameraState NativeCameraApi_GetCurrentCameraState(IntPtr ptr);

        [DllImport(NativePluginRunner.DLL)]
        private static extern void NativeCameraApi_SetCustomRenderCameraState(IntPtr ptr, ref CameraState cameraState);

        [DllImport(NativePluginRunner.DLL)]
        private static extern void NativeCameraApi_ClearCustomRenderCamera(IntPtr ptr);
    }
}