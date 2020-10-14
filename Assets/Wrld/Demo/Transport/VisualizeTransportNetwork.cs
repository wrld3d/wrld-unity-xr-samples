using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wrld;
using Wrld.Space;
using Wrld.Transport;

public class VisualizeTransportNetwork : MonoBehaviour
{
    private readonly float m_lineVerticalOffsetMeters = 1.0f;
    private TransportApi m_transportApi;

    private GameObject m_networkWayMeshesRoot;
    private GameObject m_networkLinkMeshesRoot;

    private Dictionary<string, GameObject> m_keyToWayMeshGameObjects;
    private Dictionary<string, GameObject> m_keyToLinkMeshGameObjects;
    

    private void OnEnable()
    {
        m_transportApi = Api.Instance.TransportApi;
        m_keyToWayMeshGameObjects = new Dictionary<string, GameObject>();
        m_keyToLinkMeshGameObjects = new Dictionary<string, GameObject>();
        m_transportApi.OnTransportNetworkCellAdded += OnTransportNetworkCellAdded;
        m_transportApi.OnTransportNetworkCellRemoved += OnTransportNetworkCellRemoved;
        m_transportApi.OnTransportNetworkCellUpdated += OnTransportNetworkCellUpdated;

        m_networkWayMeshesRoot = new GameObject("networkWayMeshesRoot");
        m_networkWayMeshesRoot.transform.parent = this.transform;
        m_networkWayMeshesRoot.transform.localPosition = Vector3.up * m_lineVerticalOffsetMeters;

        m_networkLinkMeshesRoot = new GameObject("networkLinkMeshesRoot");
        m_networkLinkMeshesRoot.transform.parent = this.transform;
        m_networkLinkMeshesRoot.transform.localPosition = Vector3.up * m_lineVerticalOffsetMeters;
    }

    private void OnDisable()
    {
        GameObject.Destroy(m_networkWayMeshesRoot);
        GameObject.Destroy(m_networkLinkMeshesRoot);
        m_keyToWayMeshGameObjects = null;
        m_keyToLinkMeshGameObjects = null;
        m_transportApi.OnTransportNetworkCellAdded -= OnTransportNetworkCellAdded;
        m_transportApi.OnTransportNetworkCellRemoved -= OnTransportNetworkCellRemoved;
        m_transportApi.OnTransportNetworkCellUpdated -= OnTransportNetworkCellUpdated;
    }

    private void OnTransportNetworkCellAdded(TransportNetworkType transportNetwork, TransportCellKey cellKey)
    {
        var objectKey = MakeObjectKey(transportNetwork, cellKey);

        var ways = m_transportApi.GetWaysForNetworkAndCell(transportNetwork, cellKey);
        CreateAndAddWayMeshes(objectKey, ways);

        var directedEdges = m_transportApi.GetDirectedEdgesForNetworkAndCell(transportNetwork, cellKey);
        CreateAndAddLinkMeshes(objectKey, directedEdges);
    }

    private void OnTransportNetworkCellRemoved(TransportNetworkType transportNetwork, TransportCellKey cellKey)
    {
        var objectKey = MakeObjectKey(transportNetwork, cellKey);
        RemoveAndDestroyWayMeshes(objectKey);
        RemoveAndDestroyLinkMeshes(objectKey);
    }

    private void OnTransportNetworkCellUpdated(TransportNetworkType transportNetwork, TransportCellKey cellKey)
    {
        var objectKey = MakeObjectKey(transportNetwork, cellKey);
        RemoveAndDestroyLinkMeshes(objectKey);
        var directedEdges = m_transportApi.GetDirectedEdgesForNetworkAndCell(transportNetwork, cellKey);
        CreateAndAddLinkMeshes(objectKey, directedEdges);
    }

    private IList<Vector3> WayCenterLinePointsToWorldPoints(TransportWay way)
    {
        return way.CenterLinePoints
            .Select(lla => Api.Instance.SpacesApi.GeographicToWorldPoint(LatLongAltitude.FromECEF(lla)))
            .ToList();
    }

    private string MakeObjectKey(TransportNetworkType transportNetwork, TransportCellKey cellKey)
    {
        return transportNetwork.ToString() + "_" + m_transportApi.TransportCellKeyToString(cellKey);
    }


