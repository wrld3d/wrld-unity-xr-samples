using System.Collections.ObjectModel;
using Wrld.Common.Maths;

namespace Wrld.Transport
{
    /// <summary>
    /// Result object for TransportApi.FindShortestPath().
    /// </summary>
    public class TransportPathfindResult
    {
        /// <summary>
        /// True if a path was found.
        /// </summary>
        public bool IsPathFound { get; private set; }

        /// <summary>
        /// If a path was found, an ordered list of TransportDirectedEdgeId objects representing the directed edges 
        /// traversed from start to goal.
        /// If a path was not found, an empty list.
        /// </summary>
        public ReadOnlyCollection<TransportDirectedEdgeId> PathDirectedEdgeIds { get; private set; }

        /// <summary>
        /// The path start point, as a parameterised distance along the first element in PathDirectedEdgeIds, in range 0.0 to 1.0.
        /// </summary>
        public double FirstEdgeParam { get; private set; }

        /// The path end point, as a parameterised distance along the last element in PathDirectedEdgeIds, in range 0.0 to 1.0.
        public double LastEdgeParam { get; private set; }

        /// <summary>
        /// The length of the path, in meters, if found; else 0.0.
        /// </summary>
        public double DistanceMeters { get; private set; }

        /// <summary>
        /// A list of points in ECEF coordinates representing the vertices of a polyline running through the center of 
        /// the ways traversed by the path from start to goal.
        /// </summary>
        public ReadOnlyCollection<DoubleVector3> PathPoints { get; private set; }

        /// <summary>
        /// A list of parameterized distances for each element of PathPoints, with each element in the range 0.0 to 1.0.
        /// </summary>
        public ReadOnlyCollection<double> PathPointParams { get; private set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TransportPathfindResult()
        {
            IsPathFound = false;
        }

        public TransportPathfindResult(
            bool isPathFound,
            ReadOnlyCollection<TransportDirectedEdgeId> pathDirectedEdgeIds,
            double firstEdgeParam,
            double lastEdgeParam,
            double distanceMeters,
            ReadOnlyCollection<DoubleVector3> pathPoints,
            ReadOnlyCollection<double> pathPointParams
            )
        {
            IsPathFound = isPathFound;
            PathDirectedEdgeIds = pathDirectedEdgeIds;
            FirstEdgeParam = firstEdgeParam;
            LastEdgeParam = lastEdgeParam;
            DistanceMeters = distanceMeters;
            PathPoints = pathPoints;
            PathPointParams = pathPointParams;
        }
    }
}
