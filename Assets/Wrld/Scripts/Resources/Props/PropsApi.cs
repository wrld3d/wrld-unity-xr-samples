using System;

namespace Wrld.Resources.Props
{
    /// <summary>
    /// An API to create and receive change notification for Prop instances.
    /// </summary>
    public class PropsApi
    {
        private PropsApiInternal m_apiInternal;
        internal PropsApi(PropsApiInternal apiInternal)
        {
            m_apiInternal = apiInternal;
        }

        /// <summary>
        /// Creates an instance of a Prop.
        /// </summary>
        /// <param name="propOptions">The PropOptions object which defines creation parameters for this Prop.</param>
        public Prop CreateProp(PropOptions propOptions)
        {
            return m_apiInternal.CreateProp(propOptions);
        }

        /// <summary>
        /// Allows the user to toggle the automatic population of an indoor map with any associated prop.  If this has been set to true
        /// each floor of the map will automatically fill with any associated props held in the indoor map service, if it is false, none
        /// will appear.
        /// </summary>
        /// <param name="enabled">true to enable automatic population of the indoor map with props from the indoor map service, false otherwise</param>
        public void SetAutomaticIndoorMapPopulationEnabled(bool enabled)
        {
            m_apiInternal.SetAutomaticIndoorMapPopulationEnabled(enabled);
        }

        /// <summary>
        /// Query whether automatic indoor map population is enabled
        /// </summary>
        /// <returns>True if this is enabled, false otherwise</returns>
        public bool IsAutomaticIndoorMapPopulationEnabled()
        {
            return m_apiInternal.IsAutomaticIndoorMapPopulationEnabled();
        }
    }
}