using System;
using UnityEngine;

namespace Wrld.Resources.IndoorMaps
{
    /// <summary>
    /// Represents an instanced renderable inside an Indoor Map. Usually used for things like furniture.
    /// </summary>
    public class InstancedIndoorMapRenderable : MonoBehaviour
    {
        internal IntPtr NativeInstance { get; private set; }
        internal int InstanceIndex { get; private set; }

        private IndoorMapsApiInternal m_internalAPI;

        /// <summary>
        /// Gets the color of this renderable as a Unity Color.
        /// </summary>
        public Color GetColor()
        {
            return m_internalAPI.GetInstancedIndoorMapRenderableColor(this);
        }

        /// <summary>
        /// Gets the floor index that this renderable is on.
        /// </summary>
        public int GetFloorIndex()
        {
            return m_internalAPI.GetInstancedIndoorMapRenderableFloorIndex(this);
        }

        /// <summary>
        /// If this entity is currently highlighted, gets the Unity Color of that highlight.
        /// </summary>
        /// <param name="highlightColor">Reference to a Unity Color object to store the color if successful.</param>
        /// <returns>True if a highlight color was found for this object, otherwise false.</returns>
        public bool TryGetHighlightColor(out Color highlightColor)
        {
            return m_internalAPI.TryGetInstancedIndoorMapHighlightColor(this, out highlightColor);
        }

        /// <summary>
        /// Gets the color saturation of this renderable.
        /// </summary>
        public float GetSaturation()
        {
            return m_internalAPI.GetInstancedIndoorMapRenderableSaturation(this);
        }

        /// <summary>
        /// Called just before this renderable is rendered. 
        /// </summary>
        public void OnRenderStateUpdated()
        {
            Material.PrepareToRender(this);
        }
        
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
                m_material.AssignToMeshRenderer(GetComponent<MeshRenderer>());
            }
        }

        internal void Init(int instanceIndex, IntPtr nativeInstance, IIndoorMapMaterial material, IndoorMapsApiInternal internalAPI)
        {
            InstanceIndex = instanceIndex;
            NativeInstance = nativeInstance;
            Material = material;
            m_internalAPI = internalAPI;
            OnRenderStateUpdated();
        }
    }
}