using Wrld;
using Wrld.Space;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MovingWithSlider : MonoBehaviour
{
    private LatLong m_indoorMapLocation = LatLong.FromDegrees(37.781871, -122.404812);
    private Slider m_floorSlider;

    private void OnEnable()
    {   
        m_floorSlider = FindObjectOfType<Slider>();

        Api.Instance.IndoorMapsApi.OnIndoorMapEntered += IndoorMapsApi_OnIndoorMapEntered;
        Api.Instance.IndoorMapsApi.OnIndoorMapFloorChanged += IndoorMapsApi_OnIndoorMapFloorChanged;

        Api.Instance.CameraApi.MoveTo(m_indoorMapLocation, distanceFromInterest: 500, headingDegrees: 0, tiltDegrees: 45); 
    }

    private void OnDisable()
    {
        Api.Instance.IndoorMapsApi.OnIndoorMapFloorChanged -= IndoorMapsApi_OnIndoorMapFloorChanged;
        Api.Instance.IndoorMapsApi.OnIndoorMapEntered -= IndoorMapsApi_OnIndoorMapEntered;
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

    public void OnSliderValueChanged()
    {
        Api.Instance.IndoorMapsApi.SetIndoorFloorInterpolation(m_floorSlider.value);
    }

    public void OnBeginDrag()
    {
        Api.Instance.IndoorMapsApi.ExpandIndoor();
    }

    public void OnEndDrag()
    {
        float sliderValue = m_floorSlider.value;
        int roundedValue = Mathf.RoundToInt(sliderValue);
        var map = Api.Instance.IndoorMapsApi.GetActiveIndoorMap();

        if (map != null)
        {
            int endFloorId = map.FloorIds[roundedValue];
            Api.Instance.IndoorMapsApi.SetCurrentFloorId(endFloorId);
            Api.Instance.IndoorMapsApi.CollapseIndoor();
        }
    }

    private void IndoorMapsApi_OnIndoorMapFloorChanged(int newFloorId)
    {
        Debug.LogFormat("Switched to floor {0}!", newFloorId);

        var map = Api.Instance.IndoorMapsApi.GetActiveIndoorMap();

        if (map != null)
        {
            m_floorSlider.value = Array.IndexOf(map.FloorIds, newFloorId);
        }
    }

    private void IndoorMapsApi_OnIndoorMapEntered()
    {
        var map = Api.Instance.IndoorMapsApi.GetActiveIndoorMap();
        m_floorSlider.minValue = 0.0f;
        m_floorSlider.maxValue = map.FloorCount - 1.0f;
        var currentFloorId = Api.Instance.IndoorMapsApi.GetCurrentFloorId();
        m_floorSlider.value = Array.IndexOf(map.FloorIds, currentFloorId);
    }

}
