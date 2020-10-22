using Wrld;
using Wrld.Space;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Wrld.Resources.IndoorMaps;
using Wrld.Space.Positioners;
using System.Linq;

public class QueryIndoorMapEntityInformation : MonoBehaviour
{
    private readonly LatLong m_indoorMapLocation = LatLong.FromDegrees(56.459984, -2.978238);
    private const string IndoorMapId = "westport_house";

    private IndoorMapEntityInformation m_indoorMapEntityInformation;
    private Dictionary<Positioner, GameObject> m_positionerToSpheres = new Dictionary<Positioner, GameObject>();

    private void OnEnable()
    {
        Api.Instance.PositionerApi.OnPositionerTransformedPointChanged += OnPositionerTransformedPointChanged;
        Api.Instance.IndoorMapEntityInformationApi.OnIndoorMapEntityInformationUpdated += OnIndoorMapEntityInformationUpdated;

        StartCoroutine(Example());
    }

    private void OnDisable()
    {
        Api.Instance.PositionerApi.OnPositionerTransformedPointChanged -= OnPositionerTransformedPointChanged;
        Api.Instance.IndoorMapEntityInformationApi.OnIndoorMapEntityInformationUpdated -= OnIndoorMapEntityInformationUpdated;
        DestroyPositionersAndSpheres();
        m_indoorMapEntityInformation.Discard();
    }

    IEnumerator Example()
    {
        m_indoorMapEntityInformation = Api.Instance.IndoorMapEntityInformationApi.AddIndoorMapEntityInformation(IndoorMapId, OnIndoorMapEntityInformationChanged);

        Api.Instance.CameraApi.MoveTo(m_indoorMapLocation, distanceFromInterest: 500);

        yield return new WaitForSeconds(5.0f);

        Api.Instance.IndoorMapsApi.OnIndoorMapEntered += () => { Api.Instance.IndoorMapsApi.SetCurrentFloorId(2); };
        Api.Instance.IndoorMapsApi.EnterIndoorMap(IndoorMapId);
    }


    private void OnIndoorMapEntityInformationChanged(IndoorMapEntityInformation indoorMapEntityInformation)
    {
        // indoorMapEntityInformation changed, create a sphere postioned at the location of each indoor map entity.
        DestroyPositionersAndSpheres();

        m_positionerToSpheres = indoorMapEntityInformation.IndoorMapEntities
            .Select(x => new {
                positioner = CreatePositionerForIndoorMapEntity(indoorMapEntityInformation.IndoorMapId, x),
                sphere = CreateSphere(Color.red, 1.0f) })
            .ToDictionary(x => x.positioner, x => x.sphere);
    }

    private void OnPositionerTransformedPointChanged(Positioner positioner)
    {
        // A positioner has an updated resultant point, so update visual objects
        var sphere = m_positionerToSpheres[positioner];

        LatLongAltitude resultPointLLA;
        if (positioner.TryGetLatLongAltitude(out resultPointLLA))
        {
            sphere.transform.localPosition = Api.Instance.SpacesApi.GeographicToWorldPoint(resultPointLLA);
            sphere.SetActive(true);
        }
    }

    private Positioner CreatePositionerForIndoorMapEntity(string indoorMapId, IndoorMapEntity indoorMapEntity)
    {
        return Api.Instance.PositionerApi.CreatePositioner(
            new Wrld.Space.Positioners.PositionerOptions()
                .LatitudeDegrees(indoorMapEntity.Position.GetLatitude())
                .LongitudeDegrees(indoorMapEntity.Position.GetLongitude())
                .IndoorMapWithFloorId(indoorMapId, indoorMapEntity.IndoorMapFloorId)
                .ElevationAboveGround(2.0)
                );
    }

    private void DestroyPositionersAndSpheres()
    {
        foreach (var pair in m_positionerToSpheres)
        {
            var positioner = pair.Key;
            positioner.Discard();
            GameObject.Destroy(pair.Value);
        }

    }


    private void OnIndoorMapEntityInformationUpdated(IndoorMapEntityInformation indoorMapEntityInformation)
    {
        var indoorMapEntityIds = string.Join(",", indoorMapEntityInformation.IndoorMapEntities.Select(x => x.IndoorMapEntityId).ToArray());
        Debug.Log(string.Format("IndoorMapEntityInformation for {0}. Load state: {1}; entity count: {2}; entity ids: [{3}].",
            indoorMapEntityInformation.IndoorMapId,
            indoorMapEntityInformation.IndoorMapEntityLoadState,
            indoorMapEntityInformation.IndoorMapEntities.Count,
            indoorMapEntityIds
            ));
        
    }

    private GameObject CreateSphere(Color color, float radius)
    {
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var material = new Material(Shader.Find("Standard"));
        material.color = color;
        sphere.GetComponent<Renderer>().material = material;
        sphere.transform.localScale = Vector3.one * radius;
        sphere.transform.parent = this.transform;
        sphere.SetActive(false);
        return sphere;
    }

}
