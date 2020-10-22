using System.Collections;
using UnityEngine;
using Wrld;
using Wrld.Space;
using Wrld.Space.Positioners;

public class PositionObjectWithPositioner: MonoBehaviour
{
    static LatLong targetPosition = LatLong.FromDegrees(37.783372, -122.400834);
    
    public Transform box;
    Positioner boxPositioner;

    private void OnEnable()
    {
        var positionerOptions = new PositionerOptions()
                                                .ElevationAboveGround(0)
                                                .LatitudeDegrees(targetPosition.GetLatitude())
                                                .LongitudeDegrees(targetPosition.GetLongitude());

        boxPositioner = Api.Instance.PositionerApi.CreatePositioner(positionerOptions);
        boxPositioner.OnTransformedPointChanged += OnPositionerPositionUpdated;
    }

    private void OnPositionerPositionUpdated()
    {
        var boxLocation = new LatLongAltitude();
        if (boxPositioner.TryGetLatLongAltitude(out boxLocation))
        {
            box.position = Api.Instance.SpacesApi.GeographicToWorldPoint(boxLocation);
        }
    }

    private void OnDisable()
    {
        boxPositioner.OnTransformedPointChanged -= OnPositionerPositionUpdated;
        boxPositioner.Discard();
    }
}

