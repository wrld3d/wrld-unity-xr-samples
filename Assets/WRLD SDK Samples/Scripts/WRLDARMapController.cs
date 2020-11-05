using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARPlaneManager))]
[RequireComponent(typeof(ARRaycastManager))]
public class WRLDARMapController : MonoBehaviour
{
    #pragma warning disable 0649
    [SerializeField] private Transform m_wrldMapMask;
    [SerializeField] private WRLDARStreamingCameraHandler m_streamingCameraHandler;
    [SerializeField] private GameObject[] m_SurfaceStateMsg;
    #pragma warning restore 0649

    private ARPlaneManager m_arPlaneManager;
    private ARRaycastManager m_RaycastManager;
    private ARPlane m_currentARPlane = null;
    private Dictionary<TrackableId, ARPlane> m_detectedPlanes;
    private List<ARRaycastHit> m_hits = new List<ARRaycastHit>();
    private bool m_isSurfaceSelected = false;
    private bool m_isSurfaceDetected = false;
    private enum SurfaceState { NotDetect, Detected, Selected};

    void Awake() {
        m_detectedPlanes = new Dictionary<TrackableId, ARPlane>();
        m_arPlaneManager = gameObject.GetComponent<ARPlaneManager>();
        m_RaycastManager = GetComponent<ARRaycastManager>();
    }

    bool TryGetTouchPosition(out Vector2 touchPosition)
    {
            if (Input.touchCount > 0)
            {
                touchPosition = Input.GetTouch(0).position;
                return true;
            }
        touchPosition = default;
        return false;
    }

    void Update()
    {
        if (!m_isSurfaceSelected) {
            if (!TryGetTouchPosition(out Vector2 touchPosition))
                return;

            if (m_RaycastManager.Raycast(touchPosition, m_hits, TrackableType.PlaneWithinPolygon))
            {
                foreach (var plane in m_detectedPlanes.Values)
                {
                    if (plane.trackableId == m_hits[0].trackableId)
                    {
                        m_isSurfaceSelected = true;
                        UpdateSurfaceStateMsg(SurfaceState.Selected);
                        CheckAndSetCurrentPlane(plane);
                        DisableDetectedPlaneVisuals(plane.trackableId);
                    }
                }
            }
        }
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
                    if (!m_isSurfaceDetected)
                    {
                        m_isSurfaceDetected = true;
                        UpdateSurfaceStateMsg(SurfaceState.Detected);
                    }
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

    void OnCurrentPlaneBoundaryChanged(ARPlaneBoundaryChangedEventArgs args) {
        if (m_currentARPlane != null) {
            m_wrldMapMask.parent.position = m_currentARPlane.transform.position;
            m_wrldMapMask.rotation = m_currentARPlane.transform.rotation;
            m_wrldMapMask.localScale = new Vector3(m_currentARPlane.size.x, 1f, m_currentARPlane.size.y);
            m_streamingCameraHandler.UpdateStreamingCamera();
        }
    }

    private void DisableDetectedPlaneVisuals(TrackableId selectedPlaneID)
    {
        foreach (var plane in m_arPlaneManager.trackables)
        {
            if (plane.trackableId == selectedPlaneID)
            {
                plane.gameObject.GetComponent<ARPlaneMeshVisualizer>().enabled = false;
                plane.gameObject.GetComponent<MeshCollider>().enabled = false;
                plane.gameObject.GetComponent<MeshRenderer>().enabled = false;
                plane.gameObject.GetComponent<LineRenderer>().enabled = false;
            }
            else
            {
                plane.gameObject.SetActive(false);
            }
        }
        m_arPlaneManager.planePrefab.GetComponent<ARPlaneMeshVisualizer>().enabled = false;
        m_arPlaneManager.planePrefab.GetComponent<MeshCollider>().enabled = false;
        m_arPlaneManager.planePrefab.GetComponent<MeshRenderer>().enabled = false;
        m_arPlaneManager.planePrefab.GetComponent<LineRenderer>().enabled = false;
    }

    private void UpdateSurfaceStateMsg(SurfaceState surfaceState)
    {
        switch (surfaceState)
        {
            case SurfaceState.Detected:
                m_SurfaceStateMsg[0].SetActive(false);
                m_SurfaceStateMsg[1].SetActive(true);
                break;
            case SurfaceState.Selected:
                m_SurfaceStateMsg[1].SetActive(false);
                break;
            default:
                break;
        }
    }
}
