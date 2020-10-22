using Wrld;
using Wrld.Space;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wrld.Resources.IndoorMaps;

public class PickingIndoors : MonoBehaviour
{
    private LatLong m_indoorMapLocation = LatLong.FromDegrees(56.459984, -2.978238);
    
    private List<Color> m_pickColors = new List<Color>
    {
        new Color(1.0f, 0.0f, 0.0f, 0.5f),
        new Color(0.0f, 1.0f, 0.0f, 0.5f),
        new Color(0.0f, 0.0f, 1.0f, 0.5f)
    };

    private Dictionary<string, int> m_entityIdsToColorIndex = new Dictionary<string, int>();

    private void OnEnable()
    {
        Api.Instance.IndoorMapsApi.OnIndoorMapEntitiesClicked += IndoorMapsApi_OnIndoorMapEntitiesClicked;
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

    public void ClearHighlights()
    {
        Api.Instance.IndoorMapsApi.ClearIndoorEntityHighlights();
    }

    private void OnDisable()
    {
        Api.Instance.IndoorMapsApi.OnIndoorMapEntitiesClicked -= IndoorMapsApi_OnIndoorMapEntitiesClicked;
        Api.Instance.IndoorMapsApi.OnIndoorMapEntered -= IndoorMapsApi_OnIndoorMapEntered;
    }

    private void IndoorMapsApi_OnIndoorMapEntitiesClicked(IndoorMapEntitiesClicked entities)
    {
        Debug.Log("Clicked on interior object(s)!: " + string.Join(", ", entities.Ids.ToArray()));

        foreach(var id in entities.Ids){
            m_entityIdsToColorIndex[id] = m_entityIdsToColorIndex.ContainsKey(id) ? (m_entityIdsToColorIndex[id] + 1) % m_pickColors.Count : 0;
            var color = m_pickColors[m_entityIdsToColorIndex[id]];
            Api.Instance.IndoorMapsApi.SetIndoorEntityHighlight(id, color);
        }
    }

    private void IndoorMapsApi_OnIndoorMapEntered()
    {
        Api.Instance.IndoorMapsApi.SetCurrentFloorId(2);
    }
}
