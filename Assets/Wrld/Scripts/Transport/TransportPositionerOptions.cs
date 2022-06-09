using System;
using Wrld.Space;

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
        [Obsolete("Deprecated, to be removed in future. Altitude defaults to 0 above terrain.", false)]
        /// True if optional input altitude is set.
        /// </summary>
        public bool HasAltitude { get; private set; }

        /// <summary>
        /// Optional input altitude in meters.
        /// </summary>
        public double AltitudeInMeters { get; private set; }

        /// <summary>
        /// Optional value indicating whether altitude is specified as a height above terrain, or an absolute altitude above sea level.
        /// </summary>
        public ElevationMode ElevationMode { get; private set; }

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
        public double MaxHeadingDeviationToMatchedPointDegrees { get; private set; }

        /// <summary>
        /// Constraint threshold for the maximum allowed distance between the input coordinates and a candidate point 
        /// on a TransportDirectedEdge, in meters.
        /// </summary>
        public double MaxDistanceToMatchedPointMeters { get; private set; }

        /// <summary>
        /// Constraint threshold for the maximum allowed distance between which the provided heading can match 
        /// on a TransportDirectedEdge, in meters.
        /// </summary>
        public double MaxDistanceForPossibleHeadingMatch { get; private set; }

        /// <summary>
        /// The transport network on which to attempt to find a matching point.
        /// </summary>
        public TransportNetworkType TransportNetworkType { get; private set; }


        public TransportPositionerOptions(
            double inputLatitudeDegrees,
            double inputLongitudeDegrees,
            bool hasAltitude,
            double altitudeInMeters,
            ElevationMode elevationMode,
            bool hasHeading,
            double inputHeadingDegrees,
            double maxDistanceToMatchedPointMeters,
            double maxHeadingDeviationToMatchedPointDegrees,
            double maxDistanceForPossibleHeadingMatch,
            TransportNetworkType transportNetworkType
            )
        {
            InputLatitudeDegrees = inputLatitudeDegrees;
            InputLongitudeDegrees = inputLongitudeDegrees;
#pragma warning disable CS0618
            HasAltitude = hasAltitude;
#pragma warning restore CS0618
            AltitudeInMeters = altitudeInMeters;
            ElevationMode = elevationMode;
            HasHeading = hasHeading;
            InputHeadingDegrees = inputHeadingDegrees;
            MaxDistanceToMatchedPointMeters = maxDistanceToMatchedPointMeters;
            MaxHeadingDeviationToMatchedPointDegrees = maxHeadingDeviationToMatchedPointDegrees;
            MaxDistanceForPossibleHeadingMatch = maxDistanceForPossibleHeadingMatch;
            TransportNetworkType = transportNetworkType;
        }
    }
}
