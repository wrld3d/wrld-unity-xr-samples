using System.Collections.ObjectModel;

namespace Wrld.Resources.Buildings
{
    /// <summary>
    /// Information about a building on the map, obtained by creating a BuildingHighlight request.
    /// </summary>
    public class BuildingInformation
    {
        /// <summary>
        /// A unique identifier for the building. 
        /// The BuildingId for a building on the map is not necessarily maintained between versions of the map or Api. 
        /// </summary>
        public readonly string BuildingId;

        /// <summary>
        /// Summary information about the dimensions of the building.
        /// </summary>
        public readonly BuildingDimensions BuildingDimensions;

        /// <summary>
        /// A collection of BuildingContour objects, representing the geometry of the building.
        /// </summary>
        public readonly ReadOnlyCollection<BuildingContour> BuildingContours;

        public BuildingInformation(
            string buildingId,            
            BuildingDimensions buidingDimensions,
            ReadOnlyCollection<BuildingContour> buildingContours)
        {
            this.BuildingId = buildingId;            
            this.BuildingDimensions = buidingDimensions;
            this.BuildingContours = buildingContours;
        }
    }
}
