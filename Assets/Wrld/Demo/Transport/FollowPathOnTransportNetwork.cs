using System;
using UnityEngine;
using Wrld;
using Wrld.Space;
using Wrld.Transport;

public class FollowPathOnTransportNetwork : MonoBehaviour
{
    class InputSample
    {
        public double LatitudeDegrees;
        public double LongitudeDegrees;
        public double HeadingDegrees;

        public static InputSample Create(double latitudeDegrees, double longitudeDegrees, double heading)
        {
            return new InputSample()
            {
                LatitudeDegrees = latitudeDegrees,
                LongitudeDegrees = longitudeDegrees,
                HeadingDegrees = heading
            };
        }
    };

    public double SamplePeriod = 5.0;

    private readonly InputSample[] m_inputSamples = new InputSample[] {
            InputSample.Create(37.801747, -122.419589, -10.0),
            InputSample.Create(37.802114, -122.418776, 45.0),
            InputSample.Create(37.801477, -122.417845, 170.0),
            InputSample.Create(37.799597, -122.417455, 160.0),
            InputSample.Create(37.799223, -122.418822, 260.0),
            InputSample.Create(37.800270, -122.419288, -10.0)
        };

    private int m_currentInputSampleIndex;
    private TransportPositioner m_transportPositioner;
    private TransportPositionerPointOnGraph m_prevPointOnGraph;
    private TransportPositionerPointOnGraph m_currentPointOnGraph;
    private TransportPathfindResult m_pathfindResult;

    private double m_time;
    private double m_prevSampleTime;
    private double m_currentSampleTime;
    private double m_nextSampleTime;

    private bool m_needPathfind;
    private bool m_needCurrentMatched;

    private GameObject m_capsule;

    private TransportApi m_transportApi;
    private SpacesApi m_spacesApi;

    private void OnEnable()
    {
        m_transportApi = Api.Instance.TransportApi;
        m_spacesApi = Api.Instance.SpacesApi;

        m_currentInputSampleIndex = 0;

        m_time = 0.0;
        m_prevSampleTime = 0.0;
        m_currentSampleTime = 0.0;
        m_nextSampleTime = SamplePeriod;

        m_needCurrentMatched = false;
        m_needPathfind = false;

        m_capsule = CreateCapsule(Color.yellow, 4.0f);
        m_capsule.SetActive(false);

        m_prevPointOnGraph = TransportPositionerPointOnGraph.MakeEmpty();
        m_currentPointOnGraph = TransportPositionerPointOnGraph.MakeEmpty();
        m_pathfindResult = null;

        m_transportPositioner = m_transportApi.CreatePositioner(new TransportPositionerOptionsBuilder()
            .SetInputCoordinates(m_inputSamples[0].LatitudeDegrees, m_inputSamples[0].LongitudeDegrees)
            .Build());
        m_transportPositioner.OnPointOnGraphChanged += OnPointOnGraphChanged;

        SetInputSample();
    }

    private void OnDisable()
    {
        GameObject.Destroy(m_capsule);
        m_transportPositioner.OnPointOnGraphChanged -= OnPointOnGraphChanged;
        m_transportPositioner.Discard();
        m_transportPositioner = null;
        m_prevPointOnGraph = null;
        m_currentPointOnGraph = null;
        m_pathfindResult = null;
    }

    private void Update()
    {
        m_time += Time.deltaTime;
        if (m_time >= m_nextSampleTime)
        {
            NextInput();
        }

        UpdatePointOnPath();
    }


    private void NextInput()
    {
        m_nextSampleTime += SamplePeriod;

        m_currentInputSampleIndex++;
        if (m_currentInputSampleIndex >= m_inputSamples.Length)
        {
            m_currentInputSampleIndex = 0;
        }

        SetInputSample();
    }

    private void SetInputSample()
    {
        var inputSample = m_inputSamples[m_currentInputSampleIndex];
        m_transportPositioner.SetInputCoordinates(inputSample.LatitudeDegrees, inputSample.LongitudeDegrees);
        m_transportPositioner.SetInputHeading(inputSample.HeadingDegrees);
        m_needCurrentMatched = true;
        m_needPathfind = true;
    }

    private void OnPointOnGraphChanged()
    {
        if (m_needCurrentMatched && 
            m_transportPositioner.IsMatched())
        {
            m_prevSampleTime = m_currentSampleTime;
            m_currentSampleTime = m_nextSampleTime;
            m_prevPointOnGraph = new TransportPositionerPointOnGraph(m_currentPointOnGraph);
            m_currentPointOnGraph = m_transportPositioner.GetPointOnGraph();
            m_needCurrentMatched = false;
        }

        if (m_needPathfind &&
            m_currentPointOnGraph.IsMatched &&
            m_prevPointOnGraph.IsMatched)
        {
            var pathfindResult = m_transportApi.FindShortestPath(new TransportPathfindOptionsBuilder()
                .SetPointOnGraphA(m_prevPointOnGraph)
                .SetPointOnGraphB(m_currentPointOnGraph)
                .Build());

            if (pathfindResult.IsPathFound)
            {
                m_needPathfind = false;
                m_pathfindResult = pathfindResult;
            }
        }
    }

    private void UpdatePointOnPath()
    {
        if (m_pathfindResult == null)
        {
            return;
        }

        var parameterizedDistanceAlongPath = CalcParameterizedDistanceAlongPath(m_time, m_prevSampleTime, m_currentSampleTime);
        var pointEcef = m_transportApi.GetPointEcefOnPath(m_pathfindResult, parameterizedDistanceAlongPath);
        var directionEcef = m_transportApi.GetDirectionEcefOnPath(m_pathfindResult, parameterizedDistanceAlongPath);
        var headingDegrees = m_spacesApi.HeadingDegreesFromDirectionAtPoint(directionEcef, pointEcef);

        m_capsule.SetActive(true);
        m_capsule.transform.localPosition = m_spacesApi.GeographicToWorldPoint(LatLongAltitude.FromECEF(pointEcef));
        m_capsule.transform.localEulerAngles = new Vector3(90.0f, (float)headingDegrees, 0.0f);
    }

    private static double CalcParameterizedDistanceAlongPath(double timeNow, double prevSampleTime, double currentSampleTime)
    {
        var delta = currentSampleTime - prevSampleTime;

        if (delta > 0.0)
        {
            return Math.Min(Math.Max((timeNow - prevSampleTime) / delta, 0.0), 1.0);
        }
        else
        {
            return 0.0;
        }
    }

    private GameObject CreateCapsule(Color color, float radius)
    {
        var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        var material = new Material(Shader.Find("Sprites/Default"));
        material.color = color;
        capsule.GetComponent<Renderer>().material = material;
        capsule.transform.localScale = Vector3.one * radius;
        capsule.transform.parent = this.transform;
        return capsule;
    }
}

