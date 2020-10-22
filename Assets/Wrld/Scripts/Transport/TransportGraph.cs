using System.Collections.Generic;
using System.Linq;

namespace Wrld.Transport
{
    /// <summary>
    /// A helper class that responds to TransportApi events in order to maintain the set of all 
    /// currently-resident TransportNode, TransportDirectedEdge and TransportWay objects for a 
    /// transport network.
    /// </summary>
    public class TransportGraph
    {
        /// <summary>
        ///  The transport network to observe.
        /// </summary>
        public TransportNetworkType NetworkType { get; private set; }

        /// <summary>
        /// A delegate type for event handlers receiving notification that a the values of this TransportGraph object
        /// have changed due to transport network streaming.
        /// </summary>
        /// <param name="graph">This instance.</param>
        /// <param name="cellKey">The key of the cell affected by streaming changes.</param>
        public delegate void TransportGraphChangedHandler(TransportGraph graph, TransportCellKey cellKey);

        /// <summary>
        /// Notification that the contents of this object have been mutated due to transport graph streaming.
        /// </summary>
        public event TransportGraphChangedHandler OnTransportGraphChanged;

        /// <summary>
        /// A collection of key/value pairs for TransportNode objects.
        /// </summary>
        public IDictionary<TransportNodeId, TransportNode> Nodes {  get { return m_nodes; } }

        /// <summary>
        /// A collection of key/value pairs for TransportDirectedEdge objects.
        /// </summary>
        public IDictionary<TransportDirectedEdgeId, TransportDirectedEdge> DirectedEdges {  get { return m_directedEdges; } }

        /// <summary>
        /// A collection of key/value pairs for TransportWay objects.
        /// </summary>
        public IDictionary<TransportWayId, TransportWay> Ways {  get { return m_ways; } }

        private readonly TransportApi m_transportApi;
        private IDictionary<TransportNodeId, TransportNode> m_nodes = new Dictionary<TransportNodeId, TransportNode>();
        private IDictionary<TransportDirectedEdgeId, TransportDirectedEdge> m_directedEdges = new Dictionary<TransportDirectedEdgeId, TransportDirectedEdge>();
        private IDictionary<TransportWayId, TransportWay> m_ways = new Dictionary<TransportWayId, TransportWay>();

        public TransportGraph(
            TransportNetworkType networkType,
            TransportApi transportApi
            )
        {
            this.NetworkType = networkType;
            this.m_transportApi = transportApi;            

            m_transportApi.OnTransportNetworkCellAdded += OnTransportNetworkCellAdded;
            m_transportApi.OnTransportNetworkCellRemoved += OnTransportNetworkCellRemoved;
            m_transportApi.OnTransportNetworkCellUpdated += OnTransportNetworkCellUpdated;
        }

        private void OnTransportNetworkCellAdded(TransportNetworkType networkType, TransportCellKey cellKey)
        {
            if (networkType != NetworkType)
            {
                return;
            }
            var nodeIds = m_transportApi.GetNodeIdsForNetworkInCell(networkType, cellKey);
            var directedEdgeIds = m_transportApi.GetDirectedEdgeIdsForNetworkInCell(networkType, cellKey);
            var wayIds = m_transportApi.GetWayIdsForNetworkInCell(networkType, cellKey);

            AddForCell(cellKey, nodeIds, directedEdgeIds, wayIds);
        }

        private void OnTransportNetworkCellRemoved(TransportNetworkType networkType, TransportCellKey cellKey)
        {
            if (networkType != NetworkType)
            {
                return;
            }

            RemoveForCell(cellKey);
        }

        private void OnTransportNetworkCellUpdated(TransportNetworkType networkType, TransportCellKey cellKey)
        {
            if (networkType != NetworkType)
            {
                return;
            }

            var directedEdgeIds = m_transportApi.GetDirectedEdgeIdsForNetworkInCell(networkType, cellKey);
            UpdateForCell(cellKey, directedEdgeIds);
        }

        private void AddForCell(
            TransportCellKey cellKey,
            IList<TransportNodeId> nodeIds,
            IList<TransportDirectedEdgeId> directedEdgeIds,
            IList<TransportWayId> wayIds
            )
        {
            foreach (var nodeId in nodeIds)
            {
                TransportNode node;
                if (m_transportApi.TryGetNode(nodeId, out node))
                {
                    m_nodes.Add(node.Id, node);
                }
            }

            foreach (var directedEdgeId in directedEdgeIds)
            {
                TransportDirectedEdge directedEdge;
                if (m_transportApi.TryGetDirectedEdge(directedEdgeId, out directedEdge))
                {
                    m_directedEdges.Add(directedEdge.Id, directedEdge);
                }
            }

            foreach (var wayId in wayIds)
            {
                TransportWay way;
                if (m_transportApi.TryGetWay(wayId, out way))
                {
                    m_ways.Add(way.Id, way);
                }
            }

            if (OnTransportGraphChanged != null)
            {
                OnTransportGraphChanged(this, cellKey);
            }
        }

        private void RemoveForCell(TransportCellKey cellKey)
        {
            var nodeIds = m_nodes.Keys.Where(_key => (_key.CellKey.Value == cellKey.Value)).ToList();
            var directedEdgesIds = m_directedEdges.Keys.Where(_key => (_key.CellKey.Value == cellKey.Value)).ToList();
            var wayIds = m_ways.Keys.Where(_key => (_key.CellKey.Value == cellKey.Value)).ToList();

            foreach (var nodeId in nodeIds)
            {
                m_nodes.Remove(nodeId);
            }

            foreach (var directedEdgeId in directedEdgesIds)
            {
                m_directedEdges.Remove(directedEdgeId);
            }

            foreach (var wayId in wayIds)
            {
                m_ways.Remove(wayId);
            }

            if (OnTransportGraphChanged != null)
            {
                OnTransportGraphChanged(this, cellKey);
            }
        }

        private void UpdateForCell(
            TransportCellKey cellKey,
            IList<TransportDirectedEdgeId> directedEdgeIds
            )
        {
            foreach (var directedEdgeId in directedEdgeIds)
            {
                TransportDirectedEdge directedEdge;
                if (m_transportApi.TryGetDirectedEdge(directedEdgeId, out directedEdge))
                {
                    m_directedEdges[directedEdge.Id] = directedEdge;
                }
            }

            if (OnTransportGraphChanged != null)
            {
                OnTransportGraphChanged(this, cellKey);
            }
        }
    }
}
