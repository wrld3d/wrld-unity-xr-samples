using System.Collections;
using Wrld;
using Wrld.Resources.Buildings;
using Wrld.Space;
using UnityEngine;

public class UpdatingBuildingHighlightColor : MonoBehaviour
{
    private LatLong buildingLocation = LatLong.FromDegrees(37.795189, -122.402777);
    private Color[] colors = new Color[] {
        new Color(1, 1, 0, 0.5f),
        new Color(1, 0, 0, 0.5f),
        new Color(0, 1, 0, 0.5f),
        new Color(0, 0, 1, 0.5f)
    };
    private int nextColorIndex = 0;

    private void OnEnable()
    {
        StartCoroutine(Example());
    }

    IEnumerator Example()
    {
        Api.Instance.CameraApi.MoveTo(buildingLocation, distanceFromInterest: 1000, headingDegrees: 0, tiltDegrees: 45);

        yield return new WaitForSeconds(4.0f);

        var highlight = BuildingHighlight.Create(new BuildingHighlightOptions()
            .HighlightBuildingAtLocation(buildingLocation)
            .Color(NextColor()));

        while (true)
        {
            yield return new WaitForSeconds(2.0f);
            
            highlight.SetColor(NextColor());
        }
    }

    Color NextColor()
    {
        var color = colors[nextColorIndex++];
        nextColorIndex %= colors.Length;
        return color;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
