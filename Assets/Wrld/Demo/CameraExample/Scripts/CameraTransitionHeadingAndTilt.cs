using System.Collections;
using Wrld;
using Wrld.Space;
using UnityEngine;

public class CameraTransitionHeadingAndTilt : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(Example());
    }

    IEnumerator Example()
    {
        var startLocation = LatLong.FromDegrees(37.7858, -122.401);
        Api.Instance.CameraApi.MoveTo(startLocation, distanceFromInterest: 800, headingDegrees: 0, tiltDegrees: 50);

        yield return new WaitForSeconds(4.0f);

        var endLocation = LatLong.FromDegrees(37.7858, -122.401);
        Api.Instance.CameraApi.AnimateTo(endLocation, headingDegrees: 45, tiltDegrees: 20);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}

