using System;
using UnityEngine;

namespace Wrld.Resources.IndoorMaps
{
    /// <summary>
    /// This interface is used to apply materials to Indoor Maps. Implement it in your own classes to provide custom material rendering functionality. 
    /// A default implementation is available in DefaultIndoorMapMaterial.cs.
    /// </summary>
    public interface IIndoorMapMaterial
    {
        /// <summary>
        /// The Unity Material instance associated with this IIndoorMapMaterial. 
        /// </summary>
        Material MaterialInstance { get; }

        /// <summary>
        /// This is called when a streaming texture has been fetched for the renderable that this material is associated with.
        /// The string argument is the texture key for the streaming texture associated with the renderable.
        /// The Texture argument is the Unity Texture which holds the streaming texture itself.
        /// </summary>
        Action<string, Texture> OnStreamingTextureReceived { get; set; }

        /// <summary>
        /// This function controls how the material is applied to the MeshRenderer's material or sharedMaterial properties.
        /// Note: this is called automatically when a renderable has streamed in and the Material is applied to it.
        /// </summary>
        /// <param name="renderer">The target MeshRenderer.</param>
        void AssignToMeshRenderer(MeshRenderer renderer);

        /// <summary>
        /// Called just before rendering an associated IndoorMapRenderable.
        /// </summary>
        /// <param name="renderable">The IndoorMapRenderable about to be rendered with this material.</param>
        void PrepareToRender(IndoorMapRenderable renderable);
        /// <summary>
        /// Called just before rendering an associated IndoorMapHighlightRenderable.
        /// </summary>
        /// <param name="renderable">The IndoorMapHighlightRenderable about to be rendered with this material.</param>
        void PrepareToRender(IndoorMapHighlightRenderable renderable);
        /// <summary>
        /// Called just before rendering an associated InstancedIndoorMapRenderable.
        /// </summary>
        /// <param name="renderable">The InstancedIndoorMapRenderable about to be rendered with this material.</param>
        void PrepareToRender(InstancedIndoorMapRenderable renderable);

        /// <summary>
        /// Called when materials are instantiated; provides a copy of this material.
        /// </summary>
        IIndoorMapMaterial CreateCopy();
    }
}