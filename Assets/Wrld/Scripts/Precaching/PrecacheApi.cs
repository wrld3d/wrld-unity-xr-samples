using System;
using System.Collections.Generic;
using Wrld.Space;

namespace Wrld.Precaching
{
    /// <summary>
    /// The type of callback called whenever a precaching operation completes or is canceled
    /// </summary>
    /// <param name="precacheOperationResult">the status of the precache operation</param>
    public delegate void PrecacheOperationCompletedCallback(PrecacheOperationResult precacheOperationResult);

    /// <summary>
    /// API to allow users to precache areas of the map for later use. When a map resource is first downloaded 
    /// it is added to a cache from which it will be retrieved if it is later requested again.  This API allows 
    /// users to request all resources for a given area, priming this cache and avoiding the need to redownload 
    /// these resources.
    /// </summary>
    public class PrecacheApi
    {
        PrecacheApiInternal m_precacheApiInternal;
        Dictionary<int, PrecacheOperation> m_precacheOperations = new Dictionary<int, PrecacheOperation>();
        readonly double m_maximumPrecacheRadius;

        internal PrecacheApi(PrecacheApiInternal precacheApiInternal)
        {
            m_precacheApiInternal = precacheApiInternal;
            m_precacheApiInternal.OnPrecacheOperationCompleted += NotifyPrecacheOperationCompleted;
            m_maximumPrecacheRadius = m_precacheApiInternal.GetMaximumPrecacheRadius();
        }

        /// <summary>
        /// Begin an operation to precache a spherical area of the map. This allows that area to load faster in future.
        /// </summary>
        /// <param name="center">the center of the area to precache</param>
        /// <param name="radius">the radius (in meters) of the area to precache</param>
        /// <param name="completionCallback">the callback to call whenever the precache operation completes</param>
        /// <returns>an object with a Cancel() method to allow cancellation of the precache operation</returns>
        public PrecacheOperation Precache(LatLong center, double radius, PrecacheOperationCompletedCallback completionCallback)
        {
            if (radius < 0.0 || radius > MaximumPrecacheRadius)
            {
                throw new ArgumentOutOfRangeException("radius", string.Format("radius outside of valid (0, {0}] range.", MaximumPrecacheRadius));
            }

            int operationId = m_precacheApiInternal.BeginPrecacheOperation(center, radius);
            var operation = new PrecacheOperation(m_precacheApiInternal, operationId, completionCallback);
            m_precacheOperations.Add(operationId, operation);

            return operation;
        }

        /// <summary>
        /// Return the maximum radius that can be passed to Precache(center, radius, callback) without causing an exception, in meters.
        /// </summary>
        /// <returns>the maximum radius, in meters</returns>
        public double MaximumPrecacheRadius
        {
            get
            {
                return m_maximumPrecacheRadius;
            }
        }

        private void NotifyPrecacheOperationCompleted(int operationId, PrecacheOperationResult result)
        {
            PrecacheOperation operation;

            if (m_precacheOperations.TryGetValue(operationId, out operation))
            {
                m_precacheOperations.Remove(operationId);
                operation.NotifyComplete(result);                
            }
        }
    }
}