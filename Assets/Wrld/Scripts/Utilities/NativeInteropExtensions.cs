using System;
using System.Runtime.InteropServices;

namespace Wrld.Utilities
{
    internal static class NativeInteropExtensions
    {
        public static T NativeHandleToObject<T>(this IntPtr handlePointer)
        {
            var handle = GCHandle.FromIntPtr(handlePointer);
            return (T)handle.Target;
        }
    }
}

