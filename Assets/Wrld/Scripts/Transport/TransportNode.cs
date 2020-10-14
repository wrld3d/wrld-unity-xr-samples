
using System.Collections.Generic;
using Wrld.Common.Maths;

namespace Wrld.Transport
{
    /// <summary>
    /// Represents a vertex on a transport network directed graph. 
    /// A TransportNode is located at the start and end of a TransportDirectedEdge. 
    /// Where two or more edges connect, they share single TransportNode object at their intersection.
    /// </summary>
    public class TransportNode
    {

        /// <summary>
        /// A unique identifier for the TransportNode.
        /// </summary>
        public TransportNodeId Id { get; private set; }

        /// <summary>
        /// The ECEF coordinate of this node.
        /// </summary>
        public DoubleVector3 Point { get; private set; }

        /// <summary>
        /// A list of ids of any directed edges that start at this node (i.e. where TransportDirecteEdge.NodeIdA == this.Id).
        /// Use TransportApi.TryGetDirectedEdge to access the corresponding TransportDirectedEdge for a given id.
        /// </summary>
        public IList<TransportDirectedEdgeId> IncidentDirectedEdges { get; private set; }

        internal TransportNode(
            TransportNodeId id,
            DoubleVector3 point,
            IList<TransportDirectedEdgeId> incidentDirectedEdges
            )
        {
            this.Id = id;
            this.Point = point;
            this.IncidentDirectedEdges = incidentDirectedEdges;
        }

        /// <summary>
        /// Static factory method to create an empty-value TransportNode instance. 
        /// </summary>
        /// <returns>The new empty-value object.</returns>
        public static TransportNode MakeEmpty()
        {
            var nullId = new TransportNodeId
            {
                CellKey = new TransportCellKey(),
                LocalNodeId = -1,
                NetworkType = TransportNetworkType.Road
            };
            return new TransportNode(nullId, DoubleVector3.zero, new List<TransportDirectedEdgeId>());
        }
    }
}
