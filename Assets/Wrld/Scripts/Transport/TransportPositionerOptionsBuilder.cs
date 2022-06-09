using Wrld.Space;

namespace Wrld.Transport
{
    /// <summary>
    /// A convenience builder to construct instances of Wrld.Transport.TransportPositionerOptions.
    /// </summary>
    public class TransportPositionerOptionsBuilder
    {
        private double m_latitudeDegrees;
        private double m_longitudeDegrees;
        private double m_altitudeInMeters;
        private ElevationMode m_elevationMode = ElevationMode.HeightAboveGround;
        private double m_headingDegrees;
        private double m_maxDistanceToMatchedPointMeters = 20.0;
        private double m_maxHeadingDeviationToMatchedPointDegrees = 30.0;
        private double m_maxDistanceForPossibleHeadingMatch = 0.5;
        private TransportNetworkType m_transportNetworkType = TransportNetworkType.Road;
        private bool m_hasCoordinates = false;
        private bool m_hasAltitude = false;
        private bool m_hasHeading = false;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TransportPositionerOptionsBuilder()
        {

        }

        /// <summary>
        /// Set input coordinates (required).
        /// </summary>
        /// <param name="latitudeDegrees">Input latitude coordinate in degrees.</param>
        /// <param name="longitudeDegrees">Input longitude coordinate in degrees.</param>
        /// <returns>This object, with the input coordinates set.</returns>
        public TransportPositionerOptionsBuilder SetInputCoordinates(double latitudeDegrees, double longitudeDegrees)
        {
            m_latitudeDegrees = latitudeDegrees;
            m_longitudeDegrees = longitudeDegrees;
            m_hasCoordinates = true;
            return this;
        }

        /// <summary>
        /// Set an optional input altitude.
        /// </summary>
        /// <param name="altitudeInMeters">Input altitude in meters.</param>
        /// <returns>This object, with the input altitude set.</returns>
        public TransportPositionerOptionsBuilder SetAltitudeInMeters(double altitudeInMeters)
        {
            m_altitudeInMeters = altitudeInMeters;
            m_hasAltitude = true;
            return this;
        }

        /// <summary>
        /// Set an optional mode specifying how the altitude property is interpreted.
        /// </summary>
        /// <param name="elevationMode">Input value indicating whether altitude is specified as a height above terrain, or an absolute altitude above sea level.</param>
        /// <returns>This object, with the input elevation mode set.</returns>
        public TransportPositionerOptionsBuilder SetElevationMode(ElevationMode elevationMode)
        {
            m_elevationMode = elevationMode;
            return this;
        }

        /// <summary>
        /// Set an optional input heading.
        /// </summary>
        /// <param name="headingDegrees">Input heading angle in degrees clockwise from North.</param>
        /// <returns>This object, with the input heading set.</returns>
        public TransportPositionerOptionsBuilder SetInputHeading(double headingDegrees)
        {
            m_headingDegrees = headingDegrees;
            m_hasHeading = true;
            return this;
        }

        /// <summary>
        /// Set a constraint threshold for the maximum allowed distance between the input coordinates and a candidate point
        /// on a TransportDirectedEdge, in meters.
        /// </summary>
        /// <param name="maxDistanceToMatchedPointMeters">The constraint distance, in meters.</param>
        /// <returns>This object, with the distance constraint set.</returns>
        public TransportPositionerOptionsBuilder SetMaxDistanceToMatchedPoint(double maxDistanceToMatchedPointMeters)
        {
            m_maxDistanceToMatchedPointMeters = maxDistanceToMatchedPointMeters;
            return this;
        }

        /// <summary>
        /// Set a constraint threshold for the maximum allowed difference between InputHeadingDegrees and
        /// the tangential direction of a candidate on a TransportDirectedEdge, in degrees.
        /// </summary>
        /// <param name="maxHeadingDeviationToMatchedPointDegrees">The constraint angle, in degrees.</param>
        /// <returns>This object, with the heading angle constraint set.</returns></returns>
        public TransportPositionerOptionsBuilder SetMaxHeadingDeviationToMatchedPoint(double maxHeadingDeviationToMatchedPointDegrees)
        {
            m_maxHeadingDeviationToMatchedPointDegrees = maxHeadingDeviationToMatchedPointDegrees;
            return this;
        }



        /// <summary>
        /// Set a constraint threshold for the maximum allowed distance between which the provided heading can match
        /// on a TransportDirectedEdge, in meters.
        /// </summary>
        /// <param name="maxDistanceForPossibleHeadingMatch">The constraint distance, in meters.</param>
        /// <returns>This object, with the heading distance constraint set.</returns></returns>
        public TransportPositionerOptionsBuilder SetMaxDistanceForPossibleHeadingMatch(double maxDistanceForPossibleHeadingMatch)
        {
            m_maxDistanceForPossibleHeadingMatch = maxDistanceForPossibleHeadingMatch;
            return this;
        }

        /// <summary>
        /// Set the transport network on which to attempt to find a matching point.
        /// </summary>
        /// <param name="transportNetworkType">The transport network on which the resultant TransportPositioner will operate.</param>
        /// <returns>This object, with the transport network set.</returns>
        public TransportPositionerOptionsBuilder SetTransportNetworkType(TransportNetworkType transportNetworkType)
        {
            m_transportNetworkType = transportNetworkType;
            return this;
        }

        /// <summary>
        /// Creates the options.
        /// </summary>
        /// <returns>The options object.</returns>
        public TransportPositionerOptions Build()
        {
            if (!m_hasCoordinates)
            {
                throw new System.ArgumentException("Coordinates must be set before calling Build().");
            }
            return new TransportPositionerOptions(
                m_latitudeDegrees,
                m_longitudeDegrees,
                m_hasAltitude,
                m_altitudeInMeters,
                m_elevationMode,
                m_hasHeading,
                m_headingDegrees,
                m_maxDistanceToMatchedPointMeters,
                m_maxHeadingDeviationToMatchedPointDegrees,
                m_maxDistanceForPossibleHeadingMatch,
                m_transportNetworkType);
        }
    }
}
