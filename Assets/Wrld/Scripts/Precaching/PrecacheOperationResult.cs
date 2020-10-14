namespace Wrld.Precaching
{
    /// <summary>
    /// A result of a precache operation. Returned whenever a precache operation completes via the completion
    /// handler passed to PrecacheApi.Precache.
    /// </summary>
    public class PrecacheOperationResult
    {
        internal PrecacheOperationResult(bool succeeded)
        {
            Succeeded = succeeded;
        }

        /// <summary>
        ///  A bool indicating whether the precache operation succeeded or not.
        /// </summary>
        public bool Succeeded { get; private set; }
    }
}

