
namespace Wrld.Transport
{
    /// <summary>
    /// A value type used to uniquely identify a TransportNode on a transport network.
    /// </summary>
    public struct TransportNodeId
    {
        /// <summary>
        /// A key identifying the streamed tile cell in which the TransportNode is located.
        /// </summary>
        public TransportCellKey CellKey;

        /// <summary>
        /// An integer that uniquely identifies the TransportNode within the specified cell and network type.
        /// </summary>
        public int LocalNodeId;

        /// <summary>
        /// The network type to which the TransportNode belongs.
        /// </summary>
        public TransportNetworkType NetworkType;

        /// <summary>
        /// Static factory method to create an empty instance representing a null-reference to a TransportNode.
        /// </summary>
        /// <returns>The TransportNodeId instance.</returns>
        public static TransportNodeId MakeEmpty()
        {
            var emptyId = new TransportNodeId
            {
                CellKey = new TransportCellKey(),
                LocalNodeId = -1,
                NetworkType = TransportNetworkType.Road
            };
            return emptyId;
        }
    }
}
