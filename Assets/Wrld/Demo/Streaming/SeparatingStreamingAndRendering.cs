using UnityEngine;
using Wrld;


public class SeparatingStreamingAndRendering: MonoBehaviour
{
    public Camera renderingCamera;

    void OnEnable()
    {
        Api.Instance.CameraApi.SetControlledCamera(renderingCamera);
    }
}
