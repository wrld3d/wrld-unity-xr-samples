using UnityEngine;
using UnityEngine.EventSystems;
using Wrld;

public class ConditionallyPreventCameraMovement : MonoBehaviour
{
    void OnEnable()
    {
        Api.Instance.CameraApi.SetShouldConsumeInputDelegate(ShouldConsumeCameraInput);
    }

    private void OnDisable()
    {
        Api.Instance.CameraApi.ClearShouldConsumeInputDelegate();
    }

    bool ShouldConsumeCameraInput(int pointerId)
    {
        return !EventSystem.current.IsPointerOverGameObject(pointerId);
    }
}
