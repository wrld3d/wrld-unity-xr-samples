using Wrld.MapCamera;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Wrld
{
    public class StreamingUpdater
    {
        private int m_screenWidth = 0;
        private int m_screenHeight = 0;

        [DllImport(NativePluginRunner.DLL)]
        private static extern void SetCustomStreamingCameraState(IntPtr ptr, ref CameraState state);

        [DllImport(NativePluginRunner.DLL)]
        private static extern void ClearCustomStreamingCamera(IntPtr ptr);

        [DllImport(NativePluginRunner.DLL)]
        private static extern void NotifyScreenSizeChanged(IntPtr ptr, float screenWidth, float screenHeight);

        public void Update(CameraState cameraState)
        {
            Debug.Assert(((Vector3)cameraState.ViewMatrix.GetColumn(3)).sqrMagnitude < 0.000001f,
                "The camera is expected to have zero translation in its view matrix - translation should be put in LocationECEF instead");

            if (InterestPointProvider.ClampInterestPointToValidRangeIfRequired(ref cameraState.InterestPointEcef))
            {
                Debug.LogWarning("Interest point had too high an altitude, clamping to valid range.");
            }

            UpdateScreenSize();

            SetCustomStreamingCameraState(NativePluginRunner.API, ref cameraState);
        }

        public void UpdateForBuiltInCamera()
        {
            UpdateScreenSize();
            ClearCustomStreamingCamera(NativePluginRunner.API);
        }

        private void UpdateScreenSize()
        {
            if (Screen.width != m_screenWidth || Screen.height != m_screenHeight)
            {
                m_screenWidth = Screen.width;
                m_screenHeight = Screen.height;
                NotifyScreenSizeChanged(NativePluginRunner.API, Screen.width, Screen.height);
            }
        }
    }
}

