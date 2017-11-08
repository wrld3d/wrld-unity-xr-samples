using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using GoogleARCore; // Please import Google ARCore plugin if you are seeing error here.

namespace WRLD.ARCore
{
	public class WRLDARCoreManager : MonoBehaviour
	{
		public Transform wrldMap;
		public Transform wrldMapMask;

		// Please import Google ARCore plugin if you are seeing error here.
		private TrackedPlane m_trackedPlane;

		// Please import Google ARCore plugin if you are seeing error here.
		private List<TrackedPlane> m_allPlanes = new List<TrackedPlane>();

		private WRLDARCorePositioner m_wrldMapARCorePositioner;

		public void Start()
		{
			m_wrldMapARCorePositioner = wrldMap.GetComponent<WRLDARCorePositioner> ();
		}

		public void Update ()
		{

			// Please import Google ARCore plugin if you are seeing error here.
			if (Frame.TrackingState != FrameTrackingState.Tracking)
			{
				const int LOST_TRACKING_SLEEP_TIMEOUT = 15;
				Screen.sleepTimeout = LOST_TRACKING_SLEEP_TIMEOUT;
				return;
			}
			Screen.sleepTimeout = SleepTimeout.NeverSleep;

			// Please import Google ARCore plugin if you are seeing error here.
			Frame.GetAllPlanes(ref m_allPlanes);

			// Please import Google ARCore plugin if you are seeing error here.
			TrackedPlane firstValidPlane = null;
			for (int i = 0; i < m_allPlanes.Count; i++)
			{
				// Please import Google ARCore plugin if you are seeing error here.
				if (m_allPlanes[i].IsValid)
				{
					firstValidPlane = m_allPlanes [0];
					break;
				}
			}

			if (firstValidPlane==null) 
			{
				if (m_trackedPlane != null) 
				{
					m_trackedPlane = null;
					m_wrldMapARCorePositioner.CurrentTrackedPlane = null;
				}
			}
			else if(m_trackedPlane==null)
			{
				m_trackedPlane = firstValidPlane;
				m_wrldMapARCorePositioner.CurrentTrackedPlane = m_trackedPlane;
			}

		}
	}
}
