using UnityEngine;

namespace Wrld
{
    public class ChildRestorer : MonoBehaviour
    {
        private GameObject m_intermediateGameObject;

        private void Start()
        {
            m_intermediateGameObject = new GameObject("ChildRestorer::m_intermediateGameObject for " + gameObject.name);
            m_intermediateGameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            Destroy(m_intermediateGameObject);
        }

        private void Update()
        {
            if (m_intermediateGameObject && m_intermediateGameObject.transform.childCount > 0)
            {
                int childCount = m_intermediateGameObject.transform.childCount;

                for (int childIndex = childCount - 1; childIndex >= 0; --childIndex)
                {
                    var child = m_intermediateGameObject.transform.GetChild(childIndex);
                    child.SetParent(transform, false);
                }
            }
        }

        public void RestoreChild(Transform child)
        {
            if (m_intermediateGameObject)
                child.SetParent(m_intermediateGameObject.transform, false);
        }

        public void RestoreChildren(Transform currentParent)
        {
            int childCount = currentParent.childCount;

            if (childCount > 0)
            {
                for (int childIndex = childCount - 1; childIndex >= 0; --childIndex)
                {
                    var child = currentParent.GetChild(childIndex);
                    RestoreChild(child);
                }
            }

            ChildRestorer other = currentParent.gameObject.GetComponent<ChildRestorer>();
            if (other != null)
                RestoreChildren(other.m_intermediateGameObject.transform);
        }
    }
}
