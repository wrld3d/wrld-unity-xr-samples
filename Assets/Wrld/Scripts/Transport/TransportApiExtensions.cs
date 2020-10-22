using System.Collections.Generic;
using System.Linq;
using Wrld.Common.Maths;

namespace Wrld.Transport
{
    /// <summary>
    /// Extension methods for TransportApi.
    /// </summary>
    public static class TransportApiExtensions
    {
        /// <summary>
        /// Get all TransportNode objects currently streamed in for the given transport network and cell.
        /// </summary>
        /// <param name="transportApi">TransportApi instance.</param>
        /// <param name="transportNetwork">The returned results are filtered to contain only objects belonging to this transport network.</param>
        /// <param name="cellKey">The returned results are filtered to contain only objects belonging to this cell.</param>
        /// <returns>The list of TransportNode objects for the given network and cell.</returns>
        static public IList<TransportNode> GetNodesForNetworkAndCell(this TransportApi transportApi, TransportNetworkType transportNetwork, TransportCellKey cellKey)
        {
            var nodeIds = transportApi.GetNodeIdsForNetworkInCell(transportNetwork, cellKey);
            var nodes = new List<TransportNode>();
            foreach (var nodeId in nodeIds)
            {
                TransportNode node;
                if (!transportApi.TryGetNode(nodeId, out node))
                {
                    throw new System.ArgumentOutOfRangeException("unable to fetch TransportWay");
                }
                nodes.Add(node);
            }
            return nodes;
        }

        /// <summary>
        /// Get all TransportDirectedEdge objects currently streamed in for the given transport network and cell.
        /// </summary>
        /// <param name="transportApi">TransportApi instance.</param>
        /// <param name="transportNetwork">The returned results are filtered to contain only objects belonging to this transport network.</param>
        /// <param name="cellKey">The returned results are filtered to contain only objects belonging to this cell.</param>
        /// <returns>The list of TransportDirectedEdge objects for the given network and cell.</returns>
        static public IList<TransportDirectedEdge> GetDirectedEdgesForNetworkAndCell(this TransportApi transportApi, TransportNetworkType transportNetwork, TransportCellKey cellKey)
        {
            var directedEdgeIds = transportApi.GetDirectedEdgeIdsForNetworkInCell(transportNetwork, cellKey);
            var directedEdges = new List<TransportDirectedEdge>();
            foreach (var directedEdgeId in directedEdgeIds)
            {
                TransportDirectedEdge directedEdge;
                if (!transportApi.TryGetDirectedEdge(directedEdgeId, out directedEdge))
                {
                    throw new System.ArgumentOutOfRangeException("unable to fetch TransportDirectedEdge");
                }
                directedEdges.Add(directedEdge);
            }
            return directedEdges;
        }

        /// <summary>
        /// Get all TransportWay objects currently streamed in for the given transport network and cell.
        /// </summary>
        /// <param name="transportApi">TransportApi instance.</param>
        /// <param name="transportNetwork">The returned results are filtered to contain only objects belonging to this transport network.</param>
        /// <param name="cellKey">The returned results are filtered to contain only objects belonging to this cell.</param>
        /// <returns>The list of TransportWay objects for the given network and cell.</returns>
        static public IList<TransportWay> GetWaysForNetworkAndCell(this TransportApi transportApi, TransportNetworkType transportNetwork, TransportCellKey cellKey)
        {
            var wayIds = transportApi.GetWayIdsForNetworkInCell(transportNetwork, cellKey);
            var ways = new List<TransportWay>();
            foreach (var wayId in wayIds)
            {
                TransportWay way;
                if (!transportApi.TryGetWay(wayId, out way))
                {
                    throw new System.ArgumentOutOfRangeException("unable to fetch TransportWay");
                }
                ways.Add(way);
            }
            return ways;
        }

        /// <summary>
        /// Get a string representation for a given TransportNodeId.
        /// </summary>
        /// <param name="transportApi">TransportApi instance.</param>
        /// <param name="nodeId">The id of a TransportNode.</param>
        /// <returns>A string representation of the id, in format \`\`\`&lt;networkType&gt;:&lt;cellKey&gt;:&lt;localId&gt;\`\`\`.</returns>
        static public string NodeIdToString(this TransportApi transportApi, TransportNodeId nodeId)
        {
            return string.Format("{0}:{1}:{2}", nodeId.NetworkType, transportApi.TransportCellKeyToString(nodeId.CellKey), nodeId.LocalNodeId);
        }

