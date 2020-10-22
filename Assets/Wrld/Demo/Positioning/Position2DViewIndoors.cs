using Wrld;
using Wrld.Space;
using UnityEngine;
using System.Collections;
using Wrld.Space.Positioners;

public class Position2DViewIndoors : MonoBehaviour
{
    private LatLong m_indoorMapLocation = LatLong.FromDegrees(56.459984, -2.978238);
    private string m_indoorMapId = "westport_house";
    private int m_indoorMapFloorId = 2;

    Positioner viewPositioner;
    public UnityEngine.RectTransform target2DView;

    private void OnEnable()
    {
        Api.Instance.IndoorMapsApi.OnIndoorMapEntered += IndoorMapsApi_OnIndoorMapEntered;
        Api.Instance.CameraApi.MoveTo(m_indoorMapLocation, distanceFromInterest: 30, headingDegrees: 0, tiltDegrees: 45);

        var positionerOptions = new PositionerOptions()
                                        .ElevationAboveGround(1.0f)
                                        .LatitudeDegrees(m_indoorMapLocation.GetLatitude())
                                        .LongitudeDegrees(m_indoorMapLocation.GetLongitude())
                                        .IndoorMapWithFloorId(m_indoorMapId, m_indoorMapFloorId);

        viewPositioner = Api.Instance.PositionerApi.CreatePositioner(positionerOptions);
        viewPositioner.OnScreenPointChanged += OnPositionerPositionUpdated;

        StartCoroutine(EnterMap());
    }

    private void OnPositionerPositionUpdated()
    {
        var screenPoint = Vector3.zero;
        if (viewPositioner.TryGetScreenPoint(out screenPoint))
        {
            target2DView.position = new Vector3(screenPoint.x, screenPoint.y);
        }
    }

    IEnumerator EnterMap()
    {
        yield return new WaitForSeconds(5.0f);

        Api.Instance.CameraApi.MoveTo(m_indoorMapLocation, distanceFromInterest: 30);
        Api.Instance.IndoorMapsApi.EnterIndoorMap(m_indoorMapId);
    }
	
    private void OnDisable()
    {
        viewPositioner.OnScreenPointChanged -= OnPositionerPositionUpdated;
        viewPositioner.Discard();

        Api.Instance.IndoorMapsApi.OnIndoorMapEntered -= IndoorMapsApi_OnIndoorMapEntered;
    }

    private void IndoorMapsApi_OnIndoorMapEntered()
    {
        Api.Instance.IndoorMapsApi.SetCurrentFloorId(m_indoorMapFloorId);
    }
}
