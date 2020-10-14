using UnityEngine;

namespace Wrld.Space
{
    public class TransformHelper
    {
        public static void ApplyTransform(Transform objectTransform, Vector3 parentPosition, Vector3 parentScale, Quaternion parentRotation, Quaternion childRotation)
        {
            objectTransform.localPosition = parentPosition;
            objectTransform.localRotation = parentRotation;
            objectTransform.localScale = parentScale;

            int childCount = objectTransform.childCount;

            for (int childIndex = 0; childIndex < childCount; ++childIndex)
            {
                var child = objectTransform.GetChild(childIndex);
                child.localRotation = childRotation;
            }
        }
    }
}

