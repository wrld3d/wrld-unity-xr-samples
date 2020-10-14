using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Wrld.Interop;
using Wrld.Space;


namespace Wrld.Resources.Buildings
{  
    internal enum BuildingHighlightSelectionMode
    {
        SelectAtLocation,
        SelectAtScreenPoint
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct BuildingHighlightCreateParamsInterop
    {
        public BuildingHighlightSelectionMode SelectionMode;
        public LatLongInterop Location;
        public Vector2 ScreenPoint;
        public ColorInterop HighlightColor;
        [MarshalAs(UnmanagedType.I1)]
        public bool ShouldCreateView;
    };

    [StructLayout(LayoutKind.Sequential)]
    internal struct BuildingDimensionsInterop
    {
        public double BaseAltitude;
        public double TopAltitude;
        public LatLongInterop Centroid;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct BuildingContourInterop
    {
        public int PointCount;
        public int Padding;
        public double BottomAltitude;
        public double TopAltitude;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct BuildingInformationInterop
    {
        public IntPtr BuildingId;       // string
        public int BuildingIdSize;
        public IntPtr ContourPoints;    // LatLongInterop[]
        public int ContourPointsSize;
        public IntPtr BuildingContours; //  BuildingContourInterop[]
        public int BuildingContoursSize;
        public BuildingDimensionsInterop BuildingDimensions;
    }

    internal static class BuildingsApiInteropExtensions
    {
        internal static LatLongAltitude ToLatLongAltitude(this LatLongAltitudeInterop interop)
        {
            return new LatLongAltitude(interop.LatitudeDegrees, interop.LongitudeDegrees, interop.Altitude);
        }

        public static BuildingDimensions ToBuildingDimensions(this BuildingDimensionsInterop interop)
        {
            return new BuildingDimensions(
                interop.BaseAltitude,
                interop.TopAltitude,
                interop.Centroid.ToLatLong());
        }

        [Serializable]
        internal struct BuildingDimensionsDto
        {
            public double baseAltitude;
            public double topAltitude;
            public LatLongInterop centroid;
        }

        [Serializable]
        internal struct BuildingContourDto
        {
            public double bottomAltitude;
            public double topAltitude;
            public LatLongInterop[] points;
        }

        [Serializable]
        internal struct BuildingInformationDto
        {
            public string buildingId;
            public BuildingDimensionsDto buildingDimensions;
            public BuildingContourDto[] buildingContours;
        }

        public static BuildingDimensionsDto ToBuildingDimensionsDto(this BuildingDimensions buildingDimensions)
        {
            return new BuildingDimensionsDto
            {
                baseAltitude = buildingDimensions.BaseAltitude,
                topAltitude = buildingDimensions.TopAltitude,
                centroid = buildingDimensions.Centroid.ToLatLongInterop()
            };
        }

        public static BuildingContourDto ToBuildingContourDto(this BuildingContour buildingContour)
        {
            return new BuildingContourDto
            {
                bottomAltitude = buildingContour.BottomAltitude,
                topAltitude = buildingContour.TopAltitude,
                points = buildingContour.Points.Select(_x => _x.ToLatLongInterop()).ToArray()
            };
        }

        public static BuildingInformationDto ToBuildingInformationDto(this BuildingInformation buildingInformation)
        {
            return new BuildingInformationDto()
            {
                buildingId = buildingInformation.BuildingId,
                buildingDimensions = buildingInformation.BuildingDimensions.ToBuildingDimensionsDto(),
                buildingContours = buildingInformation.BuildingContours.Select(_x => _x.ToBuildingContourDto()).ToArray()
            };
        }
    }
}
