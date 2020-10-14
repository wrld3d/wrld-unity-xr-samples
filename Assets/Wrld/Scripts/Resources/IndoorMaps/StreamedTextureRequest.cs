
namespace Wrld.Resources.IndoorMaps
{
    public class StreamedTextureRequest
    {
        public IIndoorMapMaterial Material { get; private set; }
        public string TextureKey { get; private set; }
        public IIndoorMapStreamedTextureObserver Observer { get; private set; }
        public IndoorMapTextureStreamingService Originator { get; private set; }

        public StreamedTextureRequest(IIndoorMapMaterial material, string textureKey, IIndoorMapStreamedTextureObserver observer, IndoorMapTextureStreamingService originator)
        {
            Material = material;
            TextureKey = textureKey;
            Observer = observer;
            Originator = originator;
        }
    }
}