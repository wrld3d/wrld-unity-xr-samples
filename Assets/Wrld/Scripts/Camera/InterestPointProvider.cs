using Wrld.Common.Maths;
using UnityEngine;
using Wrld.Space;
using System;

namespace Wrld.MapCamera
{
    class InterestPointProvider
    {
        private const double MaximumInterestPointAltitude = EarthConstants.Radius + 9000.0;
        private const double MaximumInterestPointAltitudeSquared = MaximumInterestPointAltitude * MaximumInterestPointAltitude;

        private DoubleVector3 m_interestPointECEF;
        private bool m_hasInterestPointFromNativeController;
        private Transform m_mapTransform;
        
        internal InterestPointProvider(Transform mapTransform)
        {
            m_mapTransform = mapTransform;
        }

        public void UpdateFromNative(DoubleVector3 interestPointECEF)
        {
            m_interestPointECEF = interestPointECEF;
            m_hasInterestPointFromNativeController = true;
        }

        public DoubleVector3 CalculateInterestPoint(Camera cameraECEF, DoubleVector3 cameraOriginECEF)
        {
            if (m_hasInterestPointFromNativeController)
            {
                m_hasInterestPointFromNativeController = false;
                return m_interestPointECEF;
            }

            var cameraToMapSpaceMatrix = m_mapTransform.worldToLocalMatrix * cameraECEF.transform.localToWorldMatrix;
            var mapSpaceViewDirection = cameraToMapSpaceMatrix.MultiplyVector(Vector3.forward);

            return CalculateEstimatedInterestPoint(mapSpaceViewDirection, cameraECEF.nearClipPlane, cameraECEF.farClipPlane, cameraOriginECEF);
        }

        private DoubleVector3 CalculateEstimatedInterestPoint(Vector3 mapSpaceViewDirection, float nearClipPlane, float farClipPlane, DoubleVector3 cameraOriginECEF)
        {
            DoubleVector3 finalCameraPositionECEF = cameraOriginECEF;
            DoubleVector3 estimatedInterestPointECEF = finalCameraPositionECEF + mapSpaceViewDirection * (nearClipPlane + farClipPlane) * 0.5f;
            ClampInterestPointToValidRangeIfRequired(ref estimatedInterestPointECEF);

            return estimatedInterestPointECEF;
        }

        public static bool ClampInterestPointToValidRangeIfRequired(ref DoubleVector3 interestPointEcef)
        {
            double magnitudeSquared = interestPointEcef.sqrMagnitude;

            if (magnitudeSquared > MaximumInterestPointAltitudeSquared)
            {
                interestPointEcef *= MaximumInterestPointAltitude / Math.Sqrt(magnitudeSquared);

                return true;
            }

            return false;
        }
    }
}