using System.Collections.Generic;
using Wrld.Common.Maths;

namespace Wrld.Transport
{
    /// <summary>
    /// Api for accessing streamed transportation networks.
    /// Separate transport networks exist for each of the TransportNetworkType enumerator list - for example, roads 
    /// exist on a separate network to rail. 
    /// Transport networks are streamed in and out in a similar manner to visual map resources, using a tiled approach. 
    /// </summary>
    public class TransportApi
    {
        /// <summary>
        /// A delegate type for event handlers receiving notification that a transport network has changed.
        /// </summary>
        /// <param name="networkType">The transport network of the changed cell.</param>
        /// <param name="cellKey">The key of the changed cell.</param>
        public delegate void TransportNetworkChangedHandler(TransportNetworkType networkType, TransportCellKey cellKey);

        /// <summary>
        /// Raised when a transport network has changed due to a new cell being streamed in. New TransportNode, TransportDirectedEdge and TransportWay objects
        /// that belong to that cell and network may now be present, and can be queried via TransportApi methods, for example via TransportApi.GetNodeIdsForNetworkInCell().
        /// </summary>
        public event TransportNetworkChangedHandler OnTransportNetworkCellAdded;

        /// <summary>
        /// Raised when a transport network has changed due to a cell being removed after no longer intersecting the 
        /// streaming camera's frustum. TransportNode, TransportDirectedEdge and TransportWay objects belonging to that
        /// cell and network will be removed.
        /// </summary>
        public event TransportNetworkChangedHandler OnTransportNetworkCellRemoved;

        /// <summary>
        /// Raised when a transport network cell has changed as connections are formed or disconnected due to adjacent portions of the graph streaming in or out.
        /// TransportDirectedEdge objects belonging to the cell may change, but TransportNode and TransportWay objects remain unchanged.
        /// </summary>
        public event TransportNetworkChangedHandler OnTransportNetworkCellUpdated;

        private TransportApiInternal m_apiInternal;

        internal TransportApi(
            TransportApiInternal apiInternal
            )
        {
            m_apiInternal = apiInternal;

            m_apiInternal.OnTransportNetworkCellAdded += (networkType, cellKey) => RaiseEvent(OnTransportNetworkCellAdded, networkType, cellKey);
            m_apiInternal.OnTransportNetworkCellRemoved += (networkType, cellKey) => RaiseEvent(OnTransportNetworkCellRemoved, networkType, cellKey);
            m_apiInternal.OnTransportNetworkCellUpdated += (networkType, cellKey) => RaiseEvent(OnTransportNetworkCellUpdated, networkType, cellKey);
        }

        /// <summary>
        /// Creates an instance of a TransportPositioner.
        /// </summary>
        /// <param name="options">A TransportPositionerOptions object that provides creation parameters for this TransportPositioner. 
        /// TransportPositionerOptionsBuilder may be used to help construct an appropriate options object.</param>
        public TransportPositioner CreatePositioner(TransportPositionerOptions options)
        {
            return m_apiInternal.CreatePositioner(options);
        }

        /// <summary>
        /// Find the shortest path, if any, between two points on a transport network as specified in options.
        /// Note that the shortest path from point A to point B is not necessarily the reverse of the shortest
        /// path from point B to point A, as some TransportWays may only be traversed in one direction.
        /// The method considers the currently streamed-in set of the transport network only. As such, it 
        /// is provided primarily as a convenience for connecting relatively nearby points for visualization 
        /// purposes - it is not intended to be useful for large-scale route planning.
        /// </summary>
        /// <param name="options">A TransportPathfindOptions object that provides input parameters. 
        /// TransportPathfindOptionsBuilder may be used to help construct an appropriate options object.</param>
        /// <returns>A result object, indicating whether a path was found, and providing the path if successful.</returns>
        public TransportPathfindResult FindShortestPath(TransportPathfindOptions options)
        {
            return m_apiInternal.FindShortestPath(options);
        }

        #region Transport Network element access
        /// <summary>
        /// Get the TransportNode value associated with the given TransportNodeId key.
        /// </summary>
        /// <param name="nodeId">The id of the TransportNode to get.</param>
        /// <param name="node">On return, contains the value associated with nodeId if found; else an empty value 
        /// as returned by TransportNode.MakeEmpty().</param>
        /// <returns>True if a TransportNode object with Id equal to nodeId was found, else false.</returns>
        public bool TryGetNode(TransportNodeId nodeId, out TransportNode node)
        {
            return m_apiInternal.TryGetNode(nodeId, out node);
        }

