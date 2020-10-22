using System;
using UnityEngine;

namespace Wrld.Resources.IndoorMaps
{
    /// <summary>
    /// Represents a highlight placed inside an Indoor Map. 
    /// </summary>
    public class IndoorMapHighlightRenderable : MonoBehaviour
    {
        internal IntPtr NativeInstance { get; private set; }
        private IndoorMapsApiInternal m_internalAPI;
        private IIndoorMapMaterial m_material;

        /// <summary>
        /// The IIndoorMapMaterial that this should be rendered with.
        /// </summary>
        public IIndoorMapMaterial Material
        {
            get
            {
                return m_material;
            }

            set
            {
                m_material = value;
                GetComponent<MeshRenderer>().sharedMaterial = m_material.MaterialInstance;
            }
        }

        /// <summary>
        /// Gets the color of this renderable as a Unity Color.
        /// </summary>
        public Color GetColor()
        {
            return m_internalAPI.GetIndoorMapHighlightRenderableColor(this);
        }

        /// <summary>
        /// Gets the floor index that this highlight is on.
        /// </summary>
        public int GetFloorIndex()
        {
            return m_internalAPI.GetIndoorMapHighlightRenderableFloorIndex(this);
        }

        /// <summary>
        /// Called just before this renderable is rendered. 
        /// </summary>
        public void OnRenderStateUpdated()
        {
            Material.PrepareToRender(this);
        }

        internal void Init(IntPtr nativeInstance, IIndoorMapMaterial material, IndoorMapsApiInternal internalAPI)
        {
            NativeInstance = nativeInstance;
            Material = material;
            m_internalAPI = internalAPI;
            OnRenderStateUpdated();
        }
    }
}