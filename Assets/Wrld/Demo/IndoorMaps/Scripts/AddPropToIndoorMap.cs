using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wrld;
using Wrld.Resources.Props;
using Wrld.Space;

public class AddPropToIndoorMap : MonoBehaviour
{
    private LatLong m_indoorMapLocation = LatLong.FromDegrees(37.782080, -122.404575);
    private List<Prop> m_props = new List<Prop>();

    private void OnEnable()
    {
        Api.Instance.PropsApi.SetAutomaticIndoorMapPopulationEnabled(false);
        
        Api.Instance.CameraApi.MoveTo(m_indoorMapLocation, distanceFromInterest: 30, headingDegrees: 0, tiltDegrees: 45);

        StartCoroutine(EnterMap());
    }

    IEnumerator EnterMap()
    {
        yield return new WaitForSeconds(5.0f);

        EnterIndoorMap();
    }

    private void OnDisable()
    {
        foreach (var prop in m_props)
        {
            prop.Discard();
        }

        m_props.Clear();
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

    public void Exit()
    {
        foreach (var prop in m_props)
        {
            prop.Discard();
        }

        m_props.Clear();

        Api.Instance.IndoorMapsApi.ExitIndoorMap();
    }

    public void EnterIndoorMap()
    {
        Api.Instance.CameraApi.MoveTo(m_indoorMapLocation, distanceFromInterest: 30);
        Api.Instance.IndoorMapsApi.EnterIndoorMap("intercontinental_hotel_8628");

        const int numberOfFloors = 5;

        for (int floorId = 0; floorId < numberOfFloors; ++floorId)
        {
            var options = new PropOptions()
                .LatitudeDegrees(m_indoorMapLocation.GetLatitude())
                .LongitudeDegrees(m_indoorMapLocation.GetLongitude())
                .IndoorMapWithFloorId("intercontinental_hotel_8628", floorId)
                .GeometryId("duck")
                .Name("my_prop" + floorId.ToString())
                .ElevationAboveGround(0.0)
                .HeadingDegrees(90.0);
            m_props.Add(Api.Instance.PropsApi.CreateProp(options));
        }
    }
}
