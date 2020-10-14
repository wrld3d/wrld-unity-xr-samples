using System;
using System.Runtime.InteropServices;
using Wrld.Common.Maths;

namespace Wrld.Transport
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct TransportPositionerOptionsInterop
    {
        public double InputLatitudeDegrees;
        public double InputLongitudeDegrees;
        [MarshalAs(UnmanagedType.I1)]
        public bool HasHeading;
        public double InputHeadingDegrees;
        public double MaxDistanceToMatchedPointMeters;
        public double MaxHeadingDeviationToMatchedPointDegrees;
        public TransportNetworkType TransportNetworkType;
    };

    [StructLayout(LayoutKind.Sequential)]
    struct MortonKeyInterop
    {
        public long Value;
    };

    [StructLayout(LayoutKind.Sequential)]
    struct TransportNodeIdInterop
    {
        public MortonKeyInterop CellKey;
        public int LocalNodeId;
        public TransportNetworkType TransportNetworkType;
    };

    [StructLayout(LayoutKind.Sequential)]
    struct TransportDirectedEdgeIdInterop
    {
        public MortonKeyInterop CellKey;
        public int LocalDirectedEdgeId;
        public TransportNetworkType TransportNetworkType;
    };

    [StructLayout(LayoutKind.Sequential)]
    struct TransportWayIdInterop
    {
        public MortonKeyInterop CellKey;
        public int LocalWayId;
        public TransportNetworkType TransportNetworkType;
    };

    [StructLayout(LayoutKind.Sequential)]
    struct TransportNodeInterop
    {
        public TransportNodeIdInterop Id;
        public DoubleVector3 Point;
        public int IncidentDirectedEdgeIdsSize;
        public IntPtr IncidentDirectedEdgeIds; // TransportDirectedEdgeIdInterop[]
    };

    [StructLayout(LayoutKind.Sequential)]
    struct TransportDirectedEdgeInterop
    {
        public TransportDirectedEdgeIdInterop Id;
        public TransportNodeIdInterop NodeIdA;
        public TransportNodeIdInterop NodeIdB;
        public TransportWayIdInterop WayId;
        [MarshalAs(UnmanagedType.I1)]
        public bool IsWayReversed;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct TransportWayInterop
    {
        public TransportWayIdInterop Id;
        public int CenterLinePointsBufferSize;
        public int CenterLineSplineParamsBufferSize;
        public int ClassificationBufferSize;
        public IntPtr CenterLinePoints; // DoubleVector3[]
        public IntPtr CenterLineSplineParams; // double[]
        public IntPtr Classification; // char[]
        public double LengthMeters;
        public double HalfWidthMeters;        
        public double AverageSpeedKph;
        public double ApproximateSpeedLimitKph;
        public TransportWayDirection WayDirection;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TransportPositionerPointOnGraphInterop
    {
        [MarshalAs(UnmanagedType.I1)]
        public bool IsMatched;
        [MarshalAs(UnmanagedType.I1)]
        public bool IsWayReversed;
        public TransportDirectedEdgeIdInterop DirectedEdgeId;
        public TransportWayIdInterop WayId;
        public double ParameterizedPointOnWay;
        public DoubleVector3 PointOnWay;
        public DoubleVector3 DirectionOnWay;
        public double HeadingOnWayDegrees;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TransportPathfindOptionsInterop
    {
        public TransportDirectedEdgeIdInterop DirectedEdgeA;
        public TransportDirectedEdgeIdInterop DirectedEdgeB;
        public double ParameterizedPointOnEdgeA;
        public double ParameterizedPointOnEdgeB;
        [MarshalAs(UnmanagedType.I1)]
        public bool UTurnAllowedAtA;
        [MarshalAs(UnmanagedType.I1)]
        public bool UTurnAllowedAtB;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TransportPathfindResultInterop
    {
        public int PathfindResultId;
        [MarshalAs(UnmanagedType.I1)]
        public bool IsPathFound;
        public int PathDirectedEdgesSize;
        public IntPtr PathDirectedEdges; // TransportDirectedEdgeIdInterop[PathDirectedEdgesSize]
        public double FirstEdgeParam;
        public double LastEdgeParam;
        public double DistanceMeters;
        public int PathPointsSize;
        public IntPtr PathPoints; // DoubleVector3[PathPointsSize]
        public IntPtr PathPointParams; // double[PathPointsSize]
    }
}
