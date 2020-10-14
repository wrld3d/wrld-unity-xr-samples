using System;

namespace Wrld.Precaching
{
    /// <summary>
    /// A handle to an ongoing precache operation.
    /// </summary>
    public class PrecacheOperation
    {
        PrecacheApiInternal m_internalApi;
        int m_operationId;
        PrecacheOperationCompletedCallback m_completionCallback;

        internal PrecacheOperation(PrecacheApiInternal internalApi, int operationId, PrecacheOperationCompletedCallback completionCallback)
        {
            m_internalApi = internalApi;
            m_operationId = operationId;
            m_completionCallback = completionCallback;
        }

        /// <summary>
        /// Cancels this precache operation if it has not yet been completed.
        /// </summary>
        public void Cancel()
        {
            m_internalApi.CancelPrecacheOperation(m_operationId);
        }

        internal void NotifyComplete(PrecacheOperationResult result)
        {
            if (m_completionCallback != null)
            {
                m_completionCallback(result);
            }
        }
    }
}