using System.Collections.Generic;

namespace UnityEngine
{
    public static class UnityApiExtensions
    {
        /// <summary>
        /// Return all of the vertices associated with a bounding box
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="includeCenter">whether to also include the center point</param>
        /// <returns></returns>
        public static IEnumerable<Vector3> GetVertices(this Bounds bounds, bool includeCenter)
        {
            Vector3 v1 = bounds.min, v2 = bounds.max;
            yield return new Vector3(v1.x, v1.y, v1.z);
            yield return new Vector3(v1.x, v2.y, v1.z);
            yield return new Vector3(v2.x, v2.y, v1.z);
            yield return new Vector3(v2.x, v1.y, v1.z);

            yield return new Vector3(v1.x, v1.y, v2.z);
            yield return new Vector3(v1.x, v2.y, v2.z);
            yield return new Vector3(v2.x, v2.y, v2.z);
            yield return new Vector3(v2.x, v1.y, v2.z);

            if (includeCenter)
                yield return bounds.center;
        }
    }
}
