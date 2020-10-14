

namespace Wrld.Space.EnvironmentFlattening
{
    /// <summary>
    /// Provides endpoints to flatten or un-flatten buildings and terrain, and query the current status.
    /// </summary>
    public class EnvironmentFlatteningApi
    {
        internal EnvironmentFlatteningApi(EnvironmentFlatteningApiInternal apiInternal)
        {
            m_apiInternal = apiInternal;
        }

        /// <summary>
        /// Flatten or un-flatten the buildings and terrain.
        /// </summary>
        /// <param name="isFlattened">A boolean which controls whether or not the buildings and terrain should be flattened.</param>
        public void SetIsFlattened(bool isFlattened)
        {
            m_apiInternal.SetIsFlattened(isFlattened);
        }

        /// <summary>
        /// Query if the buildings and terrain are currently flattened.
        /// </summary>
        public bool IsFlattened()
        {
            return m_apiInternal.IsFlattened();
        }

        /// <summary>
        /// Gets the current flattening scale of the buildings and terrain. This is useful for positioning and scaling objects.
        /// </summary>
        public float GetCurrentScale()
        {
            return m_apiInternal.GetCurrentScale();
        }

        private EnvironmentFlatteningApiInternal m_apiInternal;
    }
}