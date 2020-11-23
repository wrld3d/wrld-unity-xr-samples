using UnityEngine;

public class WRLDARStreamingCameraHandler : MonoBehaviour
{
    private Camera m_streamingCamera;

#pragma warning disable 0649
    [SerializeField] private Transform m_wrldMap;
    [SerializeField] private MeshRenderer m_streamingMask;
#pragma warning restore 0649

    private void Start()
    {
        m_streamingCamera = GetComponent<Camera>();
        UpdateStreamingCamera();
    }

    public void UpdateStreamingCamera()
    {
        Bounds bounds = m_streamingMask.bounds;
        var cameraDistance = bounds.extents.x / Mathf.Tan((m_streamingCamera.fieldOfView * Mathf.Deg2Rad) * 0.5f);
        var scale = m_wrldMap.lossyScale.x;
        m_streamingCamera.transform.position = bounds.center + Vector3.up * cameraDistance;
        m_streamingCamera.transform.LookAt(bounds.center, Vector3.forward);
        m_streamingCamera.aspect = bounds.extents.x / bounds.extents.z;
        float calculatedNear = cameraDistance - (float)Wrld.Space.EarthConstants.MaxElevation * scale;
        m_streamingCamera.nearClipPlane = Mathf.Clamp(calculatedNear, 0.1f, cameraDistance);
        float calculatedFar = cameraDistance + 1000 * scale;
        m_streamingCamera.farClipPlane = Mathf.Max(calculatedFar, calculatedNear + 0.1f);
    }
}
