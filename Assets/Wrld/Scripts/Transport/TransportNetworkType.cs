using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wrld.Transport
{
    /// <summary>
    /// Enumerated type of available transport networks.
    /// </summary>
    public enum TransportNetworkType
    {
        /// <summary>
        /// Road transport network.
        /// </summary>
        Road,

        /// <summary>
        /// Rail (train) transport network.
        /// </summary>
        Rail,

        /// <summary>
        /// Tram transport network (only available in selected map locations).
        /// </summary>
        Tram
    };
}
