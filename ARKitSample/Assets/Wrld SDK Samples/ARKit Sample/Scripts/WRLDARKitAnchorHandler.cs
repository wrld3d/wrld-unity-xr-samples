using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.iOS; // Please import Unity ARKit Plugin if you are seeing a compiler error here.

public class WRLDARKitAnchorHandler : MonoBehaviour 
{
    // Please import Unity ARKit Plugin if you are seeing a compiler error here.
    private UnityARAnchorManager m_unityARAnchorManager;

    public Transform wrldMapParent;

    public Transform wrldMapMask;

    private WRLDARStreamingCameraHandler m_arStreamingController;

    // Please import Unity ARKit Plugin if you are seeing a compiler error here.
    private Dictionary<string, ARPlaneAnchor> m_planeAnchorMap;
    private ARPlaneAnchor m_currentAnchor;
    private bool m_hasFoundAnchor = false;

    void Start()
    {
        m_arStreamingController = GameObject.FindObjectOfType<WRLDARStreamingCameraHandler> ();

        // Please import Unity ARKit Plugin if you are seeing a compiler error here.
        m_planeAnchorMap = new Dictionary<string, ARPlaneAnchor> ();

        // Please import Unity ARKit Plugin if you are seeing a compiler error here.
        UnityARSessionNativeInterface.ARAnchorAddedEvent += AddAnchor;
        UnityARSessionNativeInterface.ARAnchorUpdatedEvent += UpdateAnchor;
        UnityARSessionNativeInterface.ARAnchorRemovedEvent += RemoveAnchor;
    }

    // Please import Unity ARKit Plugin if you are seeing a compiler error here.
    public void AddAnchor(ARPlaneAnchor arPlaneAnchor)
    {
        m_planeAnchorMap.Add (arPlaneAnchor.identifier, arPlaneAnchor);

        if (m_hasFoundAnchor == false) 
        {
            m_hasFoundAnchor = true;
            m_currentAnchor = arPlaneAnchor;
            UpdateMapPositionWithAnchor (m_currentAnchor);
            UpdateMapMaskWithAnchor (m_currentAnchor);
        }
    }

    // Please import Unity ARKit Plugin if you are seeing a compiler error here.
    public void RemoveAnchor(ARPlaneAnchor arPlaneAnchor)
    {
        if (m_planeAnchorMap.ContainsKey (arPlaneAnchor.identifier)) 
        {
            m_planeAnchorMap.Remove (arPlaneAnchor.identifier);
        }

        if (m_hasFoundAnchor && arPlaneAnchor.identifier == m_currentAnchor.identifier) 
        {
            if (m_planeAnchorMap.Count > 0) 
            {
                m_currentAnchor = m_planeAnchorMap.Values.First ();
                UpdateMapMaskWithAnchor (m_currentAnchor);
            }
            else
            {
                m_hasFoundAnchor = false;
            }
        }
    }

    // Please import Unity ARKit Plugin if you are seeing a compiler error here.
    public void UpdateAnchor(ARPlaneAnchor arPlaneAnchor)
    {
        if (m_planeAnchorMap.ContainsKey (arPlaneAnchor.identifier)) 
        {
            m_planeAnchorMap [arPlaneAnchor.identifier] = arPlaneAnchor;
        }

        if (m_hasFoundAnchor && arPlaneAnchor.identifier == m_currentAnchor.identifier) 
        {
            m_currentAnchor = arPlaneAnchor;
            UpdateMapMaskWithAnchor (m_currentAnchor);
        }
    }

    // Please import Unity ARKit Plugin if you are seeing a compiler error here.
    void UpdateMapPositionWithAnchor(ARPlaneAnchor arPlaneAnchor)
    {
        //Setting the position of our map to match the position of anchor
        wrldMapParent.position = UnityARMatrixOps.GetPosition (arPlaneAnchor.transform);
    }

    // Please import Unity ARKit Plugin if you are seeing a compiler error here.
    void UpdateMapMaskWithAnchor(ARPlaneAnchor arPlaneAnchor)
    {
        //Setting the position of our map to match the position of anchor
        wrldMapMask.parent.position = UnityARMatrixOps.GetPosition (arPlaneAnchor.transform);

        //Updating our mask according to the ARKit anchor
        wrldMapMask.parent.rotation = UnityARMatrixOps.GetRotation (arPlaneAnchor.transform);

        wrldMapMask.localPosition = new Vector3(arPlaneAnchor.center.x, arPlaneAnchor.center.y, -arPlaneAnchor.center.z);
        wrldMapMask.localScale  = new Vector3(arPlaneAnchor.extent.x, wrldMapMask.localScale.y, arPlaneAnchor.extent.z);

        m_arStreamingController.UpdateStreamingCamera ();
    }

    public void ForceRepositionMap()
    {
        if (m_hasFoundAnchor) 
        {
            UpdateMapPositionWithAnchor (m_currentAnchor);
            UpdateMapMaskWithAnchor (m_currentAnchor);
        }
    }
}
