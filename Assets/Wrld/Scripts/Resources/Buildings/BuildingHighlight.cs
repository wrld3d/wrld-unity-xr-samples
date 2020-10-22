using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Wrld.Resources.Buildings
{
    /// <summary>
    /// Represents a single selected building on the map, for displaying a graphical overlay 
    /// to highlight the building, or for obtaining information about the building.
    /// </summary>
    public class BuildingHighlight
    {
        /// <summary>
        /// Uniquely identifies this object instance.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// If true, a graphical overlay will not be displayed for the selected building - this BuildingHighlight is for obtaining BuildingInformation only.
        /// </summary>
        public bool IsInformationalOnly { get; private set; }

        public BuildingInformationReceivedDelegate BuildingInformationReceivedDelegate { get; private set; }
        
        private Color m_color;
        private BuildingInformation m_buildingInformation = null;
        private BuildingsApiInternal m_buildingsApiInternal;

        private static int InvalidId = 0;

        /// <summary>
        /// Creates a building highlight and adds it to the WrldMap.
        /// </summary>
        /// <param name="buildingHighlightOptions">The BuildingHighlightOptions object specifying how to create the BuildingHighlight object.</param>
        /// <returns>The new BuildingHighlight object</returns>
        public static BuildingHighlight Create(BuildingHighlightOptions buildingHighlightOptions)
        {
            return Api.Instance.BuildingsApi.CreateHighlight(buildingHighlightOptions);
        }

        /// <summary>
        /// Removes a building highlight from the WrldMap and marks it as no longer in use (IsDiscarded() will return true).
        /// </summary>
        public void Discard()
        {
            m_buildingsApiInternal.DestroyHighlight(this);
            InvalidateId();
        }

        /// <summary>
        /// If true, this BuildingHighlight is no longer in use, and no graphical highlight will be displayed for its associated building. 
        /// If no BuildingInformation is found for the requested point, the Api marks this object as discarded.
        /// </summary>
        public bool IsDiscarded()
        {
            return Id == InvalidId;
        }

        /// <returns>True if valid BuildingInformation has been received. False if the Api is pending requested BuildingInformation, or if no BuildingInformation was received.</returns>
        public bool HasPopulatedBuildingInformation()
        {
            return m_buildingInformation != null;
        }

        /// <summary>
        /// Access BuildingInformation obtained for this BuildingHighlight. 
        /// BuildingInformation is obtained asynchronously after the creation of a BuildingHighlight object - until this is successfully received, GetBuildingInformation() will return null.
        /// Notification of BuildingInformation being received can be obtained by providing a delegate method in the BuildingHighlightOptions passed to BuildingHighlight.Create - see BuildingHighlightOptions.BuildingInformationReceivedHandler.
        /// </summary>
        public BuildingInformation GetBuildingInformation()
        {
            return m_buildingInformation;
        }

        /// <summary>
        /// Access the current color of the graphical overlay for this BuildingHighlight.
        /// </summary>
        public Color GetColor()
        {
            return m_color;
        }

        /// <summary>
        /// Sets the display color of this building highlight.
        /// </summary>
        /// <param name="color">The color to set.</param>
        public void SetColor(Color color)
        {
            m_color = color;
            m_buildingsApiInternal.SetHighlightColor(this, color);
        }

        // Use Create for public construction
        internal BuildingHighlight(
            BuildingsApiInternal buildingsApiInternal,
            int id,
            Color color,
            bool informationalOnly,
            BuildingInformationReceivedDelegate buildingInformationReceivedDelegate)
        {
            if (buildingsApiInternal == null)
            {
                throw new ArgumentNullException("buildingsApi");
            }

            if (id == InvalidId)
            {
                throw new ArgumentException("invalid id");
            }

            this.m_buildingsApiInternal = buildingsApiInternal;
            this.Id = id;
            this.m_color = color;
            this.IsInformationalOnly = informationalOnly;
            this.BuildingInformationReceivedDelegate = buildingInformationReceivedDelegate;
        }

        private void InvalidateId()
        {
            Id = InvalidId;
        }

        internal void SetBuildingInformation(BuildingInformation buildingInformation)
        {
            m_buildingInformation = buildingInformation;
            if (string.IsNullOrEmpty(buildingInformation.BuildingId))
            {
                // discard highlight if empty building information 
                Discard();
            }

            if (this.BuildingInformationReceivedDelegate != null)
            {
                this.BuildingInformationReceivedDelegate(this);
            }
        }
    }
}
