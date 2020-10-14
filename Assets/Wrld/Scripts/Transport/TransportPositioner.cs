using System;

namespace Wrld.Transport
{
    /// <summary>
    /// Allows a coordinate to be matched to a point on a transport network.
    /// </summary>
    public class TransportPositioner
    {
        /// <summary>
        /// Uniquely identifies this object instance.
        /// </summary>
        public int Id { get; private set; }

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
        public bool HasInputHeading { get; private set; }

        /// <summary>
        /// Optional input heading angle in degrees clockwise from North.
        /// </summary>
        public double InputHeadingDegrees { get; private set; }

        /// <summary>
        /// Constraint threshold for the maximum allowed difference between InputHeadingDegrees and the tangent 
        /// direction of a candidate on a TransportDirectedEdge, in degrees.
        /// </summary>
        public double MaxHeadingDeviationToMatchedPointDegrees { get; private set; }

        /// <summary>
        /// Constraint threshold for the maximum allowed distance between the input coordinates and a candidate point 
        /// on a TransportDirectedEdge, in meters.
        /// </summary>
        public double MaxDistanceToMatchedPointMeters { get; private set; }

        /// <summary>
        /// The transport network on which to attempt to find a matching point.
        /// </summary>
        public TransportNetworkType TransportNetworkType { get; private set; }

        /// <summary>
        /// Notfication that the matched point on the transport graph has changed. This may be due to the input of 
        /// this object changing, or due to transport network resources streaming in and out.
        /// </summary>
        public event Action OnPointOnGraphChanged;

        private static int InvalidId = 0;

        private TransportApiInternal m_transportApiInternal;


        // Use Api.Instance.TransportApi.CreatePositioner for public construction
        internal TransportPositioner(
            TransportApiInternal transportApiInternal,
            int id,
            TransportPositionerOptions options)
        {
            if (transportApiInternal == null)
            {
                throw new ArgumentNullException("transportApiInternal");
            }

            if (id == InvalidId)
            {
                throw new ArgumentException("invalid id");
            }

            m_transportApiInternal = transportApiInternal;

            Id = id;
            InputLatitudeDegrees = options.InputLatitudeDegrees;
            InputLongitudeDegrees = options.InputLongitudeDegrees;
            InputHeadingDegrees = options.InputHeadingDegrees;
            MaxHeadingDeviationToMatchedPointDegrees = options.MaxHeadingDeviationToMatchedPointDegrees;
            MaxDistanceToMatchedPointMeters = options.MaxDistanceToMatchedPointMeters;
            TransportNetworkType = options.TransportNetworkType;
            HasInputHeading = options.HasHeading;
        }

        /// <summary>
        /// Set the input coordinate.
        /// </summary>
        /// <param name="latitudeDegrees">Input latitude, in degrees.</param>
        /// <param name="longitudeDegrees">Input longitude, in degrees.</param>
        public void SetInputCoordinates(double latitudeDegrees, double longitudeDegrees)
        {
            InputLatitudeDegrees = latitudeDegrees;
            InputLongitudeDegrees = longitudeDegrees;
            m_transportApiInternal.SetPositionerInputCoordinates(this, latitudeDegrees, longitudeDegrees);
        }

        /// <summary>
        /// Set the optional input heading.
        /// </summary>
        /// <param name="headingDegrees">Input heading angle, as clockwise degrees from North.</param>
        public void SetInputHeading(double headingDegrees)
        {
            InputHeadingDegrees = headingDegrees;
            HasInputHeading = true;
            m_transportApiInternal.SetPositionerInputHeading(this, headingDegrees);
        }

        /// <summary>
        /// Clear the optional input heading.
        /// </summary>
        public void ClearInputHeading()
        {
            InputHeadingDegrees = 0.0;
            HasInputHeading = false;
            m_transportApiInternal.ClearPositionerInputHeading(this);
        }

        /// <summary>
        /// Query whether this object currently has a matched point on the transport network.
        /// See GetPointOnGraph() for reasons why IsMatched() may return false.
        /// </summary>
        /// <returns>True if a match has been found, else false.</returns>
        public bool IsMatched()
        {
            return m_transportApiInternal.IsPositionerMatched(this);
        }

        /// <summary>
        /// Get results of the currently best-matched point on the transport network, if any.
        /// For a successful match to be made, the following constraints must be fulfilled by the currently streamed-in set for the specified transport network: 
        /// &lt;br/&gt;
        ///   * A point on the network can be found that is less than or equal to MaxDistanceToMatchedPointMeters from the input coordinate.
        /// &lt;br/&gt;  
        ///   * If a heading was specified (HasInputHeading is true), the angle between the input heading, and the tangential 
        ///   heading of the way at the candiate match point is less than or equal to MaxHeadingDeviationToMatchedPointDegrees.
        /// &lt;br/&gt;
        /// </summary>
        /// <returns>A TransportPositionerPointOnGraph result object.</returns>
        public TransportPositionerPointOnGraph GetPointOnGraph()
        {
            return m_transportApiInternal.GetPositionerPointOnGraph(this);
        }

        /// <summary>
        /// Destroys this TransportPositioner.
        /// </summary>
        public void Discard()
        {
            m_transportApiInternal.DestroyPositioner(this);
            Id = InvalidId;
        }

        internal void NotifyPointOnGraphChanged()
        {
            if (OnPointOnGraphChanged != null)
            {
                OnPointOnGraphChanged();
            }
        }
    }
}
