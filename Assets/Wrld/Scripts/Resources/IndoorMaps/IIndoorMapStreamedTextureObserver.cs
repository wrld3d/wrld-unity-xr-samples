using UnityEngine;

namespace Wrld.Resources.IndoorMaps
{
    public interface IIndoorMapStreamedTextureObserver
    {
        void OnStreamedTextureReceived(IIndoorMapMaterial requestingMaterial, string textureKey, Texture texture);
    }
}