

using System.Collections.Generic;

namespace Wrld.Paths
{
    /// <summary>
    /// An Api containing utility methods for polyline paths.
    /// </summary>
    public class PathApi
    {
        internal PathApi(PathApiInternal apiInternal)
        {
            m_apiInternal = apiInternal;
        }

        /// <summary>
        /// For a polyline path, calculates the point on the polyline that is closest to a query point.
        /// </summary>
        /// <param name="inputPoint">The input query point.</param>
        /// <param name="polylinePathPoints">Points representing the path polyline vertices.</param>
        /// <returns>A results structure providing information about the point on the polyline path that is closest to inputPoint. </returns>
        public PointOnPath GetPointOnPath(Space.LatLong inputPoint, List<Space.LatLong> polylinePathPoints)
        {
            return m_apiInternal.GetPointOnPath(inputPoint, polylinePathPoints);
        }

        private PathApiInternal m_apiInternal;
    }
}