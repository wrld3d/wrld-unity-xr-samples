using System.Collections.Generic;
using Wrld.Common.Maths;

namespace Wrld.Transport
{
    /// <summary>
    /// Represents center-line geometry, width and other attributes of an edge on a transport network directed graph. 
    /// &lt;br/&gt;
    /// It forms the section of road (or track on Rail and Tram networks) between two nodes on a graph. 
    /// &lt;br/&gt;
    /// A TransportDirectedEdge object references a TransportWay, representing a traversal of the TransportWay in a 
    /// particular direction.
    /// &lt;br/&gt;
    /// For bi-directional ways, two TransportDirectedEdge objects may reference the same TransportWay, representing 
    /// traversals in the forward and reversed directions.
    /// &lt;br/&gt;
    /// For uni-directional ways, a single TransportDirectedEdge object references a TransportWay.
    /// </summary>
    public class TransportWay
    {
        /// <summary>
        /// A unique identifier for the TransportWay.
        /// </summary>
        public TransportWayId Id { get; private set; }

        /// <summary>
        /// A list of points in ECEF coordinates representing the vertices of a polyline running through the center of 
        /// the road or track visual geometry.
        /// </summary>
        public IList<DoubleVector3> CenterLinePoints { get; private set; }

        /// <summary>
        /// A list containing the same number of elements as CenterLinePoints. 
        /// &lt;br/&gt;
        /// Each element represents the distance along the center polyline of the CenterLinePoints element at the 
        /// same index. 
        /// Elements have values normalised in the range 0.0 to 1.0.
        /// </summary>
        public IList<double> CenterLineSplineParams { get; private set; }

        /// <summary>
        /// The geometric length, in meters, of the polyline represented by CenterLinePoints.
        /// </summary>
        public double LengthMeters { get; private set; }

        /// <summary>
        /// The nominal half-width (0.5 * width), in meters, of this road or track section. 
        /// &lt;br/&gt;
        /// Visual geometry may not match this value exactly along the entire length of the road or track section - 
        /// for example, where tapering between connected sections of differing width.
        /// </summary>
        public double HalfWidthMeters { get; private set; }

        /// <summary>
        /// The permitted direction or directions of travel along this TransportWay.
        /// </summary>
        public TransportWayDirection WayDirection { get; private set; }

        /// <summary>
        /// A string description of the functional classification of this road or track section, for example "local_road" or "secondary_road".
        /// </summary>
        public string Classification { get; private set; }

        /// <summary>
        /// An average speed, in kilometers per hour, of traffic on this road or track section. 
        /// &lt;br/&gt;
        /// This is intended for possible visualization uses; for example, if displaying simulated traffic moving along 
        /// the TransportWay this could be used to provide an appropriate speed. It does not necessarily reflect 
        /// real-world average speed. In some cases, the value has been inferred from functional classification.
        /// </summary>
        public double AverageSpeedKph { get; private set; }

        /// <summary>
        /// The approximate regulatory speed limit, in kilometers per hour, of traffic on this road or track section.
        /// &lt;br/&gt;
        /// This is intended for possible visualization uses only, and may not necessarily accurately reflect 
        /// real-world speed limits for all classifications of vehicle. In some cases, the value has been inferred 
        /// from functional classification.
        /// </summary>
        public double ApproximateSpeedLimitKph { get; private set; }

        internal TransportWay(
            TransportWayId id,
            IList<DoubleVector3> centerLinePoints,
            IList<double> centerLineSplineParams,
            double lengthMeters,
            double halfWidthMeters,
            TransportWayDirection wayDirection,
            string classification,
            double averageSpeedKph,
            double approximateSpeedLimitKph
            )
        {
            this.Id = id;
            this.CenterLinePoints = centerLinePoints;
            this.CenterLineSplineParams = centerLineSplineParams;
            this.LengthMeters = lengthMeters;
            this.HalfWidthMeters = halfWidthMeters;
            this.WayDirection = wayDirection;
            this.Classification = classification;
            this.AverageSpeedKph = averageSpeedKph;
            this.ApproximateSpeedLimitKph = approximateSpeedLimitKph;
        }

        /// <summary>
        /// Static factory method to create an empty-value TransportWay instance. 
        /// </summary>
        /// <returns>The new empty-value object.</returns>
        public static TransportWay MakeEmpty()
        {
            var nullId = new TransportWayId
            {
                CellKey = new TransportCellKey(),
                LocalWayId = -1,
                NetworkType = TransportNetworkType.Road
            };
            return new TransportWay(nullId, new List<DoubleVector3>(), new List<double>(), 0.0, 0.0, TransportWayDirection.ClosedInBothDirections, "", 0.0, 0.0);
        }


    }
}
