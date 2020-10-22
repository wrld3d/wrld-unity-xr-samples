using System.Collections;
using Wrld;
using Wrld.Space;
using UnityEngine;

public class CameraTransitionAnimate : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(Example());
    }

    IEnumerator Example()
    {
        var startLocation = LatLong.FromDegrees(37.7858, -122.401);
        Api.Instance.CameraApi.MoveTo(startLocation, distanceFromInterest: 1890);

        yield return new WaitForSeconds(4.0f);

        var destLocation = LatLong.FromDegrees(37.802, -122.4058);
        Api.Instance.CameraApi.AnimateTo(destLocation, distanceFromInterest: 500, headingDegrees: 270, tiltDegrees: 30, transitionDuration: 5, jumpIfFarAway: false);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}

