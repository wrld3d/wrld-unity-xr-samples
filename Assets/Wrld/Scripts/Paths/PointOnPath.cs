using System.Runtime.InteropServices;

namespace Wrld.Paths
{
    /// <summary>
    /// Result structure for PathApi.GetPointOnPath. It provides information about a point on a polyline path that is closest to a given query point.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class PointOnPath
    {
        /// <summary>
        /// The point on the polyline path that is closest to InputPoint.
        /// </summary>
        public readonly Wrld.Space.LatLong ResultPoint;

        /// <summary>
        /// The input query point, passed as parameter inputPoint to PathApi.GetPointOnPath.
        /// </summary>
        public readonly Wrld.Space.LatLong InputPoint;

        /// <summary>
        /// The distance between InputPoint and ResultPoint, in meters.
        /// </summary>
        public readonly double DistanceFromInputPoint;

        /// <summary>
        /// The parameteric distance of ResultPoint along the path polyline, in the range 0.0 to 1.0.
        /// </summary>
        public readonly double FractionAlongPath;

        /// <summary>
        /// The index of the polyline vertex that is immediately before or at ResultPoint. If the polyline path is empty, returns -1.
        /// </summary>
        public readonly int IndexOfPathSegmentStartVertex;

        /// <summary>
        /// The index of the polyline vertex that is immediately after or at ResultPoint. If the polyline path is empty, returns -1.
        /// </summary>
        public readonly int IndexOfPathSegmentEndVertex;

        public PointOnPath(
            Wrld.Space.LatLong resultPoint,
            Wrld.Space.LatLong inputPoint,
            double distanceFromInputPoint,
            double fractionAlongPath,
            int indexOfPathSegmentStartVertex,
            int indexOfPathSegmentEndVertex
            )
        {
            ResultPoint = resultPoint;
            InputPoint = inputPoint;
            DistanceFromInputPoint = distanceFromInputPoint;
            FractionAlongPath = fractionAlongPath;
            IndexOfPathSegmentStartVertex = indexOfPathSegmentStartVertex;
            IndexOfPathSegmentEndVertex = indexOfPathSegmentEndVertex;
        }
    }
}