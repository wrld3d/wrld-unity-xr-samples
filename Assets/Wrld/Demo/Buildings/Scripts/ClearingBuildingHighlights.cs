using System.Collections;
using Wrld;
using Wrld.Resources.Buildings;
using Wrld.Space;
using UnityEngine;

public class ClearingBuildingHighlights : MonoBehaviour
{
    private LatLong buildingLocation = LatLong.FromDegrees(37.795189, -122.402777);


    private void OnEnable()
    {
        StartCoroutine(Example());
    }

    IEnumerator Example()
    {
        Api.Instance.CameraApi.MoveTo(buildingLocation, distanceFromInterest: 1000, headingDegrees: 0, tiltDegrees: 45);

        while (true)
        {
            yield return new WaitForSeconds(4.0f);

            BuildingHighlight.Create(
                new BuildingHighlightOptions()
                    .HighlightBuildingAtLocation(buildingLocation)
                    .Color(new Color(1, 1, 0, 0.5f))
                    .BuildingInformationReceivedHandler(OnBuildingHighlighted)
            );
        }
    }

    void OnBuildingHighlighted(BuildingHighlight buildingHighlight)
    {
        if (!buildingHighlight.IsDiscarded())
        {
            StartCoroutine(ClearHighlight(buildingHighlight));
        }
    }
    IEnumerator ClearHighlight(BuildingHighlight buildingHighlight)
    {
        yield return new WaitForSeconds(2.0f);
        buildingHighlight.Discard();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
