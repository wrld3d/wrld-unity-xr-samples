using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wrld.Space
{
    /// <summary>
    /// Specifies how the elevation property of shape and Marker objects is interpreted.
    /// </summary>
    public enum ElevationMode
    {
        /// <summary>
        /// The elevation property is interpreted as an absolute altitude above mean sea level, in meters.
        /// </summary>
        HeightAboveSeaLevel,

        /// <summary>
        /// The elevation property is interpreted as a height relative to the map's terrain, in meters.
        /// </summary>
        HeightAboveGround
    };
}
