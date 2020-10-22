using Wrld.Common.Maths;
using UnityEngine;

namespace Wrld.Space
{
    public class UnityWorldSpaceTransformUpdateStrategy : ITransformUpdateStrategy
    {
        private UnityWorldSpaceCoordinateFrame m_frame;
        private float m_flattenScale;

        public UnityWorldSpaceTransformUpdateStrategy(UnityWorldSpaceCoordinateFrame frame, float scale)
        {
            m_frame = frame;
            m_flattenScale = scale;
        }

        public void UpdateTransform(Transform objectTransform, DoubleVector3 objectOriginECEF, Vector3 translationOffsetECEF, Quaternion orientationECEF, float heightOffset, bool applyFlattening)
        {
            var finalPositionECEF = objectOriginECEF + translationOffsetECEF;
            var resourceUp = m_frame.ECEFToLocalRotation * objectOriginECEF.normalized.ToSingleVector();
            var localPosition = m_frame.ECEFToLocalSpace(finalPositionECEF) + Vector3.up * heightOffset;
            var localRotation = m_frame.ECEFToLocalRotation * orientationECEF;

            if (applyFlattening && m_flattenScale != 1.0f)
            {
                var resourceToLocal = Quaternion.FromToRotation(resourceUp, Vector3.up);
                var localToResource = Quaternion.Inverse(resourceToLocal);
                var innerRotation = resourceToLocal * localRotation;
                var scaleVec = new Vector3(1, m_flattenScale, 1);

                TransformHelper.ApplyTransform(objectTransform, localPosition, scaleVec, localToResource, innerRotation);
            }
            else
            {
                TransformHelper.ApplyTransform(objectTransform, localPosition, Vector3.one, localRotation, Quaternion.identity);
            }
        }

        public void UpdateStrategy(DoubleVector3 originECEF, float environmentScale)
        {
            m_frame.SetCentralPoint(originECEF);
            m_flattenScale = environmentScale;
        }
    }
}

