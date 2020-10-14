using System;
using UnityEngine;

namespace Wrld.Resources.IndoorMaps
{
    /// <summary>
    /// Represents a renderable chunk of an Indoor Map, usually the walls and floor of one level of the building.
    /// </summary>
    public class IndoorMapRenderable : MonoBehaviour
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
                m_material.AssignToMeshRenderer(GetComponent<MeshRenderer>());
            }
        }

        /// <summary>
        /// Gets the color of this renderable as a Unity Color.
        /// </summary>
        public Color GetColor()
        {
            return m_internalAPI.GetIndoorMapRenderableColor(this);
        }

        /// <summary>
        /// Gets the color saturation of this renderable.
        /// </summary>
        public float GetSaturation()
        {
            return m_internalAPI.GetIndoorMapRenderableSaturation(this);
        }

        /// <summary>
        /// Gets the floor index associated with this renderable. 
        /// </summary>
        public int GetFloorIndex()
        {
            return m_internalAPI.GetIndoorMapRenderableFloorIndex(this);
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

