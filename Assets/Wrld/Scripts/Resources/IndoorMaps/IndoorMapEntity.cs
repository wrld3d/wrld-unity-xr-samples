using Wrld.Space;

namespace Wrld.Resources.IndoorMaps
{
    /// <summary>
    /// Represents infomation about an identifiable feature on an indoor map. These correspond to 
    /// features within a level GeoJSON in an indoor map submission via the WRLD Indoor Map REST 
    /// API. See <https://github.com/wrld3d/wrld-indoor-maps-api/blob/master/FORMAT.md>
    /// </summary>
    public class IndoorMapEntity
    {
        public IndoorMapEntity(
            string indoorMapEntityId,
            int indoorMapFloorId,
            LatLong position
            )
        {
            IndoorMapEntityId = indoorMapEntityId;
            IndoorMapFloorId = indoorMapFloorId;
            Position = position;
        }

        /// <summary>
        /// The string identifier of this indoor map entity. This identifier is expected to be 
        /// unique across all indoor map entities for a single indoor map.
        /// </summary>
        public readonly string IndoorMapEntityId;
        
        /// <summary>
        /// The identifier of an indoor map floor on which this indoor map entity is positioned.
        /// </summary>
        public readonly int IndoorMapFloorId;

        /// <summary>
        /// The location of this indoor map entity. Although indoor map entities can represent 
        /// area features such as rooms or desks, this position provides a point that is in the 
        /// center of the feature. As such, it is suitable for use if locating a GameObject at this 
        /// entity's position, or if positioning the camera to look at this entity.
        /// </summary>
        public readonly LatLong Position;
        
    }
}