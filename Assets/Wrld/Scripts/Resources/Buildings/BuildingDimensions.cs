using Wrld.Space;

namespace Wrld.Resources.Buildings
{
    /// <summary>
    /// Represents dimensional information about a building on the map
    /// </summary>
    public class BuildingDimensions
    {
        /// <summary>
        /// The altitude of the building's baseline - nominally at local ground level.
        /// </summary>
        public double BaseAltitude { get; private set; }

        /// <summary>
        /// The altitude of the building's highest point
        /// </summary>
        public double TopAltitude { get; private set; }

        /// <summary>
        /// The centroid of the building in plan view.
        /// </summary>
        public LatLong Centroid { get; private set; }

        public BuildingDimensions(
            double baseAltitude,
            double topAltitude,
            LatLong centroid)
        {
            this.BaseAltitude = baseAltitude;
            this.TopAltitude = topAltitude;
            this.Centroid = centroid;
        }
    }
}
