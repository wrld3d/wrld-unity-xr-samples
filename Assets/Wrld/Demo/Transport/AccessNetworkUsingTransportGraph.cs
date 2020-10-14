using UnityEngine;
using Wrld;
using Wrld.Transport;



public class AccessNetworkUsingTransportGraph : MonoBehaviour
{
    private TransportApi m_transportApi;
    private TransportNetworkType m_transportNetwork = TransportNetworkType.Road;
    private TransportGraph m_roadsTransportGraph;

    private void OnEnable()
    {
        m_transportApi = Api.Instance.TransportApi;
        m_roadsTransportGraph = m_transportApi.CreateTransportGraph(m_transportNetwork);

        m_roadsTransportGraph.OnTransportGraphChanged += OnTransportGraphChanged;
    }
    private void OnDisable()
    {
        m_roadsTransportGraph.OnTransportGraphChanged -= OnTransportGraphChanged;
    }

    private void OnTransportGraphChanged(TransportGraph graph, TransportCellKey cellKey)
    {
        var cellKeyString = m_transportApi.TransportCellKeyToString(cellKey);
        Debug.Log(
            string.Format("TransportGraph had cell {0} changed. It now has: {1} nodes, {2} directedEdges, {3} ways", 
                cellKeyString,
                graph.Nodes.Count, 
                graph.DirectedEdges.Count, 
                graph.Ways.Count));
    }
}

