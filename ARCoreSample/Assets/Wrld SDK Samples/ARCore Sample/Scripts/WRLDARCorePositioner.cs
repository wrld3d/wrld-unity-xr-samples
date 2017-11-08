using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore; // Please import Google ARCore plugin if you are seeing error here.

namespace WRLD.ARCore
{
	public class WRLDARCorePositioner : MonoBehaviour 
	{
		public Transform wrldMapMask;

		// Please import Google ARCore plugin if you are seeing error here.
		private TrackedPlane m_AttachedPlane;
		private bool m_isActive = true;

		// Please import Google ARCore plugin if you are seeing error here.
		public TrackedPlane CurrentTrackedPlane
		{
			set
			{
				m_AttachedPlane = value;
				if(m_AttachedPlane!=null)
				{
					// Please import Google ARCore plugin if you are seeing error here.
					wrldMapMask.transform.localScale = new Vector3 (m_AttachedPlane.Bounds.x, 1f,  m_AttachedPlane.Bounds.y);

					// Please import Google ARCore plugin if you are seeing error here.
					transform.position = m_AttachedPlane.Position;
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

			// Please import Google ARCore plugin if you are seeing error here.
			while (m_AttachedPlane.SubsumedBy != null)
			{
				// Please import Google ARCore plugin if you are seeing error here.
				m_AttachedPlane = m_AttachedPlane.SubsumedBy;
			}

			// Please import Google ARCore plugin if you are seeing error here.
			if (!m_AttachedPlane.IsValid)
			{
				wrldMapMask.localScale = Vector3.zero;
			}

			// Please import Google ARCore plugin if you are seeing error here.
			Vector3 difference = transform.position - m_AttachedPlane.Position;
			wrldMapMask.transform.localPosition = new Vector3(difference.x, wrldMapMask.transform.localPosition.y, difference.z);

			// Please import Google ARCore plugin if you are seeing error here.
			wrldMapMask.transform.localScale = new Vector3 (m_AttachedPlane.Bounds.x, 1f,  m_AttachedPlane.Bounds.y);

			// Please import Google ARCore plugin if you are seeing error here.
			wrldMapMask.transform.rotation = m_AttachedPlane.Rotation;

			if (!m_isActive) 
			{
				wrldMapMask.transform.localScale = Vector3.zero;
			}

			if ((Input.touches.Length > 0 && Input.touches [0].phase == TouchPhase.Began)) 
			{
				m_isActive = !m_isActive;
			}
		}

	}
}