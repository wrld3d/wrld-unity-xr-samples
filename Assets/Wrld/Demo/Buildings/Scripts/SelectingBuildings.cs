using System.Collections;
using Wrld;
using Wrld.Space;
using Wrld.Resources.Buildings;
using UnityEngine;

public class SelectingBuildings : MonoBehaviour
{
    [SerializeField]
    private GameObject boxPrefab = null;

    private LatLong buildingLocation = LatLong.FromDegrees(37.793988, -122.403390);

    private void OnEnable()
    {
        StartCoroutine(Example());
    }

    IEnumerator Example()
    {
        Api.Instance.CameraApi.MoveTo(buildingLocation, distanceFromInterest: 500, headingDegrees: 0, tiltDegrees: 45);

        while (true)
        {
            yield return new WaitForSeconds(4.0f);

            BuildingHighlight.Create(
                new BuildingHighlightOptions()
                    .HighlightBuildingAtLocation(buildingLocation)
                    .BuildingInformationReceivedHandler(this.OnBuildingInformationReceived)
                    .InformationOnly()
            );
        }
    }

    void OnBuildingInformationReceived(BuildingHighlight highlight)
    {
        if (highlight.IsDiscarded())
        {
            Debug.Log(string.Format("No building information was received"));
            return;
        }

        var buildingInformation = highlight.GetBuildingInformation();

        var boxAnchor = Instantiate(boxPrefab) as GameObject;
        boxAnchor.GetComponent<GeographicTransform>().SetPosition(buildingInformation.BuildingDimensions.Centroid);

        var box = boxAnchor.transform.GetChild(0);
        box.localPosition = Vector3.up * (float)buildingInformation.BuildingDimensions.TopAltitude;
        Destroy(boxAnchor, 2.0f);

        Debug.Log(string.Format("Building information received: {0}", buildingInformation.ToJson()));

        highlight.Discard();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