        /// <summary>
        /// Get a string representation for a given TransportDirectedEdgeId.
        /// </summary>
        /// <param name="transportApi">TransportApi instance.</param>
        /// <param name="directedEdgeId">The id of a TransportDirectedEdge.</param>
        /// <returns>A string representation of the id, in format \`\`\`&lt;networkType&gt;:&lt;cellKey&gt;:&lt;localId&gt;\`\`\`.</returns>
        static public string DirectedEdgeIdToString(this TransportApi transportApi, TransportDirectedEdgeId directedEdgeId)
        {
            return string.Format("{0}:{1}:{2}", directedEdgeId.NetworkType, transportApi.TransportCellKeyToString(directedEdgeId.CellKey), directedEdgeId.LocalDirectedEdgeId);
        }


        /// <summary>
        /// Get a string representation for a given TransportWayId.
        /// </summary>
        /// <param name="transportApi">TransportApi instance.</param>
        /// <param name="wayId">The id of a TransportDirectedWay.</param>
        /// <returns>A string representation of the id, in format \`\`\`&lt;networkType&gt;:&lt;cellKey&gt;:&lt;localId&gt;\`\`\`.</returns>
        static public string WayIdToString(this TransportApi transportApi, TransportWayId wayId)
        {
            return string.Format("{0}:{1}:{2}", wayId.NetworkType, transportApi.TransportCellKeyToString(wayId.CellKey), wayId.LocalWayId);
        }

        /// <summary>
        /// TransportGraph factory method.
        /// </summary>
        /// <param name="transportApi">TransportApi instance.</param>
        /// <param name="transportNetwork">The transport network for which the TransportGraph will be created.</param>
        /// <returns>A TransportGraph instance.</returns>
        static public TransportGraph CreateTransportGraph(this TransportApi transportApi, TransportNetworkType transportNetwork)
        {
            return new TransportGraph(transportNetwork, transportApi);
        }

        /// <summary>
        /// For a given TransportPositionerPointOnGraph, returns the parameterized distance along the TransportDirectedEdge 
        /// on which the point lies (as opposed to TransportPositionerPointOnGraph.ParameterizedPointOnWay, the 
        /// parameterized distance along the associated TransportWay).
        /// </summary>
        /// <param name="transportPositionerPointOnGraph">TransportPositionerPointOnGraph instance.</param>
        /// <returns>If transportPositionerPointOnGraph.IsMatched is true, the parameterized distance along the directed edge, in range 0.0 to 1.0; else 0.0.</returns>
        static public double GetParameterOnDirectedEdge(this TransportPositionerPointOnGraph transportPositionerPointOnGraph)
        {
            return transportPositionerPointOnGraph.IsWayReversed ? (1.0 - transportPositionerPointOnGraph.ParameterizedPointOnWay) : transportPositionerPointOnGraph.ParameterizedPointOnWay;
        }


        /// <summary>
        /// Get a point at a parameterized distance along the path represented by a given TransportPathfindResult.
        /// </summary>
        /// <param name="transportApi">TransportApi instance.</param>
        /// <param name="pathfindResult">Pathfind result, as returned by TransportApi.FindShortestPath.</param>
        /// <param name="t">A parameterized distance along the path, in the range 0.0 to 1.0.</param>
        /// <returns>A point in ECEF coordinates.</returns>
        static public DoubleVector3 GetPointEcefOnPath(this TransportApi transportApi, TransportPathfindResult pathfindResult, double t)
        {
            return transportApi.GetPointEcefOnPolyline(pathfindResult.PathPoints.ToArray(), pathfindResult.PathPointParams.ToArray(), t);
        }

        /// <summary>
        /// Get the center-line direction at a parameterized distance along the path represented by a given TransportPathfindResult.
        /// </summary>
        /// <param name="transportApi">TransportApi instance.</param>
        /// <param name="pathfindResult">Pathfind result, as returned by TransportApi.FindShortestPath.</param>
        /// <param name="t">A parameterized distance along the path, in the range 0.0 to 1.0.</param>
        /// <returns>A unit direction vector in ECEF coordinates.</returns>
        static public DoubleVector3 GetDirectionEcefOnPath(this TransportApi transportApi, TransportPathfindResult pathfindResult, double t)
        {
            return transportApi.GetDirectionEcefOnPolyline(pathfindResult.PathPoints.ToArray(), pathfindResult.PathPointParams.ToArray(), t);
        }
    }
}
