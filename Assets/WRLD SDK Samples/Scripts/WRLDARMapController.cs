using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARPlaneManager))]
public class WRLDARMapController : MonoBehaviour
{
    #pragma warning disable 0649
    [SerializeField] private Transform m_wrldMapMask;
    [SerializeField] private WRLDARStreamingCameraHandler m_streamingCameraHandler;
    #pragma warning restore 0649

    private ARPlaneManager m_arPlaneManager;
    private ARPlane m_currentARPlane = null;
    private Dictionary<TrackableId, ARPlane> m_detectedPlanes;

    void Awake() {
        m_detectedPlanes = new Dictionary<TrackableId, ARPlane>();
        m_arPlaneManager = gameObject.GetComponent<ARPlaneManager>();
    }

    void OnEnable()
    {
        m_arPlaneManager.planesChanged += OnARPlanesChanged;
    }

    void OnDisable() {
        m_arPlaneManager.planesChanged -= OnARPlanesChanged;
    }

    void OnARPlanesChanged(ARPlanesChangedEventArgs args) {
        if (args.added.Count > 0) {
            foreach (var addedPlane in args.added) {
                if (addedPlane.alignment == UnityEngine.XR.ARSubsystems.PlaneAlignment.HorizontalUp) {
                    AddPlane(addedPlane);
                }
            }
        }

        if (args.removed.Count > 0) {
            foreach (var removedPlane in args.removed) {
                RemovePlane(removedPlane);
            }
        }

        // Make sure we are not using any child planes
        if (m_detectedPlanes.Count > 0) {
            foreach (var plane in m_detectedPlanes.Values) {
                if (plane.subsumedBy != null) {
                    RemovePlane(plane);
                    AddPlane(GetTopMostPlane(plane));
                }
            }

            // Use one of the detected planes to show map on. Should probably move 
            // to tapping a plane but replicating old functionality right now.
            var enumerator = m_detectedPlanes.GetEnumerator();
            enumerator.MoveNext();
            var planeToUse = enumerator.Current.Value;
            CheckAndSetCurrentPlane(planeToUse);
        }
    }

    ARPlane GetTopMostPlane(ARPlane plane) {
        var currentPlane = plane;
        while(currentPlane.subsumedBy != null) {
            currentPlane = currentPlane.subsumedBy;
        }

        return currentPlane;
    }

    void AddPlane(ARPlane plane) {
        m_detectedPlanes [plane.trackableId] = plane;
    }

    void RemovePlane(ARPlane plane) {
        if (m_detectedPlanes.ContainsKey(plane.trackableId)) {
            m_detectedPlanes.Remove(plane.trackableId);
        }

        if (m_currentARPlane != null && m_currentARPlane.trackableId == plane.trackableId) {
            ClearCurrentPlane();
        }
    }

    void CheckAndSetCurrentPlane(ARPlane plane) {
        if (m_currentARPlane == null) {
            m_currentARPlane = plane;
            m_currentARPlane.boundaryChanged += OnCurrentPlaneBoundaryChanged;
        }
    }

    void ClearCurrentPlane() {
        m_currentARPlane.boundaryChanged -= OnCurrentPlaneBoundaryChanged;
        m_currentARPlane = null;
    }

    void SetWrldMapPosition() {
        m_wrldMapMask.parent.position = m_currentARPlane.transform.position;
    }

    void OnCurrentPlaneBoundaryChanged(ARPlaneBoundaryChangedEventArgs args) {
        if (m_currentARPlane != null) {
            m_wrldMapMask.localPosition = new Vector3(m_currentARPlane.center.x, m_wrldMapMask.localPosition.y, m_currentARPlane.center.z);
            m_wrldMapMask.rotation = m_currentARPlane.transform.rotation;
            m_wrldMapMask.localScale = new Vector3(m_currentARPlane.size.x, 0.1f, m_currentARPlane.size.y);
            m_streamingCameraHandler.UpdateStreamingCamera();
        }
    }
}
