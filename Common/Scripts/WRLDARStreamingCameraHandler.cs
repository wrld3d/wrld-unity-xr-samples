using UnityEngine;

public class WRLDARStreamingCameraHandler : MonoBehaviour 
{
    private Camera m_streamingCamera;

    [SerializeField]
    private Transform m_wrldMap;

    [SerializeField]
    private MeshRenderer m_streamingMask;

    [SerializeField]
    private float m_minDistance = 700f;

    private void Start()
    {
        m_streamingCamera = GetComponent<Camera> ();
        UpdateStreamingCamera ();
    }

    public void UpdateStreamingCamera()
    {
        m_streamingCamera.transform.position = m_streamingMask.transform.position;

        float maxDistance = GetCameraDistanceRelativeToBounds (m_streamingMask.bounds, m_streamingCamera);

        maxDistance = maxDistance * (1f / m_wrldMap.transform.localScale.x);

        if (maxDistance < m_minDistance) 
        {
            //Never get too close to origin.
            maxDistance = m_minDistance;
        }

        m_streamingCamera.transform.position -= m_streamingCamera.transform.forward * maxDistance;
        m_streamingCamera.farClipPlane = maxDistance * 1.1f; //Stream a bit further than max distance.
        m_streamingCamera.nearClipPlane = maxDistance * 0.1f;
    }

    private float GetCameraDistanceRelativeToBounds(Bounds bounds, Camera cam)
    {
        // project a ray from each of the bounding coords to each of the frustum planes
        float maxDistance = 0;
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(cam);
        Plane[] frustumPlanesToUse =
        {
            // [0] = Left, [1] = Right, [2] = Down,
            // [3] = Up,   [4] = Near,  [5] = Far
            frustumPlanes[0], frustumPlanes[1],
            frustumPlanes[2], frustumPlanes[3]
        };

        foreach (Vector3 v in bounds.GetVertices(true))
        {
            Ray ray = new Ray(v, cam.transform.forward);
            foreach (Plane p in frustumPlanesToUse)
            {
                float enter;
                if (p.Raycast(ray, out enter))
                {
                    if (enter > 0 && enter > maxDistance)
                    {
                        maxDistance = enter;
                    }
                }
            }
        }

        return maxDistance;
    }
}
