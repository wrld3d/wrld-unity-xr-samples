using System;
using System.Runtime.InteropServices;

namespace Wrld.Utilities
{
    internal static class NativeInteropHelpers
    {
        public static IntPtr AllocateNativeHandleForObject(object input)
        {
            var handle = GCHandle.Alloc(input);
            return GCHandle.ToIntPtr(handle);
        }

        public static void FreeNativeHandle(IntPtr handlePointer)
        {
            var handle = GCHandle.FromIntPtr(handlePointer);
            handle.Free();
        }
    }
}



