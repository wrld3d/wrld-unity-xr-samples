using Wrld.Space;
using Assets.Wrld.Scripts.Maths;
using System.Diagnostics;
using System;
using System.Collections.Generic;

namespace Wrld.Resources.IndoorMaps
{
    /// <summary>
    /// Api for obtaining information about indoor map entities: identifiable features on an indoor map.
    /// </summary>
    public class IndoorMapEntityInformationApi
    {
        /// <summary>
        /// Raised when an IndoorMapEntityInformation object has been updated.
        /// </summary>
        public event Action<IndoorMapEntityInformation> OnIndoorMapEntityInformationUpdated;

        private IndoorMapEntityInformationApiInternal m_apiInternal;

        internal IndoorMapEntityInformationApi(
            IndoorMapEntityInformationApiInternal apiInternal
            )
        {
            m_apiInternal = apiInternal;
            m_apiInternal.OnIndoorMapEntityInformationUpdated += (indoorMapEntityInformation) =>
            {
                if (OnIndoorMapEntityInformationUpdated != null)
                {
                    OnIndoorMapEntityInformationUpdated(indoorMapEntityInformation);
                }
            };
        }


        /// <summary>
        /// Adds an IndoorMapEntityInformation object, that will become populated with the ids of any indoor map 
        /// entities belonging to the specified indoor map as map tiles stream in.
        /// </summary>
        /// <param name="indoorMapId">The id of the indoor map to obtain entity information for.</param>
        /// <param name="indoorMapEntityInformationChangedDelegate">A delegate to obtain notification when the 
        /// IndoorMapEntityInformation object has been updated with indoor map entity ids, or null.</param>
        /// <returns>The IndoorMapEntityInformation instance.</returns>
        public IndoorMapEntityInformation AddIndoorMapEntityInformation(
            string indoorMapId,
            Action<IndoorMapEntityInformation> indoorMapEntityInformationChangedDelegate
            )
        {
            return m_apiInternal.AddIndoorMapEntityInformation(indoorMapId, indoorMapEntityInformationChangedDelegate);
        }

        /// <summary>
        /// Remove an IndoorMapEntityInformation object, previously added via AddIndoorMapEntityInformation.
        /// </summary>
        /// <param name="indoorMapEntityInformation">The IndoorMapEntityInformation instance to remove.</param>
        public void RemoveIndoorMapEntityInformation(IndoorMapEntityInformation indoorMapEntityInformation)
        {
            m_apiInternal.RemoveIndoorMapEntityInformation(indoorMapEntityInformation);
        }
    }
}

