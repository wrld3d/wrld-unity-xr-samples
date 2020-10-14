using System.Collections;
using Wrld;
using Wrld.Resources.Buildings;
using Wrld.Space;
using UnityEngine;


public class PickingBuildings : MonoBehaviour
{
    private Vector3 mouseDownPosition;

    void OnEnable()
    {
        var cameraLocation = LatLong.FromDegrees(37.795641, -122.404173);
        Api.Instance.CameraApi.MoveTo(cameraLocation, distanceFromInterest: 400, headingDegrees: 0, tiltDegrees: 45);
    }

    void Update()
    {
        var didTap = UpdateInputDidTap();

        if (didTap)
        {
            var ray = Api.Instance.SpacesApi.ScreenPointToRay(Input.mousePosition);

            LatLongAltitude intersectionPoint;
            var didIntersectBuilding = Api.Instance.BuildingsApi.TryFindIntersectionWithBuilding(ray, out intersectionPoint);
            if (didIntersectBuilding)
            {
                BuildingHighlight.Create(
                    new BuildingHighlightOptions()
                        .HighlightBuildingAtScreenPoint(Input.mousePosition)
                        .Color(new Color(1, 1, 0, 0.5f))
                        .BuildingInformationReceivedHandler(this.OnBuildingInformationReceived)
                );                
            }
        }
    }

    private void OnBuildingInformationReceived(BuildingHighlight highlight)
    {
        if (!highlight.IsDiscarded())
        {
            StartCoroutine(ClearHighlight(highlight));
        }
    }

    private bool UpdateInputDidTap()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mouseDownPosition = Input.mousePosition;
        }
        return (Input.GetMouseButtonUp(0) && Vector3.Distance(mouseDownPosition, Input.mousePosition) < 5.0f);
    }

    IEnumerator ClearHighlight(BuildingHighlight highlight)
    {
        yield return new WaitForSeconds(4.0f);
        highlight.Discard();
    }
}
