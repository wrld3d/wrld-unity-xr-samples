using Wrld.Common.Maths;
using Wrld.Materials;
using Wrld.Space;
using UnityEngine;

namespace Wrld.Streaming
{
    public class GameObjectStreamer
    {
        GameObjectRepository m_gameObjectRepository;
        MaterialRepository m_materialRepository;
        GameObjectFactory m_gameObjectCreator;

        private CollisionStreamingType m_collisions;
        private bool m_shouldUploadToGPU;

        public GameObjectStreamer(string rootObjectName, MaterialRepository materialRepository, Transform parentForStreamedObjects, CollisionStreamingType collisions, bool supportsFlattening, bool shouldUploadToGPU)
        {
            m_materialRepository = materialRepository;
            m_gameObjectRepository = new GameObjectRepository(rootObjectName, parentForStreamedObjects, materialRepository, supportsFlattening);
            m_gameObjectCreator = new GameObjectFactory();
            m_collisions = collisions;
            m_shouldUploadToGPU = shouldUploadToGPU;
        }

        public void Destroy()
        {
            m_gameObjectRepository.DestroyAllGameObjects();
            Object.Destroy(m_gameObjectRepository.Root);
        }

        public GameObject[] AddObjectsForMeshes(string objectID, Mesh[] meshes, DoubleVector3 originECEF, Vector3 translationOffsetECEF, Quaternion rotationECEF, string materialName)
        {
            if (m_gameObjectRepository.Contains(objectID))
            {
                return null;
            }

            var material = m_materialRepository.LoadOrCreateMaterial(objectID, materialName);
            var parent = new GameObject(objectID);
            parent.transform.SetParent(m_gameObjectRepository.Root.transform, false);
            var gameObjects = m_gameObjectCreator.CreateGameObjects(meshes, material, parent.transform, m_collisions, m_shouldUploadToGPU);

            m_gameObjectRepository.Add(objectID, originECEF, translationOffsetECEF, rotationECEF, parent);

            return gameObjects;
        }

        public bool RemoveObjects(string objectID)
        {
            return m_gameObjectRepository.Remove(objectID);
        }

        public GameObject GetObject(string objectID)
        {
            GameObject gameObject;
            m_gameObjectRepository.TryGetGameObject(objectID, out gameObject);
            return gameObject;
        }

        public void UpdateTransforms(ITransformUpdateStrategy transformUpdateStrategy, float heightOffset = 0.0f)
        {
            m_gameObjectRepository.UpdateTransforms(transformUpdateStrategy, heightOffset);
        }

        public void SetVisible(string objectID, bool visible)
        {
            GameObject gameObject;

            if (m_gameObjectRepository.TryGetGameObject(objectID, out gameObject))
            {
                #pragma warning disable 618
                // SetActive is now recommended in place of SetActiveRecursively, but they do subtly different things.
                // The correct fix for this would be to write our own version of SetActiveRecursively, but for now
                // we're a bit too close to a release for that to be safe.
                gameObject.SetActiveRecursively(visible);
                #pragma warning restore 618
            }
        }

        public void ChangeCollision(CollisionStreamingType collision)
        {
            m_collisions = collision;
        }

        // :TODO: Think about moving these out to some kind of controller type that also consumes the repository
        internal void SetTranslation(string objectID, Vector3 translation)
        {
            GameObjectRecord record = m_gameObjectRepository.GetObjectRecord(objectID);

            if (record != null)
            {
                record.TranslationOffsetECEF = translation;
                record.RootGameObject.transform.localPosition = translation;
            }
        }

        internal void SetOrientation(string objectID, Quaternion orientationECEF)
        {
            GameObjectRecord record = m_gameObjectRepository.GetObjectRecord(objectID);

            if (record != null)
            {
                record.OrientationECEF = orientationECEF;
                record.RootGameObject.transform.localRotation = orientationECEF;
            }
        }

        internal void SetColor(string objectID, Color color)
        {
            var rootObject = GetObject(objectID);

            if (rootObject != null)
            {
                var meshRenderers = rootObject.GetComponentsInChildren<MeshRenderer>(true);

                foreach (MeshRenderer meshRenderer in meshRenderers)
                {
                    if (meshRenderer != null)
                    {
                        // https://docs.unity3d.com/Manual/MaterialsAccessingViaScript.html
                        const string fadeRenderMode = "_ALPHABLEND_ON";

                        if (color.a >= 1.0f)
                        {
                            meshRenderer.sharedMaterial.DisableKeyword(fadeRenderMode);
                            meshRenderer.enabled = true;
                        }
                        else if (color.a > 0.0f)
                        {
                            meshRenderer.sharedMaterial.EnableKeyword(fadeRenderMode);
                            meshRenderer.enabled = true;
                        }
                        else
                        {
                            meshRenderer.enabled = false;
                        }

                        meshRenderer.sharedMaterial.color = color;
                    }
                }
            }
        }

        internal void SetScale(string objectID, Vector3 scale)
        {
            var gameObject = GetObject(objectID);

            if (gameObject != null)
            {
                int childCount = gameObject.transform.childCount;

                for (int childIndex = 0; childIndex < childCount; ++childIndex)
                {
                    var child = gameObject.transform.GetChild(childIndex);
                    child.localScale = scale;
                }
            }
        }
    }
}
