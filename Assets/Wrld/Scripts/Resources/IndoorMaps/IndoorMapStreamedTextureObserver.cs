using System;
using UnityEngine;

namespace Wrld.Resources.IndoorMaps
{
    internal class IndoorMapStreamedTextureObserver : IIndoorMapStreamedTextureObserver
    {
        private IndoorMapMaterialRepository m_materialRepository;

        public IndoorMapStreamedTextureObserver(IndoorMapMaterialRepository materialRepository)
        {
            m_materialRepository = materialRepository;
        }
        public void OnStreamedTextureReceived(IIndoorMapMaterial requestingMaterial, string textureKey, Texture texture)
        {
            var materialInstances = m_materialRepository.GetAllInstancesOfMaterial(requestingMaterial);

            foreach (var materialInstance in materialInstances)
            {
                if (materialInstance.OnStreamingTextureReceived != null)
                {
                    materialInstance.OnStreamingTextureReceived(textureKey, texture);
                }
            }
        }
    }
}

