
namespace Wrld.Transport
{
    /// <summary>
    /// Input parameters for TransportApi.FindShortestPath()
    /// See Wrld.Transport.TransportPathfindOptionsBuilder for a convenience builder to construct instances of this type.
    /// </summary>
    public class TransportPathfindOptions
    {
        /// <summary>
        /// The id of a directed edge object on which the start point lies.
        /// </summary>
        public TransportDirectedEdgeId DirectedEdgeIdA { get; private set; }

        /// <summary>
        /// The id of a directed edge object on which the goal point lies.
        /// </summary>
        public TransportDirectedEdgeId DirectedEdgeIdB { get; private set; }
        
        /// <summary>
        /// A point on the start directed edge, specified as a parameterised distance along the edge, in range 0.0 to 1.0.
        /// </summary>
        public double ParameterizedPointOnEdgeA { get; private set; }

        /// <summary>
        /// A point on the goal directed edge, specified as a parameterised distance along the edge, in range 0.0 to 1.0.
        /// </summary>
        public double ParameterizedPointOnEdgeB { get; private set; }

        /// <summary>
        /// If true, for a start point on a bi-directional way, the pathfinding algorithm is permitted to immediately 
        /// traverse the adjacent directed edge on the same way but in the opposite direction (following the opposite carriageway).
        /// </summary>
        public bool UTurnAllowedAtA { get; private set; }

        /// <summary>
        /// If true, for a goal point on a bi-directional way, the pathfinding algorithm is permitted to immediately 
        /// traverse the adjacent directed edge on the same way but in the opposite direction (following the opposite carriageway).
        /// </summary>
        public bool UTurnAllowedAtB { get; private set; }

        public TransportPathfindOptions(
            TransportDirectedEdgeId directedEdgeIdA,
            TransportDirectedEdgeId directedEdgeIdB,
            double parameterizedPointOnEdgeA,
            double parameterizedPointOnEdgeB,
            bool uTurnAllowedAtA,
            bool uTurnAllowedAtB
            )
        {
            DirectedEdgeIdA = directedEdgeIdA;
            DirectedEdgeIdB = directedEdgeIdB;
            ParameterizedPointOnEdgeA = parameterizedPointOnEdgeA;
            ParameterizedPointOnEdgeB = parameterizedPointOnEdgeB;
            UTurnAllowedAtA = uTurnAllowedAtA;
            UTurnAllowedAtB = uTurnAllowedAtB;
        }
    }
}
