using Wrld.Resources.IndoorMaps;

namespace Wrld.Demo.IndoorMaps
{
    public class CustomIndoorMapTextureFetcher : IIndoorMapTextureFetcher
    {
        public void IssueTextureRequestsForMaterial(IIndoorMapMaterial material, IndoorMaterialDescriptor descriptor)
        {
            // do nothing, we won't bother with a texture for these materials - see DefaultIndoorMaterialFactory for an example where textures do get loaded
        }
    }
}

