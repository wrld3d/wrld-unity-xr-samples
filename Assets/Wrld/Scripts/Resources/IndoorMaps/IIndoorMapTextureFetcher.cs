using System;

namespace Wrld.Resources.IndoorMaps
{
    /// <summary>
    /// This is used when streaming textures for an IIndoorMapMaterial are required from WRLD's service. 
    /// </summary>
    public interface IIndoorMapTextureFetcher
    {
        /// <summary>
        /// Implement this to handle streaming texture requests. An example exists in DefaultIndoorMapTextureFetcher.cs.
        /// </summary>
        /// <param name="material">The IIndoorMapMaterial to fetch this texture for.</param>
        /// <param name="descriptor">The IndoorMaterialDescriptor used to create the material parameter.</param>
        void IssueTextureRequestsForMaterial(IIndoorMapMaterial material, IndoorMaterialDescriptor descriptor);
    }
}