    private GameObject CreateGameObjectForCell(string objectName, Color color)
    {
        var go = new GameObject(objectName);

        var meshRenderer = go.AddComponent<MeshRenderer>();
        var material = new Material(Shader.Find("Sprites/Default"));
        material.color = color;
        
        meshRenderer.material = material;
        meshRenderer.receiveShadows = false;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        go.AddComponent<MeshFilter>();

        return go;
    }

    private void CreateAndAddWayMeshes(string objectKey, IList<TransportWay> ways)
    {
        var waysGameObject = CreateGameObjectForCell(objectKey, Color.cyan);
        waysGameObject.transform.SetParent(m_networkWayMeshesRoot.transform, false);
        m_keyToWayMeshGameObjects.Add(objectKey, waysGameObject);

        var waysMeshFilter = waysGameObject.GetComponent<MeshFilter>();
        waysMeshFilter.mesh = CreateMeshForWays(ways);
    }

    private void RemoveAndDestroyWayMeshes(string objectKey)
    {
        if (m_keyToWayMeshGameObjects.ContainsKey(objectKey))
        {
            var go = m_keyToWayMeshGameObjects[objectKey];
            go.transform.parent = null;
            m_keyToWayMeshGameObjects.Remove(objectKey);
            Destroy(go);
        }
    }

    private void CreateAndAddLinkMeshes(string objectKey, IList<TransportDirectedEdge> directedEdges)
    {
        var linksGameObject = CreateGameObjectForCell(objectKey, Color.yellow);
        linksGameObject.transform.SetParent(m_networkLinkMeshesRoot.transform, false);
        m_keyToLinkMeshGameObjects.Add(objectKey, linksGameObject);

        var linksMeshFilter = linksGameObject.GetComponent<MeshFilter>();
        linksMeshFilter.mesh = CreateMeshForLinks(directedEdges);
    }

    private void RemoveAndDestroyLinkMeshes(string objectKey)
    {
        if (m_keyToLinkMeshGameObjects.ContainsKey(objectKey))
        {
            var go = m_keyToLinkMeshGameObjects[objectKey];
            go.transform.parent = null;
            m_keyToLinkMeshGameObjects.Remove(objectKey);
            Destroy(go);
        }
    }


    private Mesh CreateMeshForWays(IList<TransportWay> ways)
    {
        Mesh mesh = new Mesh();
        if (!ways.Any())
        {
            return mesh;
        }

        var vertices = new List<Vector3>();
        var indices = new List<int>();
        foreach (var way in ways)
        {
            var points = WayCenterLinePointsToWorldPoints(way);
            indices.AddRange(BuildLineSegmentIndices(vertices.Count, points.Count - 1));
            vertices.AddRange(points);
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
        mesh.RecalculateBounds();
        return mesh;
    }

    private Mesh CreateMeshForLinks(IList<TransportDirectedEdge> directedEdges)
    {
        Mesh mesh = new Mesh();
        if (!directedEdges.Any())
        {
            return mesh;
        }

        var vertices = new List<Vector3>();
        var indices = new List<int>();

        const float unlinkedCellBoundaryIndicatorLength = 50.0f;
        const float nodeIndicatorLength = 5.0f;

        foreach (var directedEdge in directedEdges)
        {
            bool isUnlinkedCellBoundary = (directedEdge.NodeIdB.LocalNodeId < 0);
            var indicatorLength = isUnlinkedCellBoundary ? unlinkedCellBoundaryIndicatorLength : nodeIndicatorLength;

            TransportNode node;
            if (m_transportApi.TryGetNode(directedEdge.NodeIdA, out node))
            {
                var pointA = Api.Instance.SpacesApi.GeographicToWorldPoint(LatLongAltitude.FromECEF(node.Point));

                indices.Add(vertices.Count);
                indices.Add(vertices.Count + 1);
                vertices.Add(pointA);
                vertices.Add(pointA + Vector3.up*indicatorLength);
            }
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
        mesh.RecalculateBounds();
        return mesh;
    }

    private int[] BuildLineSegmentIndices(int indexOffset, int lineSegmentCount)
    {
        var count = lineSegmentCount * 2;
        var indices = new int[count];

        int j = 0;
        for (int i = 0; i < lineSegmentCount; ++i)
        {
            indices[j++] = indexOffset + i;
            indices[j++] = indexOffset + i + 1;
        }
        return indices;
    }
}

