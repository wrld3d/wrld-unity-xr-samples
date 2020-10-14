using Wrld;
using Wrld.Space;
using UnityEngine;
using System.Collections;

public class HighlightingIndoors : MonoBehaviour
{
    private LatLong m_indoorMapLocation = LatLong.FromDegrees(56.459984, -2.978238);

	private Color m_deskColor = new Color(0.0f, 1.0f, 0.0f, 1.0f);
    private Color m_meetingRoomColor = new Color(1.0f, 0.0f, 0.0f, 0.5f);
    private Color m_smallRoomColor = new Color(0.0f, 0.0f, 1.0f, 0.5f);
	
    private void OnEnable()
    {
        Api.Instance.IndoorMapsApi.OnIndoorMapEntered += IndoorMapsApi_OnIndoorMapEntered;
        Api.Instance.CameraApi.MoveTo(m_indoorMapLocation, distanceFromInterest: 200, headingDegrees: 0, tiltDegrees: 45); 

        StartCoroutine(EnterMap());
    }

    IEnumerator EnterMap()
    {
        yield return new WaitForSeconds(5.0f);

        Api.Instance.CameraApi.MoveTo(m_indoorMapLocation, distanceFromInterest: 500);
        Api.Instance.IndoorMapsApi.EnterIndoorMap("westport_house");
    }
	
	public void HighlightEntities()
	{
		Api.Instance.IndoorMapsApi.SetIndoorEntityHighlight("Meeting Room Small", m_smallRoomColor);
		Api.Instance.IndoorMapsApi.SetIndoorEntityHighlight("Meeting Room Large", m_meetingRoomColor);

        Api.Instance.IndoorMapsApi.SetIndoorEntityHighlights(new[] { "0007", "0002", "0033" }, m_deskColor);
		
	}
	
	public void ClearHighlights()
	{
        Api.Instance.IndoorMapsApi.ClearIndoorEntityHighlights(new[] { "0007", "0002", "0033", "Meeting Room Small", "Meeting Room Large" });
    }

    private void OnDisable()
    {
        Api.Instance.IndoorMapsApi.OnIndoorMapEntered -= IndoorMapsApi_OnIndoorMapEntered;
    }

    private void IndoorMapsApi_OnIndoorMapEntered()
    {
        Api.Instance.IndoorMapsApi.SetCurrentFloorId(2);
    }
}
