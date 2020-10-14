
namespace Wrld.Resources.IndoorMaps
{
    /// <summary>
    /// Encapsulates a set of immutable properties pertaining to an indoor map. These properties are set through the WRLD indoor map service, 
    /// and cannot be changed through the Unity SDK. An IndoorMap object can be obtained via the Api.Instance.IndoorMapsApi.GetActiveIndoorMap() method.
    /// </summary>
    public class IndoorMap
    {
        internal IndoorMap(string id, string name, int floorCount, string[] shortFloorNames, string[] floorNames, int[] floorIds, string userData)
        {
            Id = id;
            Name = name;
            FloorCount = floorCount;
            ShortFloorNames = shortFloorNames;
            FloorNames = floorNames;
            FloorIds = floorIds;
            UserData = userData;
        }

        /// <summary>
        /// Gets the unique identifier for the indoor map.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets a readable name for the indoor map, usually the building name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the number of floors in the indoor map.
        /// </summary>
        public int FloorCount { get; private set; }

        /// <summary>
        /// Gets an array of short floor names, suitable for display. These are generally string versions of floor numbers or other short identifiers such as "G" or "LG".
        /// </summary>
        public string[] ShortFloorNames { get; private set; }

        /// <summary>
        /// Gets an array of floor names. Floor names may be longer than floor ids.
        /// </summary>
        public string[] FloorNames { get; private set; }

        /// <summary>
        /// Gets an array of floor ids.
        /// </summary>
        public int[] FloorIds { get; private set; }

        /// <summary>
        /// Gets user data which has been associated with the map through the indoor map service. The user data is a string in JSON format.
        /// </summary>
        public string UserData { get; private set; }
    }
}