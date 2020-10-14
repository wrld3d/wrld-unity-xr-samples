using Wrld;
using Wrld.Space;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MovingBetweenFloors : MonoBehaviour
{
    private LatLong m_indoorMapLocation = LatLong.FromDegrees(37.781871, -122.404812);

    private void OnEnable()
    {   
        Api.Instance.IndoorMapsApi.OnIndoorMapFloorChanged += IndoorMapsApi_OnIndoorMapFloorChanged;

        Api.Instance.CameraApi.MoveTo(m_indoorMapLocation, distanceFromInterest: 500, headingDegrees: 0, tiltDegrees: 45); 
    }

    private void OnDisable()
    {
        Api.Instance.IndoorMapsApi.OnIndoorMapFloorChanged -= IndoorMapsApi_OnIndoorMapFloorChanged;
    }

    public void OnExpand()
    {
        Api.Instance.IndoorMapsApi.ExpandIndoor();
    }

    public void OnCollapse()
    {
        Api.Instance.IndoorMapsApi.CollapseIndoor();
    }

    public void MoveUp()
    {
        Api.Instance.IndoorMapsApi.MoveUpFloor();
    }

    public void MoveDown()
    {
        Api.Instance.IndoorMapsApi.MoveDownFloor();
    }

    public void ExitMap()
    {
        Api.Instance.IndoorMapsApi.ExitIndoorMap();
    }

    public void EnterMap()
    {
        if (Api.Instance.IndoorMapsApi.GetActiveIndoorMap() == null)
        {
            Api.Instance.CameraApi.MoveTo(m_indoorMapLocation, distanceFromInterest: 500);
            Api.Instance.IndoorMapsApi.EnterIndoorMap("intercontinental_hotel_8628");
        }
    }


    private void IndoorMapsApi_OnIndoorMapFloorChanged(int newFloorId)
    {
        Debug.LogFormat("Switched to floor {0}!", newFloorId);
    }


}
