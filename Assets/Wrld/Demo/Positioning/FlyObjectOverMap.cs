using System.Collections;
using UnityEngine;
using Wrld;
using Wrld.Common.Maths;
using Wrld.Space;
using Wrld.Space.Positioners;

public class FlyObjectOverMap: MonoBehaviour
{
    float movementSpeed = 100.0f;
    float turnSpeed = 90.0f;
    float movementAngle = 0.0f;

    public GeographicTransform coordinateFrame;
    public Transform box;
    public LatLong targetPosition = new LatLong(37.802, -122.406);

    void OnEnable()
    {
        Api.Instance.CameraApi.MoveTo(targetPosition, distanceFromInterest: 1700, headingDegrees: 0, tiltDegrees: 45);
        coordinateFrame.SetPosition(targetPosition);
    }

    void Update()
    {
        // Update movement angle from input
        movementAngle += Input.GetAxis("Horizontal") * turnSpeed * Time.deltaTime;
        coordinateFrame.SetHeading(movementAngle);

        // Update target position from input
        var latitudeDelta  = Mathf.Cos(Mathf.Deg2Rad * movementAngle) * Input.GetAxis("Vertical") * movementSpeed * Time.deltaTime;
        var longitudeDelta = Mathf.Sin(Mathf.Deg2Rad * movementAngle) * Input.GetAxis("Vertical") * movementSpeed * Time.deltaTime;

        targetPosition.SetLatitude(targetPosition.GetLatitude() + (latitudeDelta * 0.00006f));
        targetPosition.SetLongitude(targetPosition.GetLongitude() + (longitudeDelta * 0.00006f));

        // Command GeographicTransform to move using lat-long
        coordinateFrame.SetPosition(targetPosition);
    }
}

