using Wrld.Common.Maths;
using Wrld.Materials;
using Wrld.Space;
using System.Collections.Generic;
using UnityEngine;

namespace Wrld.Streaming
{
    public class GameObjectRecord
    {
        public DoubleVector3 OriginECEF { get; set; }
        public Quaternion OrientationECEF { get; set; }

        public Vector3 TranslationOffsetECEF { get; set; }

        public GameObject RootGameObject { get; set; }
    }

    public class GameObjectRepository
    {
        private Dictionary<string, GameObjectRecord> m_gameObjectsById = new Dictionary<string, GameObjectRecord>();
        private GameObject m_root;
        private MaterialRepository m_materialRepository;
        private bool m_applyFlattening;

        public GameObject Root { get { return m_root; } }

        public GameObjectRepository(string rootName, Transform parentForStreamedObjects, MaterialRepository materialRepository, bool applyFlattening)
        {
            m_root = new GameObject(rootName);
            m_root.transform.SetParent(parentForStreamedObjects, false);
            m_materialRepository = materialRepository;
            m_applyFlattening = applyFlattening;
        }

        public void Add(string id, DoubleVector3 originECEF, Vector3 translationOffsetECEF, Quaternion orientationECEF, GameObject rootGameObject)
        {
            if (Contains(id))
            {
                return; // :TODO: fix
            }

            var record = new GameObjectRecord { OriginECEF = originECEF, TranslationOffsetECEF = translationOffsetECEF, OrientationECEF = orientationECEF, RootGameObject = rootGameObject };
            m_gameObjectsById.Add(id, record);
        }

        public bool Contains(string id)
        {
            return m_gameObjectsById.ContainsKey(id);
        }

        private void DestroyGameObject(GameObject gameObject)
        {
            int childCount = gameObject.transform.childCount;

            for (int childIndex = 0; childIndex < childCount; ++childIndex)
            {
                var child = gameObject.transform.GetChild(childIndex);
                DestroyGameObject(child.gameObject);
            }

            gameObject.transform.DetachChildren();

            var mesh = gameObject.GetComponent<MeshFilter>();

            if (mesh != null)
            {
                GameObject.DestroyImmediate(mesh.sharedMesh);
            }

            var meshRenderer = gameObject.GetComponent<MeshRenderer>();

            if (meshRenderer != null)
            {
                foreach (var material in meshRenderer.sharedMaterials)
                {
                    if (material != null)
                    {
                        m_materialRepository.ReleaseMaterial(material.name);
                    }
                }
            }

            GameObject.DestroyImmediate(gameObject);
        }

        public bool Remove(string id)
        {
            GameObjectRecord value;

            if (m_gameObjectsById.TryGetValue(id, out value))
            {
                m_gameObjectsById.Remove(id);
                DestroyGameObject(value.RootGameObject);

                return true;
            }

            return false;
        }

        public void DestroyAllGameObjects()
        {
            var gameObjectIdList = new List<string>(m_gameObjectsById.Keys);

            foreach(var gameObjectId in gameObjectIdList)
            {
                Remove(gameObjectId);
            }

            UnityEngine.Resources.UnloadUnusedAssets();
        }

        public void UpdateTransforms(ITransformUpdateStrategy transformUpdateStrategy, float heightOffset)
        {
            foreach (var record in m_gameObjectsById.Values)
            {
                transformUpdateStrategy.UpdateTransform(record.RootGameObject.transform, record.OriginECEF, record.TranslationOffsetECEF, record.OrientationECEF, heightOffset, m_applyFlattening);
            }
        }

        public GameObjectRecord GetObjectRecord(string objectID)
        {
            GameObjectRecord record;

            if (m_gameObjectsById.TryGetValue(objectID, out record))
            {
                return record;
            }

            return null;
        }

        public bool TryGetGameObject(string objectID, out GameObject gameObject)
        {
            GameObjectRecord record = GetObjectRecord(objectID);

            if (record != null)
            {
                gameObject = record.RootGameObject;
                return true;
            }
            else
            {
                Debug.LogFormat("Failed to get objects with id {0}", objectID);
                gameObject = null;
            }

            return false;
        }
    }
}
