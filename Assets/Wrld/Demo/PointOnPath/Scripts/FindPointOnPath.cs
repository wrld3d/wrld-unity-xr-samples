using Wrld;
using Wrld.Space;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Wrld.Space.Positioners;

public class FindPointOnPath : MonoBehaviour
{
    private readonly List<LatLong> m_pathPoints = new List<LatLong>()
        {
            LatLong.FromDegrees(56.459866, -2.980015),
            LatLong.FromDegrees(56.459727, -2.979299),
            LatLong.FromDegrees(56.461067, -2.979750),
            LatLong.FromDegrees(56.461017, -2.980325),
        };

    private readonly LatLong m_queryPoint = LatLong.FromDegrees(56.460624, -2.978936);
    private LatLong m_resultPoint;

    private List<Positioner> m_positionersForPathPoints;
    private Positioner m_positionerForQueryPoint;
    private Positioner m_positionerForResultPoint;


    private LineRenderer m_pathLineRenderer;
    private GameObject m_sphereInput;
    private GameObject m_sphereOutput;


    void OnEnable()
    {
        // hook to receive change notification on resulant transform of positioners
        Api.Instance.PositionerApi.OnPositionerTransformedPointChanged += OnPositionerTransformedPointChanged;

        // get information about the closest point on a path to query point
        var pointOnPath = Api.Instance.PathApi.GetPointOnPath(m_queryPoint, m_pathPoints);
        m_resultPoint = pointOnPath.ResultPoint;

        CreateLineRendererGameObject();

        m_sphereInput = CreateSphere(Color.red, 5.0f);
        m_sphereOutput = CreateSphere(Color.green, 5.0f);

        // create Positioner instances for each path vertex and query/result point, to position at
        // height relative to ground
        m_positionersForPathPoints = m_pathPoints
            .Select(x => CreatePositionerForPoint(x))
            .ToList();

        m_positionerForQueryPoint = CreatePositionerForPoint(m_queryPoint);
        m_positionerForResultPoint = CreatePositionerForPoint(m_resultPoint);
    }

    private void OnPositionerTransformedPointChanged(Positioner positioner)
    {
        // A positioner has an updated resultant point, so update visual objects
        UpdateVisualization();
    }

    private Positioner CreatePositionerForPoint(LatLong point)
    {
        var options = new PositionerOptions()
            .LatitudeDegrees(point.GetLatitude())
            .LongitudeDegrees(point.GetLongitude())
            .ElevationAboveGround(1.0);
        return Api.Instance.PositionerApi.CreatePositioner(options);
    }

    private void UpdateVisualization()
    {
        if (
            m_positionerForQueryPoint == null ||
            m_positionerForResultPoint == null ||
            m_positionersForPathPoints == null ||
            m_pathPoints.Count != m_positionersForPathPoints.Count)
        {
            return;
        }


        var points = new List<Vector3>();

        foreach (var positioner in m_positionersForPathPoints)
        {
            LatLongAltitude pointLLA;
            if (!positioner.TryGetLatLongAltitude(out pointLLA))
            {
                continue;
            }

            var worldPoint = Api.Instance.SpacesApi.GeographicToWorldPoint(pointLLA);
            points.Add(worldPoint);
        }

#if UNITY_5_6_OR_NEWER
        m_pathLineRenderer.positionCount = points.Count;
#else
        m_pathLineRenderer.numPositions = points.Count;
#endif
        m_pathLineRenderer.SetPositions(points.ToArray());



        LatLongAltitude queryPointLLA;
        if (m_positionerForQueryPoint.TryGetLatLongAltitude(out queryPointLLA))
        {
            m_sphereInput.transform.localPosition = Api.Instance.SpacesApi.GeographicToWorldPoint(queryPointLLA);
            m_sphereInput.SetActive(true);
        }

        LatLongAltitude resultPointLLA;
        if (m_positionerForResultPoint.TryGetLatLongAltitude(out resultPointLLA))
        {
            m_sphereOutput.transform.localPosition = Api.Instance.SpacesApi.GeographicToWorldPoint(resultPointLLA);
            m_sphereOutput.SetActive(true);
        }
    }

    private void OnDisable()
    {
        GameObject.Destroy(m_pathLineRenderer);
        GameObject.Destroy(m_sphereInput);
        GameObject.Destroy(m_sphereOutput);

        Api.Instance.PositionerApi.OnPositionerTransformedPointChanged -= OnPositionerTransformedPointChanged;

        foreach (var positioner in m_positionersForPathPoints)
        {
            positioner.Discard();
        }

        if (m_positionerForQueryPoint != null)
        {
            m_positionerForQueryPoint.Discard();
        }

        if (m_positionerForResultPoint != null)
        {
            m_positionerForResultPoint.Discard();
        }

        m_positionersForPathPoints = null;
        m_positionerForQueryPoint = null;
        m_positionerForResultPoint = null;
    }

    private void CreateLineRendererGameObject()
    {
        m_pathLineRenderer = gameObject.AddComponent<LineRenderer>();
        m_pathLineRenderer.useWorldSpace = true;
        m_pathLineRenderer.startColor = Color.red;
        m_pathLineRenderer.endColor = Color.red;
        m_pathLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        m_pathLineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        m_pathLineRenderer.receiveShadows = false;
    }

    private GameObject CreateSphere(Color color, float radius)
    {
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var material = new Material(Shader.Find("Sprites/Default"));
        material.color = color;
        sphere.GetComponent<Renderer>().material = material;
        sphere.transform.localScale = Vector3.one * radius;
        sphere.transform.parent = this.transform;
        sphere.SetActive(false);
        return sphere;
    }

}