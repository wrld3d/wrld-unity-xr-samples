

namespace Wrld.Resources.IndoorMaps
{
    public class DefaultIndoorMapTextureFetcher : IIndoorMapTextureFetcher
    {
        IIndoorMapTextureStreamingService m_textureStreamingService;

        public DefaultIndoorMapTextureFetcher(IIndoorMapTextureStreamingService textureStreamingService)
        {
            m_textureStreamingService = textureStreamingService;
        }

        public void IssueTextureRequestsForMaterial(IIndoorMapMaterial material, IndoorMaterialDescriptor descriptor)
        {
            string texturePath;
            var key = "DiffuseTexturePath";

            if (descriptor.Strings.TryGetValue(key, out texturePath))
            {
                m_textureStreamingService.RequestTextureForMaterial(material, descriptor.IndoorMapName, key, texturePath, false);
            }

            var cubeMapKey = "CubeMapTexturePath";
            string cubeMapTexturePath;

            if (descriptor.Strings.TryGetValue(cubeMapKey, out cubeMapTexturePath))
            {
                if (!string.IsNullOrEmpty(cubeMapTexturePath))
                {
                    m_textureStreamingService.RequestTextureForMaterial(material, descriptor.IndoorMapName, cubeMapKey, cubeMapTexturePath, true);
                }
            }
        }
    }
}
