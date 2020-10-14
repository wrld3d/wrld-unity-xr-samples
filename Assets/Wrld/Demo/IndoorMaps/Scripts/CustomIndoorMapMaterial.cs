using System;
using UnityEngine;
using UnityEngine.Rendering;
using Wrld.Resources.IndoorMaps;

namespace Wrld.Demo.IndoorMaps
{
    public class CustomIndoorMapMaterial : IIndoorMapMaterial
    {
        public Material MaterialInstance { get; private set; }
        public Action<string, Texture> OnStreamingTextureReceived { get; set; }

        public void AssignToMeshRenderer(MeshRenderer renderer)
        {
            renderer.sharedMaterial = MaterialInstance;
        }

        public void PrepareToRender(IndoorMapRenderable renderable)
        {
            UpdateMaterial(renderable.GetColor(), renderable.gameObject);
        }

        public void PrepareToRender(IndoorMapHighlightRenderable renderable)
        {
            UpdateMaterial(renderable.GetColor(), renderable.gameObject);
        }

        public void PrepareToRender(InstancedIndoorMapRenderable renderable)
        {
            Color color;

            if (!renderable.TryGetHighlightColor(out color))
            {
                color = renderable.GetColor();
            }

            UpdateMaterial(color, renderable.gameObject);
        }

        public void OnStreamingTextureReceivedHandler(string textureKey, Texture texture)
        {
            // don't use textures
        }

        public IIndoorMapMaterial CreateCopy()
        {
            return new CustomIndoorMapMaterial(new Material(MaterialInstance));
        }

        public CustomIndoorMapMaterial(Material material)
        {
            MaterialInstance = material;
            OnStreamingTextureReceived += OnStreamingTextureReceivedHandler;
        }

        private void UpdateMaterial(Color color, GameObject gameObject)
        {
            // https://docs.unity3d.com/Manual/MaterialsAccessingViaScript.html
            const string fadeRenderMode = "_ALPHABLEND_ON";
            var meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                meshRenderer.receiveShadows = false;
                meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                if (color.a > 0.0f)
                {
                    meshRenderer.enabled = true;

                    if (color.a >= 1.0f)
                    {
                        MaterialInstance.SetFloat("_Mode", 0);
                        MaterialInstance.DisableKeyword(fadeRenderMode);
                        MaterialInstance.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                        MaterialInstance.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    }
                    else
                    {
                        MaterialInstance.SetFloat("_Mode", 2);
                        MaterialInstance.EnableKeyword(fadeRenderMode);
                        MaterialInstance.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        MaterialInstance.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    }
                }
                else
                {
                    meshRenderer.enabled = false;
                }
            }

            MaterialInstance.color = color * Color.red;
        }
    }
}