        /// <summary>
        /// The TransportDirectedEdge value associated with the given TransportDirectedEdgeId key.
        /// </summary>
        /// <param name="directedEdgeId">The id of the TransportDirectedEdge to get.</param>
        /// <param name="directedEdge">On return, contains the value associated with directedEdgeId if found; else 
        /// an empty value as returned by DirectedEdge.MakeEmpty().</param>
        /// <returns>True if a TransportDirectedEdge object with Id equal to directedEdgeId was found, else false.</returns>
        public bool TryGetDirectedEdge(TransportDirectedEdgeId directedEdgeId, out TransportDirectedEdge directedEdge)
        {
            return m_apiInternal.TryGetDirectedEdge(directedEdgeId, out directedEdge);
        }

        /// <summary>
        /// The TransportWay value associated with the given TransportWayId key.
        /// </summary>
        /// <param name="wayId">The id of the TransportWay to get.</param>
        /// <param name="way">On return, contains the value associated with transportWayId if found; else an empty
        /// value as returned by TransportWay.MakeEmpty().</param>
        /// <returns>True if a TransportWay object with Id equal to wayId was found, else false.</returns>
        public bool TryGetWay(TransportWayId wayId, out TransportWay way)
        {
            return m_apiInternal.TryGetWay(wayId, out way);
        }
        #endregion

        #region Transport Network element Id access
        /// <summary>
        /// Get a collection of TransportNodeId keys for all TransportNode objects currently streamed in for the given transport network.
        /// </summary>
        /// <param name="transportNetwork">The returned results are filtered to contain only ids for nodes belonging to this transport network.</param>
        /// <returns>An unordered list of TransportNodeId objects.</returns>
        public IList<TransportNodeId> GetNodeIdsForNetwork(TransportNetworkType transportNetwork)
        {
            return m_apiInternal.GetNodeIdsForNetwork(transportNetwork);
        }

        /// <summary>
        /// Get a collection of TransportDirectedEdgeId keys for all TransportDirectedEdge objects currently streamed in for the given transport network.
        /// </summary>
        /// <param name="transportNetwork">The returned results are filtered to contain only ids for directed edges belonging to this transport network.</param>
        /// <returns>An unordered list of TransportDirectedEdgeId objects.</returns>
        public IList<TransportDirectedEdgeId> GetDirectedEdgeIdsForNetwork(TransportNetworkType transportNetwork)
        {
            return m_apiInternal.GetDirectedEdgeIdsForNetwork(transportNetwork);
        }

        /// <summary>
        /// Get a collection of TransportWayId keys for all TransportWay objects currently streamed in for the given transport network.
        /// </summary>
        /// <param name="transportNetwork">The returned results are filtered to contain only ids for ways belonging to this transport network.</param>
        /// <returns>An unordered list of TransportNodeId objects.</returns>
        public IList<TransportWayId> GetWayIdsForNetwork(TransportNetworkType transportNetwork)
        {
            return m_apiInternal.GetWayIdsForNetwork(transportNetwork);
        }

        /// <summary>
        /// Get a collection of TransportNodeId keys for all TransportNode objects currently streamed in for the given transport network and cell.
        /// </summary>
        /// <param name="transportNetwork">The returned results are filtered to contain only ids for nodes belonging to this transport network.</param>
        /// <param name="cellKey">The returned results are filtered to contain only ids for nodes belonging to this cell.</param>
        /// <returns>An unordered list of the id of all TransportNode objects currently resident, filtered by transportNetwork and cellKey.</returns>
        public IList<TransportNodeId> GetNodeIdsForNetworkInCell(TransportNetworkType transportNetwork, TransportCellKey cellKey)
        {
            return m_apiInternal.GetNodeIdsForNetworkInCell(transportNetwork, cellKey);
        }

