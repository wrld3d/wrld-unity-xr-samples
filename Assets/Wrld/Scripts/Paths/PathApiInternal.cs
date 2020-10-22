using System;
using System.Runtime.InteropServices;
using Wrld.Interop;
using System.Collections.Generic;

namespace Wrld.Paths
{
    // Suppress unused warning for the Interop object as it is assigned to in C++ codebase but not
    // in Unity code.
    #pragma warning disable CS0649
    internal struct PointOnPathInterop
    {
       public LatLongInterop resultPoint;
       public LatLongInterop inputPoint;
       public double distancefromInputPoint;
       public double fractionAlongPath;
       public int indexOfPathSegmentStartVertex;
       public int indexOfPathSegmentEndVertex;
    };
    #pragma warning restore CS0649


    internal class PathApiInternal
    {
        public PointOnPath GetPointOnPath(Wrld.Space.LatLong inputPoint, List<Wrld.Space.LatLong> polylinePathPoints)
        {
            var pathPointsBuffer = new LatLongInterop[polylinePathPoints.Count];
            for (int i = 0; i < polylinePathPoints.Count; ++i)
            {
                pathPointsBuffer[i] = LatLongInterop.FromLatLong(polylinePathPoints[i]);
            }

            var pathPointsBufferGCHandle = GCHandle.Alloc(pathPointsBuffer, GCHandleType.Pinned);
            var bufferPtr = pathPointsBufferGCHandle.AddrOfPinnedObject();

            var inputPointInterop = LatLongInterop.FromLatLong(inputPoint);

            var resultInterop = NativePathApi_GetPointOnPath(NativePluginRunner.API, inputPointInterop, bufferPtr, pathPointsBuffer.Length);

            var result = resultInterop.FromInterop();

            pathPointsBufferGCHandle.Free();

            return result;
        }


        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern PointOnPathInterop NativePathApi_GetPointOnPath(IntPtr ptr, LatLongInterop inputPoint, IntPtr polylinePathPointsPtr, int polylinePathPointsSize);
    }
}