namespace Wrld.Transport
{
    /// <summary>
    /// Defines creation parameters for a TransportPositioner.
    /// See Wrld.Transport.TransportPositionerOptionsBuilder for a convenience builder to construct instances of this type.
    /// </summary>
    public class TransportPositionerOptions
    {
        /// <summary>
        /// Input latitude coordinate in degrees.
        /// </summary>
        public double InputLatitudeDegrees { get; private set; }

        /// <summary>
        /// Input longitude coordinate in degrees.
        /// </summary>
        public double InputLongitudeDegrees { get; private set; }

        /// <summary>
        /// True if optional input heading is set.
        /// </summary>
        public bool HasHeading { get; private set; }

        /// <summary>
        /// Optional input heading angle in degrees clockwise from North.
        /// </summary>
        public double InputHeadingDegrees { get; private set; }

        /// <summary>
        /// Constraint threshold for the maximum allowed difference between InputHeadingDegrees and the tangential 
        /// direction of a candidate on a TransportDirectedEdge, in degrees.
        /// </summary>
        public double MaxDistanceToMatchedPointMeters { get; private set; }

        /// <summary>
        /// Constraint threshold for the maximum allowed distance between the input coordinates and a candidate point 
        /// on a TransportDirectedEdge, in meters.
        /// </summary>
        public double MaxHeadingDeviationToMatchedPointDegrees { get; private set; }

        /// <summary>
        /// The transport network on which to attempt to find a matching point.
        /// </summary>
        public TransportNetworkType TransportNetworkType { get; private set; }


        public TransportPositionerOptions(
            double inputLatitudeDegrees,
            double inputLongitudeDegrees,
            bool hasHeading,
            double inputHeadingDegrees,
            double maxDistanceToMatchedPointMeters,
            double maxHeadingDeviationToMatchedPointDegrees,
            TransportNetworkType transportNetworkType
            )
        {
            InputLatitudeDegrees = inputLatitudeDegrees;
            InputLongitudeDegrees = inputLongitudeDegrees;
            HasHeading = hasHeading;
            InputHeadingDegrees = inputHeadingDegrees;
            MaxDistanceToMatchedPointMeters = maxDistanceToMatchedPointMeters;
            MaxHeadingDeviationToMatchedPointDegrees = maxHeadingDeviationToMatchedPointDegrees;
            TransportNetworkType = transportNetworkType;
        }
    }
}
