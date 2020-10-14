
namespace Wrld.Transport
{
    /// <summary>
    /// A key used to uniquely identify a cell on a transport network.
    /// TransportApi.TransportCellKeyToString() may be used to obtain a string representation.
    /// </summary>
    public struct TransportCellKey
    {
        /// <summary>
        /// Integer key representation.
        /// </summary>
        public long Value;
    }
}
