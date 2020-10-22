using System;

namespace Wrld.Resources.IndoorMaps
{
    public interface IIndoorMapTextureStreamingService
    {
        void RequestTextureForMaterial(IIndoorMapMaterial material, string interiorName, string textureKey, string texturePath, bool isCubemap);
    }
}
