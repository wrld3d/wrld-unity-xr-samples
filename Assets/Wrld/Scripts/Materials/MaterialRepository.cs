using AOT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using Wrld.Resources.IndoorMaps;
using Wrld.Utilities;

namespace Wrld.Materials
{
    public class MaterialRepository
    {
        public MaterialRepository(string materialsDirectory, Material defaultLandmarkMaterial, TextureLoadHandler textureLoadHandler)
        { 
            m_materialDirectory = materialsDirectory;
            m_materials = new Dictionary<string, MaterialRecord>();
            m_defaultMaterial = defaultLandmarkMaterial == null ? LoadPlaceHolderMaterial("placeholder") : defaultLandmarkMaterial;
            m_defaultRasterTerrainMaterial = LoadOrCreateMaterial("placeholder", "placeholder_rasterTerrain");
            m_defaultInteriorMaterial = LoadOrCreateMaterial("", "placeholder_interior");
            m_textureLoadHandler = textureLoadHandler;
        }

        public void Update()
        {
            m_materialsToRemove.Clear();

            foreach (var material in m_materialsRequiringTexture)
            {
                if (TryAssignTextureForMaterial(material))
                {
                    m_materialsToRemove.Add(material);
                }
            }

            m_materialsRequiringTexture.ExceptWith(m_materialsToRemove);
        }

        private bool TryAssignTextureForMaterial(Material material)
        {
            var texture = GetTextureIDForMaterial(NativePluginRunner.API, material.name);

            if (texture != 0)
            {
                m_textureLoadHandler.Update();
                ApplyTextureToMaterial(material, m_textureLoadHandler.GetTexture(texture));

                if (material.name.Contains("-alpha_"))
                {
                    material.SetFloat("_Mode", 2.0f);
                    material.EnableKeyword("_ALPHATEST_ON");
                }

                return true;
            }

            return false;
        }

        private void ApplyTextureToMaterial(Material material, Texture texture)
        {
            material.SetTexture(Shader.PropertyToID("_MainTex"), texture);
        }

        Color RandomGrayColor()
        {
            var value = UnityEngine.Random.value;
            return new Color(value, value, value);
        }

        public Material LoadPlaceHolderMaterial(string placeholderName)
        {
            Material material = null;

            if (!string.IsNullOrEmpty(m_materialDirectory))
            {
                material = (Material)UnityEngine.Resources.Load(Path.Combine(m_materialDirectory, placeholderName), typeof(Material));

                if (material == null)
                {
                    throw new ArgumentException("Cannot find placeholder material or material directory is inaccessible");
                } 
            }
            else
            {
                material = new Material(Shader.Find("Standard"));
                material.SetColor("_Color", RandomGrayColor());
            }

            return material;
        }

        [DllImport(NativePluginRunner.DLL)]
        private static extern uint GetTextureIDForMaterial(
            IntPtr api, 
            [MarshalAs(UnmanagedType.LPStr)] string materialName);

        private static bool RequiresStreamedTexture(string materialName)
        {
            return materialName.StartsWith("Raster") || materialName.StartsWith("landmark_");
        }

        private static string GetDisambiguatedMaterialName(string objectID, string materialName)
        {
            return materialName.StartsWith("landmark_") ? materialName + "_" + objectID : materialName;
        }

        private Material CreateMaterialFromTemplate(string materialName)
        {
            var sourceMaterial = m_defaultMaterial;

            if (materialName.StartsWith("Raster"))
            {
                sourceMaterial = m_defaultRasterTerrainMaterial;
            }
            else if (materialName.StartsWith("Interior"))
            {
                sourceMaterial = m_defaultInteriorMaterial;
            }

            var material = new Material(sourceMaterial);
            material.CopyPropertiesFromMaterial(sourceMaterial);

            return material;
        }
        
        private string AdjustMaterialNameForProceduralLandmark(string materialName)
        {
            // hack: force assign the buildings material for procedural landmarks that are produced when indoor maps don't specify a landmark id            
            if (materialName.ToLower().StartsWith("landmark_indoor_"))
            {
                return "buildings_01";
            }

            return materialName;
        }

