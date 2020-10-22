using System.Collections;
using Wrld;
using Wrld.Space;
using UnityEngine;

public class SetCustomRenderCamera : MonoBehaviour
{
    private Camera m_mainCamera;
    
    private void OnEnable()
    {
        m_mainCamera = UnityEngine.Camera.main;
        m_mainCamera.nearClipPlane = 2;
        m_mainCamera.farClipPlane = 8000;


        Api.Instance.CameraApi.SetCustomRenderCamera(m_mainCamera);
    }

    private void Update()
    {
        // Animate the UnityEngine.Camera instance by setting its transform. 
        const float heightOffset = 250.0f;
        m_mainCamera.transform.position = new Vector3(0.0f, heightOffset, 0.0f);

        const float pitch = 30.0f;
        float yaw = Mathf.Sin(Time.realtimeSinceStartup * 1.0f) * 25.0f;
        const float roll = 0.0f;
        m_mainCamera.transform.rotation = Quaternion.Euler(pitch, yaw, roll);
    }

}

