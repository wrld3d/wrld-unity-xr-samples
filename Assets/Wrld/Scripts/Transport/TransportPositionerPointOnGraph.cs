
using Wrld.Common.Maths;

namespace Wrld.Transport
{
    /// <summary>
    /// Publishes the result of a TransportPositioner.
    
    /// </summary>
    public class TransportPositionerPointOnGraph
    {
        /// <summary>
        /// True if a matched point on a transport network is found that fulfills the input constraints of the TransportPositioner.
        /// </summary>
        public bool IsMatched { get; private set; }

        /// <summary>
        /// If true, the TransportDirectedEdge referenced by DirectedEdgeId traverses its associated TransportWay in the reverse direction. 
        /// The value is equal to DirectedEdgeId.IsWayReversed, the field is provided here for convenience.
        /// </summary>
        public bool IsWayReversed { get; private set; }

        /// <summary>
        /// If IsMatched is true, the id of the TransportDirectedEdge on which the matched point lies; else an empty value.
        /// </summary>
        public TransportDirectedEdgeId DirectedEdgeId { get; private set; }

        /// <summary>
        /// If IsMatched is true, a point on the center line polyline of the TransportWay associated with DirectedEdgeId. 
        /// The point is expressed as a parameterised distance along the TransportWay center polyline in the range 0.0 to 1.0.
        /// </summary>
        public double ParameterizedPointOnWay { get; private set; }

        /// <summary>
        /// If IsMatched is true, a point on the center line polyline of the TransportWay associated with DirectedEdgeId. 
        /// The point is expressed in ECEF coordinates.
        /// If IsMatched is false, a zero vector.
        /// </summary>
        public DoubleVector3 PointOnWay { get; private set; }

        /// <summary>
        /// If IsMatched is true, the local direction of the TransportWay center line at PointOnWay, in ECEF space.
        /// Where PointOnWay lies on a center line vertex, the direction of the polyline line segment immediately prior 
        /// to the vertex is used, except for the first vertex, in which case the direction of the first line segment 
        /// is returned.
        /// If IsMatched is false, a zero vector.
        /// </summary>
        public DoubleVector3 DirectionOnWay { get; private set; }

        /// <summary>
        /// If IsMatched is true, the heading angle, in degrees clockwise from North, of the local direction of the TransportWay center line 
        /// at PointOnWay.
        /// If IsMatched is false, 0.0.
        /// </summary>
        public double HeadingOnWayDegrees { get; private set; }

        public TransportPositionerPointOnGraph(
            bool isMatched,
            bool isWayReversed,
            TransportDirectedEdgeId directedEdgeId,
            double parameterizedPointOnWay,
            DoubleVector3 pointOnWay,
            DoubleVector3 directionOnWay,
            double headingOnWayDegrees
            )
        {
            IsMatched = isMatched;
            IsWayReversed = isWayReversed;
            DirectedEdgeId = directedEdgeId;
            ParameterizedPointOnWay = parameterizedPointOnWay;
            PointOnWay = pointOnWay;
            DirectionOnWay = directionOnWay;
            HeadingOnWayDegrees = headingOnWayDegrees;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="other"></param>
        public TransportPositionerPointOnGraph(TransportPositionerPointOnGraph other)
        {
            IsMatched = other.IsMatched;
            IsWayReversed = other.IsWayReversed;
            DirectedEdgeId = other.DirectedEdgeId;
            ParameterizedPointOnWay = other.ParameterizedPointOnWay;
            PointOnWay = other.PointOnWay;
            DirectionOnWay = other.DirectionOnWay;
            HeadingOnWayDegrees = other.HeadingOnWayDegrees;
        }

        /// <summary>
        /// Static factory method to create an empty-value TransportPositionerPointOnGraph instance. 
        /// </summary>
        /// <returns>The new empty-value object.</returns>
        public static TransportPositionerPointOnGraph MakeEmpty()
        {
            return new TransportPositionerPointOnGraph(
                isMatched: false,
                isWayReversed: false,
                directedEdgeId: TransportDirectedEdgeId.MakeEmpty(),
                parameterizedPointOnWay: 0.0,
                pointOnWay: DoubleVector3.zero,
                directionOnWay: DoubleVector3.zero,
                headingOnWayDegrees: 0.0
                );
        }
    }
}
