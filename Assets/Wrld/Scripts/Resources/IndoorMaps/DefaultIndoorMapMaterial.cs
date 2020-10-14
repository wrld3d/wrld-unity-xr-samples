using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Wrld.Resources.IndoorMaps
{
    public class DefaultIndoorMapMaterial : IIndoorMapMaterial
    {
        public enum DrawOrder
        {
            StencilMirrorReflectionMask = 0,
            StencilMirrorReflectedGeometry = 1,
            StencilMirrorReflectiveSurface = 2,
            AfterStencilMirror = 3,
            Count = 4
        };

        private Color m_diffuseColor;
        private DrawOrder m_drawOrder;
        private Material m_prepassMaterial;

        public Material MaterialInstance { get; private set; }
        public Action<string, Texture> OnStreamingTextureReceived { get; set; }

        public DefaultIndoorMapMaterial(Material material, Color diffuseColor, DrawOrder drawOrder, Material prepassMaterial)
        {
            MaterialInstance = material;
            m_diffuseColor = diffuseColor;
            m_drawOrder = drawOrder;
            m_prepassMaterial = prepassMaterial;

            OnStreamingTextureReceived += OnStreamingTextureReceivedHandler;
        }

        public void AssignToMeshRenderer(MeshRenderer renderer)
        {
            if (m_prepassMaterial != null)
            {
                renderer.sharedMaterials = new[] { MaterialInstance, m_prepassMaterial };
            }
            else
            {
                renderer.sharedMaterial = MaterialInstance;
            }
        }

        public void PrepareToRender(IndoorMapRenderable renderable)
        {
            var color = renderable.GetColor() * m_diffuseColor;
            SetMaterialColor(color, renderable.GetFloorIndex(), renderable.gameObject);
        }

        public void PrepareToRender(IndoorMapHighlightRenderable renderable)
        {
            var color = renderable.GetColor();
            const float maxHighlightAlpha = 0.5f;
            color.a *= maxHighlightAlpha;
            SetMaterialColor(color, renderable.GetFloorIndex(), renderable.gameObject);
        }

        public void PrepareToRender(InstancedIndoorMapRenderable renderable)
        {
            Color color;

            if (!renderable.TryGetHighlightColor(out color))
            {
                color = renderable.GetColor() * m_diffuseColor;
            }

            SetMaterialColor(color, renderable.GetFloorIndex(), renderable.gameObject);
        }

        public IIndoorMapMaterial CreateCopy()
        {
            var prepassMaterial = m_prepassMaterial != null ? new Material(m_prepassMaterial) : null;

            return new DefaultIndoorMapMaterial(new Material(MaterialInstance), m_diffuseColor, m_drawOrder, prepassMaterial);
        }

        public void OnStreamingTextureReceivedHandler(string textureKey, Texture texture)
        {
            if (textureKey.Equals("CubeMapTexturePath"))
            {   
                MaterialInstance.SetTexture(Shader.PropertyToID("_Cube"), texture);
            }
            else
            {
                MaterialInstance.SetTexture(Shader.PropertyToID("_MainTex"), texture);
            }
        }

        private void SetMaterialColor(Color color, int floorIndex, GameObject gameObject)
        {
            bool isVisible = color.a > 0.0f;
            bool isOpaque = color.a >= 1.0f;
            var meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>(true);

            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                if (isVisible)
                {
                    var material = meshRenderer.sharedMaterial;
                    bool isReflectiveFloor = meshRenderer.sharedMaterials.Length > 1;

                    if (isReflectiveFloor)
                    {
                        if (color.a < 1.0f)
                        {
                            meshRenderer.enabled = false;
                            continue;
                        }

                        const bool mirrorMaskRequiresStrictDrawOrder = true;
                        UpdateDrawOrder(meshRenderer.sharedMaterials[1], floorIndex, mirrorMaskRequiresStrictDrawOrder, DrawOrder.StencilMirrorReflectionMask);
                    }

                    meshRenderer.enabled = true;

                    bool requiresStrictDrawOrder = !isOpaque || m_drawOrder != DrawOrder.AfterStencilMirror;
                    UpdateDrawOrder(material, floorIndex, requiresStrictDrawOrder, m_drawOrder);
                    UpdateBlendState(material, isOpaque);
                    UpdateZState(meshRenderer, material, isOpaque);

                    material.color = color;
                }
                else
                {
                    meshRenderer.enabled = false;
                }
            }
        }

        private static void UpdateDrawOrder(Material material, int floorIndex, bool requiresStrictDrawOrder, DrawOrder drawOrder)
        {
            int baseTransparentIndex = ((int)RenderQueue.Transparent) + floorIndex * (int)DrawOrder.Count;

            material.renderQueue = (!requiresStrictDrawOrder) ? ((int)RenderQueue.Transparent) : baseTransparentIndex + (int)drawOrder;
        }

        private static void UpdateZState(MeshRenderer meshRenderer, Material material, bool isOpaque)
        {
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            material.SetFloat("_ZWrite", isOpaque ? 1.0f : 0.0f);
        }

        private static void UpdateBlendState(Material material, bool isOpaque)
        {
            // https://docs.unity3d.com/Manual/MaterialsAccessingViaScript.html
            const string fadeRenderMode = "_ALPHABLEND_ON";

            if (isOpaque)
            {
                material.DisableKeyword(fadeRenderMode);
                material.SetInt("_SrcBlend", (int)BlendMode.One);
                material.SetInt("_DstBlend", (int)BlendMode.Zero);
            }
            else
            {
                material.EnableKeyword(fadeRenderMode);
                material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            }
        }
    }
}