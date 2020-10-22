
namespace Wrld.Transport
{

    /// <summary>
    /// Enumerated type of permitted direction of travel on Transport graph Ways.
    /// </summary>
    public enum TransportWayDirection
    {
        /// <summary>Travel is permitted in both directions.</summary>
        Bidirectional,

        /// <summary>Travel is permitted in one direction.</summary>
        OneWay,

        /// <summary>Travel is not permitted along this Way.</summary>
        ClosedInBothDirections
    }
}
