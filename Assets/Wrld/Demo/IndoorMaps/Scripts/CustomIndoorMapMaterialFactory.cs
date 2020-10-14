using System.IO;
using UnityEngine;
using Wrld.Resources.IndoorMaps;

namespace Wrld.Demo.IndoorMaps
{
    public class CustomIndoorMapMaterialFactory : IIndoorMapMaterialFactory
    {
        private Material m_sourceMaterial;

        public CustomIndoorMapMaterialFactory(string indoorMapMaterialsDirectory)
        {
            m_sourceMaterial = (Material)UnityEngine.Resources.Load(Path.Combine(indoorMapMaterialsDirectory, "InteriorsDiffuseUntexturedMaterial"), typeof(Material));
        }

        public IIndoorMapMaterial CreateMaterialFromDescriptor(IndoorMaterialDescriptor descriptor)
        {
            var material = new Material(m_sourceMaterial);
            
            return new CustomIndoorMapMaterial(material);
        }
    }
}