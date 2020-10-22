using System.Collections;
using Wrld;
using Wrld.Space;
using UnityEngine;
using UnityEngine.UI;

public class DisplayIndoorMapName : MonoBehaviour
{
    private LatLong m_indoorMapLocation = LatLong.FromDegrees(37.781871, -122.404812);

    private void OnEnable()
    {
        Api.Instance.IndoorMapsApi.OnIndoorMapEntered += IndoorMapsApi_OnIndoorMapEntered;

        Api.Instance.CameraApi.MoveTo(m_indoorMapLocation, distanceFromInterest: 500, headingDegrees: 0, tiltDegrees: 45);
    }

    public void EnterMap()
    {
        if (Api.Instance.IndoorMapsApi.GetActiveIndoorMap() == null)
        {
            Api.Instance.CameraApi.MoveTo(m_indoorMapLocation, distanceFromInterest: 500);
            Api.Instance.IndoorMapsApi.EnterIndoorMap("intercontinental_hotel_8628");
        }
    }
    
	public void ExitMap() 
    {
        Api.Instance.IndoorMapsApi.ExitIndoorMap();
    }

    private void OnDisable()
    {
        Api.Instance.IndoorMapsApi.OnIndoorMapEntered -= IndoorMapsApi_OnIndoorMapEntered;
    }

    private void IndoorMapsApi_OnIndoorMapEntered()
    {
        Debug.LogFormat("Indoor Map Name: {0}", Api.Instance.IndoorMapsApi.GetActiveIndoorMap().Name);


    }

}
