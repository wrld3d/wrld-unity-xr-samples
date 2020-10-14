using System.Collections.Generic;
using UnityEngine;

namespace Wrld.Resources.IndoorMaps
{
    internal class IndoorMapMaterialRepository
    {
        public void AddTemplateMaterial(IIndoorMapMaterial material)
        {
            m_instantiatedMaterials.Add(material, new List<IIndoorMapMaterial>() { material });
        }

        public IIndoorMapMaterial InstantiateMaterial(IIndoorMapMaterial templateMaterial)
        {
            var newMaterialInstance = templateMaterial.CreateCopy();

            // Each renderable should have its own copy of the material for per-renderable state (like colouring to work)
            // For per-material state, however, we need to share the material.

            // CreateMaterial is more like CreateMaterialTemplate
            Debug.Assert(m_instantiatedMaterials.ContainsKey(templateMaterial));
            m_instantiatedMaterials[templateMaterial].Add(newMaterialInstance);

            return newMaterialInstance;
        }

        public List<IIndoorMapMaterial> GetAllInstancesOfMaterial(IIndoorMapMaterial material)
        {
            if (m_instantiatedMaterials.ContainsKey(material))
            {
                return m_instantiatedMaterials[material];
            }

            return new List<IIndoorMapMaterial>();
        }
        public void DeleteMaterial(IIndoorMapMaterial material)
        {
            m_instantiatedMaterials.Remove(material);
        }

        private Dictionary<IIndoorMapMaterial, List<IIndoorMapMaterial>> m_instantiatedMaterials = new Dictionary<IIndoorMapMaterial, List<IIndoorMapMaterial>>();
    }
}