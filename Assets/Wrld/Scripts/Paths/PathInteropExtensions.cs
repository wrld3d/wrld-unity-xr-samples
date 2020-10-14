using Wrld.Interop;

namespace Wrld.Paths
{
    internal static class PathInteropExtensions
    {
        public static PointOnPath FromInterop(this PointOnPathInterop pointOnPathInterop)
        {
            return new PointOnPath(pointOnPathInterop.resultPoint.ToLatLong(),
                pointOnPathInterop.inputPoint.ToLatLong(),
                pointOnPathInterop.distancefromInputPoint,
                pointOnPathInterop.fractionAlongPath,
                pointOnPathInterop.indexOfPathSegmentStartVertex,
                pointOnPathInterop.indexOfPathSegmentEndVertex);
        }
    }
}