using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wrld;
using Wrld.Space;
using Wrld.Transport;

public class FindPathOnTransportNetwork : MonoBehaviour
{
    private readonly LatLong m_inputPositionA = LatLong.FromDegrees(37.802345, -122.419721);
    private readonly LatLong m_inputPositionB = LatLong.FromDegrees(37.802311, -122.417294);
    private readonly float m_lineVerticalOffsetMeters = 1.0f;

    private TransportPositioner m_transportPositionerA;
    private TransportPositioner m_transportPositionerB;
    private LineRenderer m_pathLineRenderer;

    private bool m_foundPath = false;

    private void OnEnable()
    {
        CreateLineRendererGameObject();

        m_transportPositionerA = CreatePositioner(m_inputPositionA);
        m_transportPositionerB = CreatePositioner(m_inputPositionB);
        m_transportPositionerA.OnPointOnGraphChanged += OnPointOnGraphChanged;
        m_transportPositionerB.OnPointOnGraphChanged += OnPointOnGraphChanged;
    }

    private void OnDisable()
    {
        GameObject.Destroy(m_pathLineRenderer);
        m_transportPositionerA.OnPointOnGraphChanged -= OnPointOnGraphChanged;
        m_transportPositionerA.Discard();

        m_transportPositionerB.OnPointOnGraphChanged -= OnPointOnGraphChanged;
        m_transportPositionerB.Discard();
    }

    private TransportPositioner CreatePositioner(LatLong location)
    {
        var options = new TransportPositionerOptionsBuilder()
            .SetInputCoordinates(location.GetLatitude(), location.GetLongitude())
            .Build();

        return Api.Instance.TransportApi.CreatePositioner(options);
    }

    private void OnPointOnGraphChanged()
    {
        if (m_foundPath)
        {
            return;
        }

        if (!m_transportPositionerA.IsMatched() || !m_transportPositionerB.IsMatched())
        {
            return;
        }

        var pathfindOptions = new TransportPathfindOptionsBuilder()
            .SetPointOnGraphA(m_transportPositionerA.GetPointOnGraph())
            .SetPointOnGraphB(m_transportPositionerB.GetPointOnGraph())
            .Build();

        var pathfindResult = Api.Instance.TransportApi.FindShortestPath(pathfindOptions);

        if (pathfindResult.IsPathFound)
        {
            m_foundPath = true;

            CreatePathLines(pathfindResult);

            var pathResultDirectedEdges = BuildPathResultDirectedEdges(pathfindResult);
            var pathResultWays = BuildPathResultWays(pathResultDirectedEdges);
            var pathResultNodes = BuildPathResultNodes(pathResultDirectedEdges);
            LogPathInformation(m_inputPositionA, m_inputPositionB, pathfindResult, pathResultWays, pathResultNodes);
        }
        
    }

    private void CreatePathLines(TransportPathfindResult result)
    {
        var points = new List<Vector3>();

        foreach(var pointEcef in result.PathPoints)
        {
            var lla = LatLongAltitude.FromECEF(pointEcef);
            lla.SetAltitude(lla.GetAltitude() + m_lineVerticalOffsetMeters);
            var worldPoint = Api.Instance.SpacesApi.GeographicToWorldPoint(lla);
            points.Add(worldPoint);
        }

#if UNITY_5_6_OR_NEWER
        m_pathLineRenderer.positionCount = points.Count;
#else
        m_pathLineRenderer.numPositions = points.Count;
#endif
        m_pathLineRenderer.SetPositions(points.ToArray());
    }

    
    private static List<TransportDirectedEdge> BuildPathResultDirectedEdges(TransportPathfindResult pathfindResult)
    {
        var directedEdges = new List<TransportDirectedEdge>();
        foreach (var directedEdgeId in pathfindResult.PathDirectedEdgeIds)
        {
            TransportDirectedEdge directedEdge;
            if (!Api.Instance.TransportApi.TryGetDirectedEdge(directedEdgeId, out directedEdge))
            {
                throw new System.ArgumentOutOfRangeException("unable to fetch TransportDirectedEdge");
            }
            directedEdges.Add(directedEdge);
        }
        return directedEdges;
    }

    private static List<TransportWay> BuildPathResultWays(IList<TransportDirectedEdge> directedEdges)
    { 
        var ways = new List<TransportWay>();
        foreach (var directedEdge in directedEdges)
        {
            TransportWay way;
            if (!Api.Instance.TransportApi.TryGetWay(directedEdge.WayId, out way))
            {
                throw new System.ArgumentOutOfRangeException("unable to fetch TransportWay");
            }
            ways.Add(way);
        }
        return ways;
    }

    private static List<TransportNode> BuildPathResultNodes(IList<TransportDirectedEdge> directedEdges)
    {
        var nodes = new List<TransportNode>();

        if (directedEdges.Any())
        {
            TransportNode node;
            if (!Api.Instance.TransportApi.TryGetNode(directedEdges.First().NodeIdA, out node))
            {
                throw new System.ArgumentOutOfRangeException("unable to fetch TransportNode");
            }
            nodes.Add(node);
        }

        foreach (var directedEdge in directedEdges)
        {
            TransportNode node;
            if (!Api.Instance.TransportApi.TryGetNode(directedEdge.NodeIdB, out node))
            {
                throw new System.ArgumentOutOfRangeException("unable to fetch TransportNode");
            }
            nodes.Add(node);
        }


        return nodes;
    }

    private static void LogPathInformation(
        LatLong inputPositionA, 
        LatLong inputPositionB, 
        TransportPathfindResult pathfindResult, 
        List<TransportWay> pathResultWays,
        List<TransportNode> pathResultNodes)
    {
        Debug.Log(string.Format("Found path from {0} to {1}: distance {2:0.00}m",
            inputPositionA.ToString(),
            inputPositionB.ToString(),
            pathfindResult.DistanceMeters));

        foreach (var way in pathResultWays)
        {
            Debug.Log(string.Format("Way id [{0}] has classification {1}, length {2:0.00}m, width {3:0.00}m",
                Api.Instance.TransportApi.WayIdToString(way.Id),
                way.Classification,
                way.LengthMeters,
                way.HalfWidthMeters * 2
                ));
        }

        foreach (var node in pathResultNodes)
        {
            Debug.Log(string.Format("Node id [{0}] at {1} has {2} incident edges",
                Api.Instance.TransportApi.NodeIdToString(node.Id),
                LatLongAltitude.FromECEF(node.Point).ToString(),
                node.IncidentDirectedEdges.Count
                ));

        }

    }

    private void CreateLineRendererGameObject()
    {
        m_pathLineRenderer = gameObject.AddComponent<LineRenderer>();
        m_pathLineRenderer.useWorldSpace = true;
        m_pathLineRenderer.startColor = Color.green;
        m_pathLineRenderer.endColor = Color.red;
        m_pathLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        m_pathLineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        m_pathLineRenderer.receiveShadows = false;
    }
}

