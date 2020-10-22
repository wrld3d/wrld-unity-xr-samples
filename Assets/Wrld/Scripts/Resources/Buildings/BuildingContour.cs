using System.Collections.ObjectModel;
using Wrld.Space;

namespace Wrld.Resources.Buildings
{
    /// <summary>
    /// Represents a building or part of building as a polygon with minimum and maximum altitudes. This can be used to construct an extruded polygon (prism) to visually represent the building.
    /// Complex buildings may be made up of multiple BuildingContours.
    /// </summary>
    public class BuildingContour
    {
        /// <summary>
        /// The minimum altitude above sea level.
        /// </summary>
        public readonly double BottomAltitude;

        /// <summary>
        /// The maximum altitude above sea level.
        /// </summary>
        public readonly double TopAltitude;

        /// <summary>
        /// The vertices of the building outline polygon, ordered clockwise from above.
        /// </summary>
        public readonly ReadOnlyCollection<LatLong> Points;

        public BuildingContour(
            double bottomAltitude,
            double topAltitude,
            ReadOnlyCollection<LatLong> points)
        {
            this.BottomAltitude = bottomAltitude;
            this.TopAltitude = topAltitude;
            this.Points = points;
        }
    }
}