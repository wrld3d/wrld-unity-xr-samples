using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore; // Please import Google ARCore plugin if you are seeing a compiler error here.

namespace WRLD.ARCore
{
	public class WRLDARCorePositioner : MonoBehaviour 
	{
		public Transform wrldMapMask;

		// Please import Google ARCore plugin if you are seeing a compiler error here.
		private TrackedPlane m_AttachedPlane;
		private bool m_isActive = true;

		// Please import Google ARCore plugin if you are seeing a compiler error here.
		public TrackedPlane CurrentTrackedPlane
		{
			set
			{
				m_AttachedPlane = value;
				if(m_AttachedPlane!=null)
				{
					// Please import Google ARCore plugin if you are seeing a compiler error here.
					wrldMapMask.localScale = new Vector3(m_AttachedPlane.ExtentX, 1f, m_AttachedPlane.ExtentZ);

					// Please import Google ARCore plugin if you are seeing a compiler error here.
					transform.position = m_AttachedPlane.CenterPose.position;
				}
				else
				{
					wrldMapMask.transform.localScale = Vector3.zero;
				}	
			}
		}

		public void Update()
		{
			if (m_AttachedPlane == null) 
			{
				return;
			}

			// Please import Google ARCore plugin if you are seeing a compiler error here.
			while (m_AttachedPlane.SubsumedBy != null)
			{
				// Please import Google ARCore plugin if you are seeing a compiler error here.
				m_AttachedPlane = m_AttachedPlane.SubsumedBy;
			}

			// Please import Google ARCore plugin if you are seeing a compiler error here.
			if (m_AttachedPlane.TrackingState != TrackingState.Tracking)
			{
				wrldMapMask.localScale = Vector3.zero;
			}

			// Please import Google ARCore plugin if you are seeing a compiler error here.
			Vector3 difference = transform.position - m_AttachedPlane.CenterPose.position;
			wrldMapMask.transform.localPosition = new Vector3(difference.x, wrldMapMask.localPosition.y, difference.z);

			// Please import Google ARCore plugin if you are seeing a compiler error here.
			wrldMapMask.transform.localScale = new Vector3(m_AttachedPlane.ExtentX, 1f, m_AttachedPlane.ExtentZ);

			// Please import Google ARCore plugin if you are seeing a compiler error here.
			wrldMapMask.transform.rotation = m_AttachedPlane.CenterPose.rotation;

			if (!m_isActive) 
			{
				wrldMapMask.transform.localScale = Vector3.zero;
			}

			if ((Input.touches.Length > 0 && Input.touches[0].phase == TouchPhase.Began))
			{
				m_isActive = !m_isActive;
			}
		}

	}
}