        public Material LoadOrCreateMaterial(string objectID, string materialName)
        {
            MaterialRecord record;
            string disambiguatedMaterialName = GetDisambiguatedMaterialName(objectID, materialName);
            
            if (m_materials.TryGetValue(disambiguatedMaterialName, out record))
            {
                record.ReferenceCount++;

                return record.Material;
            }

            Material material = null;
            
            materialName = AdjustMaterialNameForProceduralLandmark(materialName);
            bool requiresTexture = RequiresStreamedTexture(materialName);

            if (!string.IsNullOrEmpty(m_materialDirectory))
            {
                material = (Material)UnityEngine.Resources.Load(Path.Combine(m_materialDirectory, materialName), typeof(Material));

                if (material == null)
                {
                    if (materialName == "placeholder_terrain")
                    {
                        material = m_defaultRasterTerrainMaterial;
                    }
                    else
                    {
                        if (IsHighlightMaterialName(materialName))
                        {
                            // Since we do not have appropriate modeling of highlight views on unity yet the highlight material is created in the CreateHighlight method in BuildingsApi.cs
                            if (!materialName.Contains("entity_highlight")) // only send warning if this is not an interior highlight
                            {
                                Debug.LogWarningFormat("Highlight material {0} has not been created with the appropriate color. Creating default material.", materialName);
                            }
                            material = CreateHighlightMaterial(new Color(1, 1, 0, 0.5f));
                        }
                        else
                        {
                            material = CreateMaterialFromTemplate(materialName);
                        }
                    }
                }
            }
            else
            {
                material = new Material(Shader.Find("Standard"));

                if (!materialName.StartsWith("landmark"))
                {
                    material.SetColor("_Color", RandomGrayColor());
                }
            }

            record = CreateAndAddMaterialRecord(material, disambiguatedMaterialName);
            record.ReferenceCount++;

            if (requiresTexture)
            {
                if (!TryAssignTextureForMaterial(material))
                {
                    m_materialsRequiringTexture.Add(material);
                }
            }

            return material;
        }

        private MaterialRecord CreateAndAddMaterialRecord(Material material, string materialName)
        {
            material.name = materialName;
            MaterialRecord record = new MaterialRecord { Material = material, ReferenceCount = 0 };
            m_materials[materialName] = record;
            return record;
        }

        public void ReleaseMaterial(string materialName)
        {
            materialName = AdjustMaterialNameForProceduralLandmark(materialName);

            if (!RequiresStreamedTexture(materialName))
            {
                return;
            }

            MaterialRecord record;

            if (m_materials.TryGetValue(materialName, out record))
            {
                if (--record.ReferenceCount == 0)
                {
                    if (m_materialsRequiringTexture.Contains(record.Material))
                    {
                        m_materialsRequiringTexture.Remove(record.Material);
                    }

                    m_materials.Remove(materialName);
                    GameObject.DestroyImmediate(record.Material);
                }
            }
            else
            {
                Debug.LogWarningFormat("material {0} was not present", materialName);
            }
        }

        private bool IsHighlightMaterialName(string materialName)
        {
            return materialName.StartsWith("Highlight") || materialName.StartsWith("entity_highlight");
        }

        public void ReleaseHighlightMaterial(string materialName)
        {
            if (IsHighlightMaterialName(materialName))
            {
                MaterialRecord record;
                if (m_materials.TryGetValue(materialName, out record))
                {
                    m_materials.Remove(materialName);
                    GameObject.DestroyImmediate(record.Material);
                }
                else
                {
                    Debug.LogWarningFormat("material {0} was not present", materialName);
                }
            }
            else
            {
                Debug.LogWarningFormat("{0} : is not a highlight material. Names must begin with Highight", materialName);
            }
        }

        private Material CreateHighlightMaterial(Color color)
        {
            Shader shader = Shader.Find("Wrld/Highlight");
            Material material = new Material(shader);
            material.SetColor("_Color", color);
            return material;
        }

        public void CreateAndAddHighlightMaterial(string materialName, Color color)
        {
            if (IsHighlightMaterialName(materialName))
            {
                MaterialRecord record;
                if (m_materials.TryGetValue(materialName, out record))
                {
                    Debug.LogWarningFormat("material {0} already exists", materialName);
                }
                else
                {
                    Material material = CreateHighlightMaterial(color);
                    CreateAndAddMaterialRecord(material, materialName);
                }
            }
            else
            {
                Debug.LogWarningFormat("{0} : highlight material names must begin with Highight", materialName);
            }
        }

        public void SetHighlightMaterialColor(string materialName, Color color)
        {
            if (IsHighlightMaterialName(materialName))
            {
                MaterialRecord record;
                if (m_materials.TryGetValue(materialName, out record))
                {
                    record.Material.SetColor("_Color", color);
                }
                else
                {
                    Debug.LogWarningFormat("material {0} was not present", materialName);
                }
            }
            else
            {
                Debug.LogWarningFormat("{0} : is not a highlight material. Names must begin with Highight", materialName);
            }
        }

        struct ApplyTextureToMaterialRequest
        {
            public string MaterialName { get; set; }
            public uint TextureID { get; set; }
        }

        class MaterialRecord
        {
            public Material Material { get; set; }
            public uint ReferenceCount { get; set; }
        }

        private HashSet<Material> m_materialsRequiringTexture = new HashSet<Material>();
        private HashSet<Material> m_materialsToRemove = new HashSet<Material>();

        private Dictionary<string, MaterialRecord> m_materials;
        private Material m_defaultMaterial;
        private Material m_defaultRasterTerrainMaterial;
        private Material m_defaultInteriorMaterial;
        private TextureLoadHandler m_textureLoadHandler;
        private string m_materialDirectory = null;
    };

}
