// Copyright Wrld Ltd (2012-2014), All Rights Reserved
using UnityEngine;

namespace Wrld.MapInput.Mouse
{
    public class MouseZoomGesture
    {
        private IUnityInputHandler m_handler;
        float m_sensitivity;
        float m_maxZoomPerSecond;
        float m_zoomAccumulator;

        private bool UpdatePinching(bool pinching, MouseInputEvent touchEvent, out float pinchScale, int numTouches, bool pointerUp)
        {
            pinchScale = 0.0f;
            return false;
        }

        public MouseZoomGesture(IUnityInputHandler handler)
        {
            m_handler = handler;
            // m_sensitivity is a unitless scaling value to convert to same units as used by pinch-zoom on touch devices
            // (which is distance pinched in normalized screen coordinates).
            m_sensitivity = 0.008f;
            m_maxZoomPerSecond = 0.5f;
        }

        public void PointerMove(MouseInputEvent mouseEvent)
        {
            // mouseEvent.z values provided via Unity seem to be multiples of +/- 1.0, not necessarily raised very frame
            // no units mentioned here: https://docs.unity3d.com/ScriptReference/Input-mouseScrollDelta.html
            float zoomDelta = -mouseEvent.z * m_sensitivity;
            m_zoomAccumulator += zoomDelta;
        }

        public void Update(float dt)
        {
            if (m_zoomAccumulator == 0.0f)
                return;

            TruncateZoomAccumulator();

            // mouse-wheel / trackpad smoothing - apply speed-limit, consume the deltas that
            // are buffered in m_zoomAccumulator over potentially several frames.
            float maxZoomDelta = dt * m_maxZoomPerSecond;
            float clampedZoomDelta = Mathf.Clamp(m_zoomAccumulator, -maxZoomDelta, maxZoomDelta);

            m_zoomAccumulator -= clampedZoomDelta;

            AppInterface.ZoomData zoomData;
            zoomData.distance = clampedZoomDelta;

            m_handler.Event_Zoom(zoomData);
        }

        private void TruncateZoomAccumulator()
        {
            // this is to avoid m_zoomAccumulator growing beyond what can be consumed within maxBufferTime seconds
            float maxBufferTime = 0.5f;
            float maxMagnitude = m_maxZoomPerSecond * maxBufferTime;
            m_zoomAccumulator = Mathf.Clamp(m_zoomAccumulator, -maxMagnitude, maxMagnitude);
        }
    };
}
