using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wrld.Transport
{
    /// <summary>
    /// Represents an edge connecting two ordered nodes on a transport network directed graph. 
    /// &lt;br/&gt;
    /// The direction of traversal is from NodeIdA to NodeIdB. It references a TransportWay object by id, which provides 
    /// geometry and other attributes associated with the edge.
    /// &lt;br/&gt;
    /// A TransportDirectedEdge instance is present in the graph for each permissible traversal direction of a TransportWay: 
    /// &lt;br/&gt;
    ///   * For a uni-directional TransportWay, a single TransportDirectedEdge is present (in the direction of allowed travel).
    /// &lt;br/&gt;
    ///   * For a bi-directional TransportWay, two TransportDirectedEdge are present, one for each traversal direction.
    /// &lt;br/&gt;
    /// &lt;br/&gt;
    /// Transport graphs are streamed as tiled cells. Where a way crosses a cell boundary, the way is cut, and a pair 
    /// of boundary nodes inserted at the intersection point, one in each of the adjacent cells. Each boundary node has 
    /// a 'link' TransportDirectedEdge object added to its IncidentDirectedEdges list, referencing that node as NodeIdA. 
    /// NodeIdB is set to an empty-value, indicating that this references a node in  a cell that has not yet streamed 
    /// in. As cells stream in, the graph connectivity is re-formed by fixing up NodeIdB to a valid node id. These 
    /// 'link' directed edge instances have an empty-value WayId, and are of zero length.
    /// </summary>
    public class TransportDirectedEdge
    {
        /// <summary>
        /// A unique identifier for the TransportDirectedEdge.
        /// </summary>
        public TransportDirectedEdgeId Id { get; private set; }

        /// <summary>
        /// The id of the TransportNode at the start of this TransportDirectedEdge. Use TransportApi.TryGetNode to access the corresponding TransportNode value.
        /// </summary>
        public TransportNodeId NodeIdA { get; private set; }

        /// <summary>
        /// The id of the TransportNode at the end of this TransportDirectedEdge. Use TransportApi.TryGetNode to access the corresponding TransportNode value.
        /// &lt;br/&gt;
        /// For 'link' directed edges at cell boundaries, this may have an empty value, indicating that the end of the edge is in a cell that has not yet streamed in. 
        /// </summary>
        public TransportNodeId NodeIdB { get; private set; }

        /// <summary>
        /// The id of the TransportWay associated with this TransportDirectedEdge. Use TransportApi.TryGetWay to access the corresponding TransportWay value.
        /// For 'link' directed edges at cell boundaries, this has an empty value. 
        /// </summary>
        public TransportWayId WayId { get; private set; }

        /// <summary>
        /// If true, this TransportDirectedEdge represents a traversal of the assoiciated TransportWay in the reverse 
        /// direction, i.e. starting at the last element and ending at the first element of TransportWay.CenterLinePoints.
        /// </summary>
        public bool IsWayReversed { get; private set; }

        internal TransportDirectedEdge(
                TransportDirectedEdgeId id,
                TransportNodeId nodeIdA,
                TransportNodeId nodeIdB,
                TransportWayId wayId,
                bool isWayReversed
            )
        {
            this.Id = id;
            this.NodeIdA = nodeIdA;
            this.NodeIdB = nodeIdB;
            this.WayId = wayId;
            this.IsWayReversed = isWayReversed;
        }

        /// <summary>
        /// Static factory method to create an empty-value TransportDirectedEdge instance. 
        /// </summary>
        /// <returns>The new empty-value object.</returns>
        public static TransportDirectedEdge MakeEmpty()
        {
            var nullId = new TransportDirectedEdgeId
            {
                CellKey = new TransportCellKey(),
                LocalDirectedEdgeId = -1,
                NetworkType = TransportNetworkType.Road
            };
            return new TransportDirectedEdge(nullId, new TransportNodeId(), new TransportNodeId(), new TransportWayId(), false);
        }

    }
}