        /// <summary>
        /// Get a collection of TransportDirectedEdgeId keys for all TransportDirectedEdge objects currently streamed in for the given transport network.
        /// </summary>
        /// <param name="transportNetwork">The returned results are filtered to contain only ids for directed edges belonging to this transport network.</param>
        /// <param name="cellKey">The returned results are filtered to contain only ids for directed edges  belonging to this cell.</param>
        /// <returns>An unordered list of the id of all TransportDirectedEdge objects currently resident, filtered by transportNetwork and cellKey.</returns>
        public IList<TransportDirectedEdgeId> GetDirectedEdgeIdsForNetworkInCell(TransportNetworkType transportNetwork, TransportCellKey cellKey)
        {
            return m_apiInternal.GetDirectedEdgeIdsForNetworkInCell(transportNetwork, cellKey);
        }

        /// <summary>
        /// Get a collection of TransportWayId keys for all TransportWay objects currently streamed in for the given transport network and cell.
        /// </summary>
        /// <param name="transportNetwork">The returned results are filtered to contain only ids for ways belonging to this transport network.</param>
        /// <param name="cellKey">The returned results are filtered to contain only ids for ways belonging to this cell.</param>
        /// <returns>An unordered list of the id of all TransportWay objects currently resident, filtered by transportNetwork and cellKey.</returns>
        public IList<TransportWayId> GetWayIdsForNetworkInCell(TransportNetworkType transportNetwork, TransportCellKey cellKey)
        {
            return m_apiInternal.GetWayIdsForNetworkInCell(transportNetwork, cellKey);
        }
        #endregion


        #region Polyline helpers
        /// <summary>
        /// Get an interpolated point along a polyline reprented as an array of points, with associated parameterized 
        /// distances of each point along the polyline.
        /// </summary>
        /// <param name="polylinePoints">Polyline vertex points, in ECEF coordinates.</param>
        /// <param name="polylineParams">Polyline parameterized distance of each vertex, each element in the range 0.0 to 1.0.</param>
        /// <param name="t">A parameterized distance along the polyline, in the range 0.0 to 1.0.</param>
        /// <returns>A point in ECEF coordinates at parameterized distance t along the supplied polyline.</returns>
        public DoubleVector3 GetPointEcefOnPolyline(DoubleVector3[] polylinePoints, double[] polylineParams, double t)
        {
            if (polylinePoints.Length != polylineParams.Length)
            {
                throw new System.ArgumentException("polylinePoints and polylineParams must be equal size");
            }

            if (polylinePoints.Length < 2)
            {
                throw new System.ArgumentException("polylinePoints must contain at least 2 elements");
            }

            return m_apiInternal.GetPointEcefOnPolyline(polylinePoints, polylineParams, t);
        }

        /// <summary>
        /// Get the direction of a polyline at a parameterized distance along it. The polyline is reprented as an array 
        /// of points, with associated parameterized.
        /// </summary>
        /// <param name="polylinePoints">Polyline vertex points, in ECEF coordinates.</param>
        /// <param name="polylineParams">Polyline parameterized distance of each vertex, each element in the range 0.0 to 1.0.</param>
        /// <param name="t">A parameterized distance along the polyline, in the range 0.0 to 1.0.</param>
        /// <returns>A unit direction vector in ECEF coordinates at parameterized distance t along the supplied polyline.</returns>
        public DoubleVector3 GetDirectionEcefOnPolyline(DoubleVector3[] polylinePoints, double[] polylineParams, double t)
        {
            if (polylinePoints.Length != polylineParams.Length)
            {
                throw new System.ArgumentException("polylinePoints and polylineParams must be equal size");
            }

            if (polylinePoints.Length < 2)
            {
                throw new System.ArgumentException("polylinePoints must contain at least 2 elements");
            }

            return m_apiInternal.GetDirectionEcefOnPolyline(polylinePoints, polylineParams, t);
        }

        #endregion

        #region ToString helpers
        /// <summary>
        /// Get a string representation for a given TransportCellKey.
        /// </summary>
        /// <param name="cellKey">A key uniquely identifying a tile on a transport network.</param>
        /// <returns>A string representation, formatted as a Morton code.</returns>
        public string TransportCellKeyToString(TransportCellKey cellKey)
        {
            return m_apiInternal.TransportCellKeyToString(cellKey);
        }
        #endregion

        private static void RaiseEvent(TransportNetworkChangedHandler handler, TransportNetworkType networkType, TransportCellKey cellKey)
        {
            if (handler != null)
            {
                handler(networkType, cellKey);
            }
        }

        internal TransportApiInternal GetApiInternal()
        {
            return m_apiInternal;
        }

    }
}
