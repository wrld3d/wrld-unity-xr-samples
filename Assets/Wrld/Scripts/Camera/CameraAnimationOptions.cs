using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wrld.MapCamera
{
    internal class CameraAnimationOptions
    {
        public readonly double durationSeconds;
        public readonly double preferredAnimationSpeed;
        public readonly double minDuration;
        public readonly double maxDuration;
        public readonly double snapDistanceThreshold;
        public readonly bool snapIfDistanceExceedsThreshold;
        public readonly bool interruptByGestureAllowed;
        public readonly bool hasExplicitDuration;
        public readonly bool hasPreferredAnimationSpeed;
        public readonly bool hasMinDuration;
        public readonly bool hasMaxDuration;
        public readonly bool hasSnapDistanceThreshold;

        private CameraAnimationOptions(
            double durationSeconds,
            double preferredAnimationSpeed,
            double minDuration,
            double maxDuration,
            double snapDistanceThreshold,
            bool snapIfDistanceExceedsThreshold,
            bool interruptByGestureAllowed,
            bool hasExplicitDuration,
            bool hasPreferredAnimationSpeed,
            bool hasMinDuration,
            bool hasMaxDuration,
            bool hasSnapDistanceThreshold
        )
        {
            this.durationSeconds = durationSeconds;
            this.preferredAnimationSpeed = preferredAnimationSpeed;
            this.minDuration = minDuration;
            this.maxDuration = maxDuration;
            this.snapDistanceThreshold = snapDistanceThreshold;
            this.snapIfDistanceExceedsThreshold = snapIfDistanceExceedsThreshold;
            this.interruptByGestureAllowed = interruptByGestureAllowed;

            this.hasExplicitDuration = hasExplicitDuration;
            this.hasPreferredAnimationSpeed = hasPreferredAnimationSpeed;
            this.hasMinDuration = hasMinDuration;
            this.hasMaxDuration = hasMaxDuration;
            this.hasSnapDistanceThreshold = hasSnapDistanceThreshold;
        }

        public class Builder
        {

            private double m_durationSeconds = 0.0;
            private double m_preferredAnimationSpeed = 0.0;
            private double m_minDuration = 0.0;
            private double m_maxDuration = 0.0;
            private double m_snapDistanceThreshold = 0.0;
            private bool m_snapIfDistanceExceedsThreshold = true;
            private bool m_interruptByGestureAllowed = true;


            private bool m_hasExplicitDuration = false;
            private bool m_hasPreferredAnimationSpeed = false;
            private bool m_hasMinDuration = false;
            private bool m_hasMaxDuration = false;
            private bool m_hasSnapDistanceThreshold = false;


            public Builder Duration(double? durationSeconds)
            {
                if (durationSeconds.HasValue)
                {
                    return Duration(durationSeconds.Value);
                }
                else
                {
                    m_durationSeconds = 0.0;
                    m_hasExplicitDuration = false;
                }
                return this;
            }

            public Builder Duration(double durationSeconds)
            {
                m_durationSeconds = durationSeconds;
                m_hasExplicitDuration = true;
                return this;
            }

            public Builder PreferredAnimationSpeed(double animationSpeedMetersPerSecond)
            {
                m_preferredAnimationSpeed = animationSpeedMetersPerSecond;
                m_hasPreferredAnimationSpeed = true;
                m_hasExplicitDuration = false;
                return this;
            }

            public Builder SnapIfDistanceExceedsThreshold(bool shouldSnap)
            {
                m_snapIfDistanceExceedsThreshold = shouldSnap;
                return this;
            }

            public Builder InterruptByGestureAllowed(bool isAllowed)
            {
                m_interruptByGestureAllowed = isAllowed;
                return this;
            }

            public Builder MinDuration(double minDuration)
            {
                m_minDuration = minDuration;
                m_hasMinDuration = true;
                return this;
            }

            public Builder MaxDuration(double maxDuration)
            {
                m_maxDuration = maxDuration;
                m_hasMaxDuration = true;
                return this;
            }

            public Builder SnapDistanceThreshold(double snapDistanceThresholdMeters)
            {
                m_snapDistanceThreshold = snapDistanceThresholdMeters;
                m_hasSnapDistanceThreshold = true;
                return this;
            }

            public CameraAnimationOptions Build()
            {
                return new CameraAnimationOptions(
                        m_durationSeconds,
                        m_preferredAnimationSpeed,
                        m_minDuration,
                        m_maxDuration,
                        m_snapDistanceThreshold,
                        m_snapIfDistanceExceedsThreshold,
                        m_interruptByGestureAllowed,
                        m_hasExplicitDuration,
                        m_hasPreferredAnimationSpeed,
                        m_hasMinDuration,
                        m_hasMaxDuration,
                        m_hasSnapDistanceThreshold
                );
            }

        }
    }
}
