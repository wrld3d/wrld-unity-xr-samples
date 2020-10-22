
namespace Wrld.Transport
{

    /// <summary>
    /// A value type used to uniquely identify a TransportDirectedEdge on a transport network.
    /// </summary>
    public struct TransportDirectedEdgeId
    {
        /// <summary>
        /// A key identifying the streamed tile cell in which the TransportDirectedEdge is located.
        /// </summary>
        public TransportCellKey CellKey;

        /// <summary>
        /// An integer that is uniquely identifies the TransportDirectedEdge within the specified cell and network type.
        /// </summary>
        public int LocalDirectedEdgeId;

        /// <summary>
        /// The network type to which the TransportDirectedEdge belongs.
        /// </summary>
        public TransportNetworkType NetworkType;

        /// <summary>
        /// Static factory method to create an empty instance representing a null-reference to a TransportDirectedEdge
        /// </summary>
        /// <returns>The TransportDirectedEdgeId instance.</returns>
        public static TransportDirectedEdgeId MakeEmpty()
        {
            var emptyId = new TransportDirectedEdgeId
            {
                CellKey = new TransportCellKey(),
                LocalDirectedEdgeId = -1,
                NetworkType = TransportNetworkType.Road
            };
            return emptyId;
        }

    }
}
