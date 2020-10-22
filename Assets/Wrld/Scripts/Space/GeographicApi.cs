using System.Collections.Generic;
using UnityEngine;


namespace Wrld.Space
{
    public class GeographicApi
    {
        List<GeographicTransform> m_geographicTransforms = new List<GeographicTransform>();
        Transform m_rootTransform;

        internal GeographicApi(Transform rootTransform)
        {
            m_rootTransform = rootTransform;
        }

        internal void UpdateTransforms(ITransformUpdateStrategy updateStrategy)
        {
            foreach (var geographicTransform in m_geographicTransforms)
            {
                geographicTransform.UpdateTransform(updateStrategy);
            }
        }

        internal void Destroy()
        {
            var toBeDestroyed = m_geographicTransforms.ToArray();

            foreach (var geographicTransform in toBeDestroyed)
            {
                geographicTransform.OnDestroy();
            }
        }

        /// <summary>
        /// Register a GeographicTransform object to have its position updated by the API.  This object should be
        /// a child of your WRLDMap object. If the object is not a child of your WRLDMap object, this function will 
        /// re-parent the object, along with any pre-existing parents.
        /// </summary>
        /// <param name="geographicTransform">The GeographicTransform object to register and start updating.</param>
        public void RegisterGeographicTransform(GeographicTransform geographicTransform)
        {
            if (!ValidateIsChildOfRoot(geographicTransform.transform))
            {
                GetTopmostParentTransform(geographicTransform.transform).SetParent(m_rootTransform);
            }

            m_geographicTransforms.Add(geographicTransform);
        }

        /// <summary>
        /// Unregister a GeographicTransform and stop updating its position.
        /// </summary>
        /// <param name="geographicTransform"> The GeographicTransform object to stop updating.</param>
        public void UnregisterGeographicTransform(GeographicTransform geographicTransform)
        {
            m_geographicTransforms.Remove(geographicTransform);
        }

        private bool ValidateIsChildOfRoot(Transform transform)
        {
            Transform current = transform.parent;

            while (current != null)
            {
                if (current == m_rootTransform)
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private Transform GetTopmostParentTransform(Transform transform)
        {
            Transform topmost = transform;

            while (topmost.parent != null)
            {
                topmost = topmost.parent;
            }

            return topmost;
        }
    }
}
