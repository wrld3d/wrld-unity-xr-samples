using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Wrld.Common.Maths;
using Wrld.Space;
using Wrld.Materials;
using Assets.Wrld.Scripts.Maths;
using Wrld.Common.Camera;
using Wrld.Utilities;
using Wrld.Interop;

namespace Wrld.Resources.Buildings
{
    internal class BuildingsApiInternal
    {
        private MaterialRepository m_materialRepository;
        private const string HighlightMaterialPrefix = "Highlight_Building_";
        private IDictionary<int, BuildingHighlight> m_buildingHighlightIdToObject = new Dictionary<int, BuildingHighlight>();
        private IntPtr m_handleToSelf;

        internal BuildingsApiInternal(MaterialRepository materialRepository)
        {
            m_materialRepository = materialRepository;
            m_handleToSelf = NativeInteropHelpers.AllocateNativeHandleForObject(this);
        }

        internal void Destroy()
        {
            NativeInteropHelpers.FreeNativeHandle(m_handleToSelf);
        }
        internal IntPtr GetHandle()
        {
            return m_handleToSelf;
        }

        /// <summary>
        /// Creates a building highlight and adds it to the WrldMap.
        /// </summary>
        /// <param name="buildingHighlightOptions">The BuildingHighlightOptions object specifying how to create the BuildingHighlight object.</param>
        /// <returns>The new BuildingHighlight object</returns>
        public BuildingHighlight CreateHighlight(BuildingHighlightOptions buildingHighlightOptions)
        {
            var createParamsInterop = new BuildingHighlightCreateParamsInterop
            {
                SelectionMode = buildingHighlightOptions.GetSelectionMode(),
                Location = buildingHighlightOptions.GetSelectionLocation().ToLatLongInterop(),
                ScreenPoint = CameraHelpers.ScreenPointOriginTopLeft(buildingHighlightOptions.GetSelectionScreenPoint()),
                HighlightColor = buildingHighlightOptions.GetColor().ToColorInterop(),
                ShouldCreateView = !buildingHighlightOptions.IsInformationOnly()
            };

            var highlightId = NativeBuildingsApi_CreateHighlight(NativePluginRunner.API, ref createParamsInterop);
            m_materialRepository.CreateAndAddHighlightMaterial(MakeMaterialId(highlightId), buildingHighlightOptions.GetColor());
            var buildingHighlight = new BuildingHighlight(
                this,
                highlightId,
                buildingHighlightOptions.GetColor(),
                buildingHighlightOptions.IsInformationOnly(),
                buildingHighlightOptions.GetBuildingInformationReceivedHandler());

            m_buildingHighlightIdToObject.Add(highlightId, buildingHighlight);

            var buildingInformation = TryFetchBuildingInformationForHighlight(buildingHighlight.Id);
            if (buildingInformation != null)
            {
                buildingHighlight.SetBuildingInformation(buildingInformation);
            }

            return buildingHighlight;
        }

        private string MakeMaterialId(int highlightId)
        {
            return HighlightMaterialPrefix + highlightId;
        }

        public void DestroyHighlight(BuildingHighlight buildingHighlight)
        {
            if (!m_buildingHighlightIdToObject.ContainsKey(buildingHighlight.Id))
            {
                return;
            }

            m_buildingHighlightIdToObject.Remove(buildingHighlight.Id);
            NativeBuildingsApi_DestroyHighlight(NativePluginRunner.API, buildingHighlight.Id);
            m_materialRepository.ReleaseHighlightMaterial(MakeMaterialId(buildingHighlight.Id));
        }

        public bool TryFindIntersectionWithBuilding(DoubleRay rayEcef, out LatLongAltitude out_intersectionPoint)
        {
            LatLongAltitudeInterop intersectionPointInterop;
            bool didIntersect = NativeBuildingsApi_TryFindIntersectionWithBuilding(NativePluginRunner.API, ref rayEcef.origin, ref rayEcef.direction, out intersectionPointInterop);
            out_intersectionPoint = intersectionPointInterop.ToLatLongAltitude();
            return didIntersect;
        }

        public bool TryFindIntersectionAndNormalWithBuilding(DoubleRay rayEcef, out LatLongAltitude out_intersectionPoint, out DoubleVector3 out_intersectionNormal)
        {
            LatLongAltitudeInterop intersectionPointInterop;

            bool didIntersect = NativeBuildingsApi_TryFindIntersectionAndNormalWithBuilding(NativePluginRunner.API, ref rayEcef.origin, ref rayEcef.direction, out intersectionPointInterop, out out_intersectionNormal);
            out_intersectionPoint = intersectionPointInterop.ToLatLongAltitude();
            return didIntersect;
        }

        public void SetHighlightColor(BuildingHighlight buildingHighlight, Color color)
        {
            var buildingHighlightId = buildingHighlight.Id;
            if (!m_buildingHighlightIdToObject.ContainsKey(buildingHighlightId))
            {
                return;
            }
            var colorInterop = color.ToColorInterop();
            NativeBuildingsApi_SetHighlightStyleAttributes(NativePluginRunner.API, buildingHighlightId, ref colorInterop);
            // currently bypasses platform round-trip
            m_materialRepository.SetHighlightMaterialColor(HighlightMaterialPrefix + buildingHighlightId, color);
        }

