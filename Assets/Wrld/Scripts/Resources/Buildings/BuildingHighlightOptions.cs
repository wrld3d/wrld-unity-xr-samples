using UnityEngine;
using Wrld.Common.Maths;
using Wrld.Space;

namespace Wrld.Resources.Buildings
{
    public delegate void BuildingInformationReceivedDelegate(BuildingHighlight buildingHighlight);

    /// <summary>
    /// Creation parameters for constructing a BuildingHighlight object.
    /// </summary>
    public class BuildingHighlightOptions
    {
        private LatLong m_selectionLocation = new LatLong(0.0, 0.0);
        private Vector2 m_selectionScreenPoint = Vector2.zero;
        
        private BuildingHighlightSelectionMode m_selectionMode = BuildingHighlightSelectionMode.SelectAtLocation;
        private Color m_color = UnityEngine.Color.white;
        private bool m_informationOnly = false;

        private BuildingInformationReceivedDelegate m_buildingInformationReceivedHandler;
        
        public BuildingHighlightOptions()
        {
        }


        /// <summary>
        /// Used to highlight any building that may be present at a LatLong location
        /// </summary>
        /// <param name="location">The LatLong location to query.</param>
        /// <returns>This BuildingHighlightOptions instance, with the query location set.</returns>
        public BuildingHighlightOptions HighlightBuildingAtLocation(LatLong location)
        {
            m_selectionLocation = location;
            m_selectionMode = BuildingHighlightSelectionMode.SelectAtLocation;
            return this;
        }

        /// <summary>
        /// Used to highlight any building that may be present at a screen coordinate for the current view point. 
        /// A ray from the camera origin and passing through the screen point is constructed - the first building that the ray intersects, if any, will be highlighted.
        /// </summary>
        /// <param name="screenPoint">The LatLong location to query.</param>
        /// <returns>This BuildingHighlightOptions instance, with the query location set.</returns>
        public BuildingHighlightOptions HighlightBuildingAtScreenPoint(Vector2 screenPoint)
        {
            m_selectionScreenPoint = screenPoint;
            m_selectionMode = BuildingHighlightSelectionMode.SelectAtScreenPoint;
            return this;
        }

        /// <summary>
        /// Sets the color of the graphical highlight.
        /// </summary>
        /// <param name="color">The color of the graphical highlight.</param>
        /// <returns>This BuildingHighlightOptions instance, with the color set.</returns>
        public BuildingHighlightOptions Color(Color color)
        {
            m_color = color;
            return this;
        }

        /// <returns>This BuildingHighlightOptions instance, with the InformationOnly property set to true.</returns>
        public BuildingHighlightOptions InformationOnly()
        {
            m_informationOnly = true;
            return this;
        }

        /// <returns>This BuildingHighlightOptions instance, with the BuildingInformationReceivedDelegate set.</returns>
        public BuildingHighlightOptions BuildingInformationReceivedHandler(BuildingInformationReceivedDelegate handler)
        {
            m_buildingInformationReceivedHandler = handler;
            return this;
        }


        internal BuildingHighlightSelectionMode GetSelectionMode()
        {
            return m_selectionMode;
        }

        internal Vector2 GetSelectionScreenPoint()
        {
            return m_selectionScreenPoint;
        }

        internal LatLong GetSelectionLocation()
        {
            return m_selectionLocation;
        }

        internal Color GetColor()
        {
            return m_color;
        }

        internal bool IsInformationOnly()
        {
            return m_informationOnly;
        }

        internal BuildingInformationReceivedDelegate GetBuildingInformationReceivedHandler()
        {
            return m_buildingInformationReceivedHandler;
        }


    }
}
