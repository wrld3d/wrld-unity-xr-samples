
namespace Wrld.Transport
{
    /// <summary>
    /// A value type used to uniquely identify a TransportWay on a transport network.
    /// </summary>
    public struct TransportWayId
    {
        /// <summary>
        /// A key identifying the streamed tile cell in which the TransportWay is located.
        /// </summary>
        public TransportCellKey CellKey;

        /// <summary>
        /// An integer that uniquely identifies the TransportWay within the specified cell and network type.
        /// </summary>
        public int LocalWayId;

        /// <summary>
        /// The network type to which the TransportWay belongs.
        /// </summary>
        public TransportNetworkType NetworkType;

        /// <summary>
        /// Static factory method to create an empty instance representing a null-reference to a TransportWay
        /// </summary>
        /// <returns>A TransportWayId instance.</returns>
        public static TransportWayId MakeEmpty()
        {
            var emptyId = new TransportWayId
            {
                CellKey = new TransportCellKey(),
                LocalWayId = -1,
                NetworkType = TransportNetworkType.Road
            };
            return emptyId;
        }
    }
}
