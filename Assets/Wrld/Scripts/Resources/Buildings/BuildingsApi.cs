using Wrld.Space;
using Assets.Wrld.Scripts.Maths;
using Wrld.Common.Maths;

namespace Wrld.Resources.Buildings
{
    public class BuildingsApi
    {
        private BuildingsApiInternal m_apiInternal;
        internal BuildingsApi(BuildingsApiInternal apiInternal)
        {
            m_apiInternal = apiInternal;
        }

        /// <summary>
        /// Create a BuildingHighlight object, for displaying graphical highlighting of a building, or for obtaining information about a building on the map.
        /// The method returns a BuildingHighlight object instance synchronously, but the result may not be initially populated with information about a building.
        /// Internally, an asynchronous web request may be made to fetch data, though locally cached results are used in some cases.
        /// Notification of the building information becoming available can be obtained by specifying a handler in the options parameter, via BuildingHighlightOptions.BuildingInformationReceivedHandler.
        /// BuildingHighlight.HasPopulatedBuildingInformation() can be called to query whether building information has yet been populated.
        /// </summary>
        /// <param name="buildingHighlightOptions">Creation options - see BuildingHighlightOptions for details.</param>
        /// <returns>A new BuildingHighlight object.</returns>
        public BuildingHighlight CreateHighlight(BuildingHighlightOptions buildingHighlightOptions)
        {
            return m_apiInternal.CreateHighlight(buildingHighlightOptions);
        }

        /// <summary>
        /// Perform a ray intersection test against the currently streamed map features, returning true if the first intersection with the ray is a building.
        /// A suitable ray may be obtained with SpacesApi.ScreenPointToRay() or SpacesApi.LatLongToVerticallyDownRay().
        /// </summary>
        /// <param name="rayEcef">A ray in ECEF coordinates.</param>
        /// <param name="out_intersectionPoint">The point of intersection of the ray and building, if any. The result is only valid if this method returns true.</param>
        /// <returns>True if the first intersection between the ray and map features is a building;
        /// false if no intersection is found, or if the ray first intersects with a map feature other than a building (for example, a tree).</returns>
        public bool TryFindIntersectionWithBuilding(DoubleRay rayEcef, out LatLongAltitude out_intersectionPoint)
        {
            return m_apiInternal.TryFindIntersectionWithBuilding(rayEcef, out out_intersectionPoint);
        }

        /// <summary>
        /// Perform a ray intersection test against the currently streamed map features, returning true if the first intersection with the ray is a building.
        /// A suitable ray may be obtained with SpacesApi.ScreenPointToRay() or SpacesApi.LatLongToVerticallyDownRay().
        /// </summary>
        /// <param name="rayEcef">A ray in ECEF coordinates.</param>
        /// <param name="out_intersectionPoint">The point of intersection of the ray and building, if any. The result is only valid if this method returns true.</param>
        /// <param name="out_intersectionNormal">The surface normal of the intersection, if any. The result is only valid if this method returns true.</param>
        /// <returns>True if the first intersection between the ray and map features is a building;
        /// false if no intersection is found, or if the ray first intersects with a map feature other than a building (for example, a tree).</returns>
        public bool TryFindIntersectionAndNormalWithBuilding(DoubleRay rayEcef, out LatLongAltitude out_intersectionPoint, out DoubleVector3 out_intersectionNormal)
        {
            return m_apiInternal.TryFindIntersectionAndNormalWithBuilding(rayEcef, out out_intersectionPoint, out out_intersectionNormal);
        }
    }
}
