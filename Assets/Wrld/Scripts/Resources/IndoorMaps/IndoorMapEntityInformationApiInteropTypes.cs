using System;
using System.Runtime.InteropServices;
using Wrld.Common.Maths;
using Wrld.Interop;

namespace Wrld.Resources.IndoorMaps
{
    [StructLayout(LayoutKind.Sequential)]
    struct IndoorMapEntityInterop
    {
        public int indoorMapEntityModelId;
        public IntPtr indoorMapEntityId;
        public int indoorMapEntityIdBufferSize;
        public int indoorMapFloorId;
        public LatLongInterop position;
    };

    [StructLayout(LayoutKind.Sequential)]
    internal struct IndoorMapEntityInformationInterop
    {
        public int indoorMapEntityInformationModelId;
        public IntPtr indoorMapEntityModelIds;
        public int indoorMapEntityModelIdsBufferSize;
        public IndoorMapEntityLoadState indoorMapEntityLoadState;
    };

}
