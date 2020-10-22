namespace Wrld.Resources.IndoorMaps
{
    /// <summary>
    /// Represents the current map tiles loading state for an indoor map.
    /// </summary>
    public enum IndoorMapEntityLoadState
    {
        /// <summary>
        /// The indoor map is not loaded.
        /// </summary>
        None,

        /// <summary>
        /// Some map tiles for the indoor map are loaded.
        /// </summary>
        Partial,

        /// <summary>
        /// All map tiles for the indoor map are loaded.
        /// </summary>
        Complete
    }
}
