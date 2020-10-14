using System.Collections;
using Wrld;
using Wrld.Resources.Buildings;
using Wrld.Space;
using UnityEngine;

public class HighlightingBuildings : MonoBehaviour
{
    private LatLong buildingLocation = LatLong.FromDegrees(37.795189, -122.402777);


    private void OnEnable()
    {
        StartCoroutine(Example());
    }

    IEnumerator Example()
    {
        Api.Instance.CameraApi.MoveTo(buildingLocation, distanceFromInterest: 1000, headingDegrees: 0, tiltDegrees: 45);

        yield return new WaitForSeconds(4.0f);

        BuildingHighlight.Create(
            new BuildingHighlightOptions()
                .HighlightBuildingAtLocation(buildingLocation)
                .Color(new Color(1, 1, 0, 0.5f))
                .BuildingInformationReceivedHandler(OnBuildingInformationReceived)
        );
    }

    void OnBuildingInformationReceived(BuildingHighlight highlight)
    {
        if (highlight.IsDiscarded())
        {
            return;
        }

        Debug.Log(string.Format("Received information for building with id '{0}'", highlight.GetBuildingInformation().BuildingId));
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
