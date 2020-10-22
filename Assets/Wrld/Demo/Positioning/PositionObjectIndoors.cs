using Wrld;
using Wrld.Space;
using UnityEngine;
using System.Collections;

public class PositionObjectIndoors : MonoBehaviour
{
    private LatLong m_indoorMapLocation = LatLong.FromDegrees(56.459984, -2.978238);
	
    private void OnEnable()
    {
        Api.Instance.IndoorMapsApi.OnIndoorMapEntered += IndoorMapsApi_OnIndoorMapEntered;
        Api.Instance.CameraApi.MoveTo(m_indoorMapLocation, distanceFromInterest: 30, headingDegrees: 0, tiltDegrees: 45); 

        StartCoroutine(EnterMap());
    }

    IEnumerator EnterMap()
    {
        yield return new WaitForSeconds(5.0f);

        Api.Instance.CameraApi.MoveTo(m_indoorMapLocation, distanceFromInterest: 30);
        Api.Instance.IndoorMapsApi.EnterIndoorMap("westport_house");
    }
	
    private void OnDisable()
    {
        Api.Instance.IndoorMapsApi.OnIndoorMapEntered -= IndoorMapsApi_OnIndoorMapEntered;
    }

    private void IndoorMapsApi_OnIndoorMapEntered()
    {
        Api.Instance.IndoorMapsApi.SetCurrentFloorId(2);
    }
}
