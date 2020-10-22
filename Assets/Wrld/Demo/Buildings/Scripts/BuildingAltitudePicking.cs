using System.Collections;
using Wrld;
using Wrld.Space;
using UnityEngine;

public class BuildingAltitudePicking : MonoBehaviour
{
    [SerializeField]
    private GameObject boxPrefab = null;

    private LatLong cameraLocation = LatLong.FromDegrees(37.795641, -122.404173);
    private LatLong boxLocation1 = LatLong.FromDegrees(37.795159, -122.404336);
    private LatLong boxLocation2 = LatLong.FromDegrees(37.795173, -122.404229);


    private void OnEnable()
    {
        StartCoroutine(Example());
    }

    IEnumerator Example()
    {
        Api.Instance.CameraApi.MoveTo(cameraLocation, distanceFromInterest: 400, headingDegrees: 0, tiltDegrees: 45);

        while (true)
        {
            yield return new WaitForSeconds(4.0f);

            MakeBox(boxLocation1);
            MakeBox(boxLocation2);
        }
    }

    void MakeBox(LatLong latLong)
    {
        var ray = Api.Instance.SpacesApi.LatLongToVerticallyDownRay(latLong);
        LatLongAltitude buildingIntersectionPoint;
        var didIntersectBuilding = Api.Instance.BuildingsApi.TryFindIntersectionWithBuilding(ray, out buildingIntersectionPoint);
        if (didIntersectBuilding)
        {
            var boxAnchor = Instantiate(boxPrefab) as GameObject;
            boxAnchor.GetComponent<GeographicTransform>().SetPosition(buildingIntersectionPoint.GetLatLong());

            var box = boxAnchor.transform.GetChild(0);
            box.localPosition = Vector3.up * (float)buildingIntersectionPoint.GetAltitude();
            Destroy(boxAnchor, 2.0f);
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
