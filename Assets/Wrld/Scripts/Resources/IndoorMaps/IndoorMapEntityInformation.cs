using System;
using System.Collections.Generic;
using System.Linq;

namespace Wrld.Resources.IndoorMaps
{
    /// <summary>
    /// Maintains information about indoor map entities belonging to an indoor map with specified id. Entity 
    /// information is updated as map tiles stream in. Change notification is available via the OnChanged event.
    /// </summary>
    public class IndoorMapEntityInformation
    {
        /// <summary>
        /// Uniquely identifies this object.
        /// </summary>
        public int Id { get; private set; }
        
        /// <summary>
        /// The string id of the indoor map associated with this IndoorMapEntityInformation object.
        /// </summary>
        public string IndoorMapId { get; private set; }

        /// <summary>
        /// A collection of IndoorMapEntity objects, representing the currently loaded indentifiable features for the associated indoor map.
        /// </summary>
        public IList<IndoorMapEntity> IndoorMapEntities { get { return m_indoorMapEntities; } }

        /// <summary>
        /// The current streaming status for the associated indoor map.
        /// </summary>
        public IndoorMapEntityLoadState IndoorMapEntityLoadState { get; private set; }

        /// <summary>
        /// Notfication that the contents of this IndoorMapEntityInformation instance has changed.
        /// This may be due to streamed indoor map entity resources having loaded.
        /// </summary>
        public Action<IndoorMapEntityInformation> OnChanged;


        private List<IndoorMapEntity> m_indoorMapEntities = new List<IndoorMapEntity>();
        private IndoorMapEntityInformationApiInternal m_indoorMapEntityInformationApiInternal;
        private static int InvalidId = 0;

        /// <summary>
        /// Creates an IndoorMapEntityInformation instance and adds it to the WrldMap.
        /// </summary>
        /// <param name="indoorMapId">The string identifier of the indoor map to obtain entity information for.</param>
        /// <param name="indoorMapEntityInformationChangedDelegate">A delegate to called when an the IndoorMapEntityInformation 
        /// object has been updated. This can occur as map tiles stream in, causing additional indoor map entities to be present.</param>
        /// <returns>The new IndoorMapEntityInformation object.</returns>
        public static IndoorMapEntityInformation Create(
            string indoorMapId,
            Action<IndoorMapEntityInformation> indoorMapEntityInformationChangedDelegate
            )
        {
            return Api.Instance.IndoorMapEntityInformationApi.AddIndoorMapEntityInformation(indoorMapId, indoorMapEntityInformationChangedDelegate);
        }

        /// <summary>
        /// Removes an IndoorMapEntityInformation instance from the WrldMap and marks it as no longer in use (IsDiscarded() will return true).
        /// </summary>
        public void Discard()
        {
            m_indoorMapEntityInformationApiInternal.RemoveIndoorMapEntityInformation(this);
            InvalidateId();
        }

        /// <summary>
        /// If true, this IndoorMapEntityInformation is no longer in use. 
        /// </summary>
        public bool IsDiscarded()
        {
            return Id == InvalidId;
        }
        
        
        internal IndoorMapEntityInformation(
            IndoorMapEntityInformationApiInternal indoorMapEntityInformationApiInternal,
            int id,
            string indoorMapId,
            Action<IndoorMapEntityInformation> indoorMapEntityInformationChangedDelegate
            )
        {
            if (indoorMapEntityInformationApiInternal == null)
            {
                throw new ArgumentNullException("null indoorMapEntityInformationApiInternal");
            }

            if (id == InvalidId)
            {
                throw new ArgumentException("invalid id");
            }
            
            this.m_indoorMapEntityInformationApiInternal = indoorMapEntityInformationApiInternal;
            this.Id = id;
            this.IndoorMapId = indoorMapId;
            this.IndoorMapEntityLoadState = IndoorMapEntityLoadState.None;
            this.OnChanged = indoorMapEntityInformationChangedDelegate;
        }
        
        private void InvalidateId()
        {
            Id = InvalidId;
        }
        
        internal void SetEntityInformation(
            IList<IndoorMapEntity> indoorMapEntities,
            IndoorMapEntityLoadState indoorMapEntityLoadState
            )
        {
            m_indoorMapEntities = indoorMapEntities.ToList();
            IndoorMapEntityLoadState = indoorMapEntityLoadState;

            if (this.OnChanged != null)
            {
                this.OnChanged.Invoke(this);
            }
        }
    }
}
