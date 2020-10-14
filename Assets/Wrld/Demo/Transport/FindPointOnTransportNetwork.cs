using System.Collections;
using UnityEngine;
using Wrld;
using Wrld.Common.Maths;
using Wrld.Space;
using Wrld.Transport;

public class FindPointOnTransportNetwork : MonoBehaviour
{
    private readonly LatLongAltitude m_inputCoords = LatLongAltitude.FromDegrees(37.784468, -122.401268, 10.0);
    private float m_inputHeadingDegreesA = 225.0f;
    private float m_inputHeadingDegreesB = 300.0f;
    private bool m_isHeadingA;
    private TransportPositioner m_transportPositioner;
    GameObject m_sphereInput;
    GameObject m_sphereOutput;
    GameObject m_directionIndicatorInput;
    GameObject m_directionIndicatorOutput;

    private void OnEnable()
    {
        CreateVisualizationObjects();

        var options = new TransportPositionerOptionsBuilder()
            .SetInputCoordinates(m_inputCoords.GetLatitude(), m_inputCoords.GetLongitude())
            .SetInputHeading(GetCurrentInputHeading())
            .Build();

        m_transportPositioner = Api.Instance.TransportApi.CreatePositioner(options);
        m_transportPositioner.OnPointOnGraphChanged += OnPointOnGraphChanged;

        StartCoroutine(ToggleInputHeading());
    }

    private void OnDisable()
    {
        GameObject.Destroy(m_sphereInput);
        GameObject.Destroy(m_sphereOutput);
        GameObject.Destroy(m_directionIndicatorInput);
        GameObject.Destroy(m_directionIndicatorOutput);

        m_transportPositioner.OnPointOnGraphChanged -= OnPointOnGraphChanged;
        m_transportPositioner.Discard();
    }

    IEnumerator ToggleInputHeading()
    {
        while (enabled)
        {
            m_isHeadingA = !m_isHeadingA;
            m_directionIndicatorInput.transform.eulerAngles = new Vector3(0.0f, GetCurrentInputHeading(), 0.0f);

            m_transportPositioner.SetInputHeading(GetCurrentInputHeading());
            yield return new WaitForSeconds(5);
        }        
    }

    private float GetCurrentInputHeading()
    {
        return m_isHeadingA ? m_inputHeadingDegreesA : m_inputHeadingDegreesB;
    }

    private void OnPointOnGraphChanged()
    {
        if (m_transportPositioner.IsMatched())
        {
            var pointOnGraph = m_transportPositioner.GetPointOnGraph();

            var outputLLA = LatLongAltitude.FromECEF(pointOnGraph.PointOnWay);
            const double verticalOffset = 1.0;
            outputLLA.SetAltitude(outputLLA.GetAltitude() + verticalOffset);
            var outputPosition = Api.Instance.SpacesApi.GeographicToWorldPoint(outputLLA);

            m_sphereOutput.transform.position = outputPosition;
            m_directionIndicatorOutput.transform.localPosition = outputPosition;
            m_directionIndicatorOutput.transform.eulerAngles = new Vector3(0.0f, (float)pointOnGraph.HeadingOnWayDegrees, 0.0f);

            m_sphereOutput.SetActive(true);
            m_directionIndicatorOutput.SetActive(true);
        }
    }

    private void CreateVisualizationObjects()
    {
        var inputPosition = Api.Instance.SpacesApi.GeographicToWorldPoint(m_inputCoords);
        m_sphereInput = CreateSphere(Color.red, 2.0f);
        m_sphereInput.transform.localPosition = inputPosition;

        m_directionIndicatorInput = CreateDirectionIndicator(Color.red, 5.0f);
        m_directionIndicatorInput.transform.localPosition = inputPosition;

        m_directionIndicatorOutput = CreateDirectionIndicator(Color.green, 5.0f);

        m_sphereOutput = CreateSphere(Color.green, 2.0f);
        m_sphereOutput.transform.localPosition = inputPosition;

        m_sphereOutput.SetActive(false);
        m_directionIndicatorOutput.SetActive(false);
    }


    private GameObject CreateSphere(Color color, float radius)
    {
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var material = new Material(Shader.Find("Sprites/Default"));
        material.color = color;
        sphere.GetComponent<Renderer>().material = material;
        sphere.transform.localScale = Vector3.one * radius;
        sphere.transform.parent = this.transform;
        return sphere;
    }

    private GameObject CreateDirectionIndicator(Color color, float length)
    {
        var gameObject = new GameObject("Direction Indicator");
        gameObject.transform.parent = this.transform;

        var lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = 1.0f;
        lineRenderer.endWidth = 0.0f;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
#if UNITY_5_6_OR_NEWER
        lineRenderer.positionCount = 2;
#else
        lineRenderer.numPositions = 2;
#endif

        lineRenderer.SetPositions(new Vector3[] { Vector3.zero, Vector3.forward* length });

        return gameObject;
    }
}

