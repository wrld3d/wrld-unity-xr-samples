using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using Wrld.Interop;

namespace Wrld.Resources.Labels
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct LabelCreateOptionsInterop
    {
        public IntPtr LabelID;

        public IntPtr Text;
        public ColorInterop HaloColor;
        public int BaseFontSize;
        public double FontScale;

        public ushort iconTexturePage;
        public double iconU0;
        public double iconV0;
        public double iconU1;
        public double iconV1;

        public double iconWidth;
        public double iconHeight;

        [MarshalAs(UnmanagedType.I1)]
        public bool HasTextComponent;
        [MarshalAs(UnmanagedType.I1)]
        public bool HasIconComponent;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct LabelUpdateStateInterop
    {
        public IntPtr LabelID;

        public ColorInterop TextColor;
        public Vector2 TextPosition;
        public double TextRotationAngleDegrees;

        public ColorInterop IconColor;
        public Vector2 IconPosition;
        public double IconRotationAngleDegrees;
    }

}