        public void NotifyBuildingHighlightChanged(int buildingHighlightId)
        {
            if (m_buildingHighlightIdToObject.ContainsKey(buildingHighlightId))
            {
                var buildingHighlight = m_buildingHighlightIdToObject[buildingHighlightId];
                var buildingInformation = TryFetchBuildingInformationForHighlight(buildingHighlight.Id);
                if (buildingInformation != null)
                {
                    buildingHighlight.SetBuildingInformation(buildingInformation);
                }
            }
        }

        private BuildingInformation TryFetchBuildingInformationForHighlight(int buildingHighlightId)
        {
            BuildingInformation buildingInformation = null;
            BuildingInformationInterop buildingInfoInterop = new BuildingInformationInterop();
            // call C++ api point to populate buffer size fields
            var success = NativeBuildingsApi_TryGetBuildingInformation(NativePluginRunner.API, buildingHighlightId, ref buildingInfoInterop);

            if (success)
            {
                // alloc + pin buffers
                var buildingIdBuffer = new byte[buildingInfoInterop.BuildingIdSize];
                var buildingIdBufferGCHandle = GCHandle.Alloc(buildingIdBuffer, GCHandleType.Pinned);
                buildingInfoInterop.BuildingId = buildingIdBufferGCHandle.AddrOfPinnedObject();

                var contourPointsBuffer = new LatLongInterop[buildingInfoInterop.ContourPointsSize];
                var contourPointsBufferGCHandle = GCHandle.Alloc(contourPointsBuffer, GCHandleType.Pinned);
                buildingInfoInterop.ContourPoints = contourPointsBufferGCHandle.AddrOfPinnedObject();

                var buildingContoursBuffer = new BuildingContourInterop[buildingInfoInterop.BuildingContoursSize];
                var buildingContoursBufferGCHandle = GCHandle.Alloc(buildingContoursBuffer, GCHandleType.Pinned);
                buildingInfoInterop.BuildingContours = buildingContoursBufferGCHandle.AddrOfPinnedObject();

                // call C++ api point again to populate buffers
                success = NativeBuildingsApi_TryGetBuildingInformation(NativePluginRunner.API, buildingHighlightId, ref buildingInfoInterop);

                if (success)
                {
                    buildingInformation = CreateBuildingInformation(buildingInfoInterop, contourPointsBuffer, buildingContoursBuffer);
                }

                buildingIdBufferGCHandle.Free();
                contourPointsBufferGCHandle.Free();
                buildingContoursBufferGCHandle.Free();
            }

            return buildingInformation;
        }

        private static BuildingInformation CreateBuildingInformation(
            BuildingInformationInterop buildingInfoInterop,
            LatLongInterop[] contourPointsBuffer,
            BuildingContourInterop[] buildingContoursBuffer)
        {
            var buildingContours = new List<BuildingContour>();
            var contourPointsOffset = 0;

            for (int i = 0; i < buildingInfoInterop.BuildingContoursSize; ++i)
            {
                var buildingContourInterop = buildingContoursBuffer[i];
                var pointCount = buildingContourInterop.PointCount;
                var points = new List<LatLong>();
                for (int j = 0; j < pointCount; ++j)
                {
                    points.Add(contourPointsBuffer[contourPointsOffset + j].ToLatLong());
                }

                var buildingContour = new BuildingContour(
                    buildingContourInterop.BottomAltitude,
                    buildingContourInterop.TopAltitude,
                    points.AsReadOnly());

                buildingContours.Add(buildingContour);

                contourPointsOffset += pointCount;
            }

            var buildingInformation = new BuildingInformation(
                Marshal.PtrToStringAnsi(buildingInfoInterop.BuildingId, buildingInfoInterop.BuildingIdSize - 1),
                buildingInfoInterop.BuildingDimensions.ToBuildingDimensions(),
                buildingContours.AsReadOnly()
                );
            return buildingInformation;
        }

        public delegate void BuildingHighlightChangedDelegate(IntPtr buildingsApiHandle, int buildingHighlightId);

        [MonoPInvokeCallback(typeof(BuildingHighlightChangedDelegate))]
        public static void OnBuildingHighlightChanged(IntPtr buildingsApiHandle, int buildingHighlightId)
        {
            var buildingsApiInternal = buildingsApiHandle.NativeHandleToObject<BuildingsApiInternal>();
            buildingsApiInternal.NotifyBuildingHighlightChanged(buildingHighlightId);
        }

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern int NativeBuildingsApi_CreateHighlight(IntPtr ptr, ref BuildingHighlightCreateParamsInterop createParamsInterop);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern int NativeBuildingsApi_DestroyHighlight(IntPtr ptr, int buildingHighlightId);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativeBuildingsApi_TryFindIntersectionWithBuilding(IntPtr ptr, ref DoubleVector3 rayOrigin, ref DoubleVector3 rayDirection, out LatLongAltitudeInterop out_intersectionPoint);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativeBuildingsApi_TryFindIntersectionAndNormalWithBuilding(IntPtr ptr, ref DoubleVector3 rayOrigin, ref DoubleVector3 rayDirection, out LatLongAltitudeInterop out_intersectionPoint, out DoubleVector3 out_intersectionNormal);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern int NativeBuildingsApi_SetHighlightStyleAttributes(IntPtr ptr, int buildingHighlightId, ref ColorInterop color);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativeBuildingsApi_TryGetBuildingInformation(IntPtr ptr, int buildingHighlightId, ref BuildingInformationInterop out_buildingInformation);
    }
}
