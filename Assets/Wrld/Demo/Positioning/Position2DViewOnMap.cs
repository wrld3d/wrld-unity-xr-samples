using System.Collections;
using UnityEngine;
using Wrld;
using Wrld.Space;
using Wrld.Space.Positioners;

public class Position2DViewOnMap: MonoBehaviour
{
    static LatLong targetPosition = LatLong.FromDegrees(37.802355, -122.405848);
    
    Positioner viewPositioner;
    public UnityEngine.RectTransform target2DView;

    private void OnEnable()
    {
        var positionerOptions = new PositionerOptions()
                                                .ElevationAboveGround(25)
                                                .LatitudeDegrees(targetPosition.GetLatitude())
                                                .LongitudeDegrees(targetPosition.GetLongitude());

        viewPositioner = Api.Instance.PositionerApi.CreatePositioner(positionerOptions);
        viewPositioner.OnScreenPointChanged += OnPositionerPositionChanged;
    }

    private void OnPositionerPositionChanged()
    {
        var screenPoint = Vector3.zero;
        if (viewPositioner.TryGetScreenPoint(out screenPoint))
        {
            target2DView.position = new Vector3(screenPoint.x, screenPoint.y);
        }
    }

    public void OnCollapseButtonClicked()
    {
        Api.Instance.EnvironmentFlatteningApi.SetIsFlattened(!Api.Instance.EnvironmentFlatteningApi.IsFlattened());
    }

    public void OnDisable()
    {
        viewPositioner.OnScreenPointChanged -= OnPositionerPositionChanged;
        viewPositioner.Discard();
    }
}

