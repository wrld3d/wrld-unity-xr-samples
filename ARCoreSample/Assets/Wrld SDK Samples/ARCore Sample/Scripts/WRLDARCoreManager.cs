using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using GoogleARCore; // Please import Google ARCore plugin if you are seeing a compiler error here.

namespace WRLD.ARCore
{
	public class WRLDARCoreManager : MonoBehaviour
	{
		public Transform wrldMap;
		public Transform wrldMapMask;

		// Please import Google ARCore plugin if you are seeing a compiler error here.
		private TrackedPlane m_trackedPlane;

		// Please import Google ARCore plugin if you are seeing a compiler error here.
		private List<TrackedPlane> m_allPlanes = new List<TrackedPlane>();

		private WRLDARCorePositioner m_wrldMapARCorePositioner;

		public void Start()
		{
			m_wrldMapARCorePositioner = wrldMap.GetComponent<WRLDARCorePositioner> ();
		}

		public void Update ()
		{

			// Please import Google ARCore plugin if you are seeing a compiler error here.
			if (Session.Status != SessionStatus.Tracking)
			{
				const int LOST_TRACKING_SLEEP_TIMEOUT = 15;
				Screen.sleepTimeout = LOST_TRACKING_SLEEP_TIMEOUT;
				return;
			}

			Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // Please import Google ARCore plugin if you are seeing a compiler error here.
            Session.GetTrackables(m_allPlanes);

			// Please import Google ARCore plugin if you are seeing a compiler error here.
			TrackedPlane firstValidPlane = null;
			for (int i = 0; i < m_allPlanes.Count; i++)
			{
				// Please import Google ARCore plugin if you are seeing a compiler error here.
				if (m_allPlanes[i].TrackingState == TrackingState.Tracking)
				{
					firstValidPlane = m_allPlanes [i];
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
