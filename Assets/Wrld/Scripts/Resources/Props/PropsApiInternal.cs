using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Wrld.Space;

namespace Wrld.Resources.Props
{
    internal class PropsApiInternal
    {
        private IDictionary<int, Prop> m_propIdToObject = new Dictionary<int, Prop>();

        public Prop CreateProp(PropOptions positionerOptions)
        {
            var createParamsInterop = new PropCreateParamsInterop
            {
                Name = positionerOptions.GetName(),
                GeometryId = positionerOptions.GetGeometryId(),
                ElevationMode = positionerOptions.GetElevationMode(),
                HeadingDegrees = positionerOptions.GetHeadingDegrees(),
                LatitudeDegrees = positionerOptions.GetLatitudeDegrees(),
                LongitudeDegrees = positionerOptions.GetLongitudeDegrees(),
                Elevation = positionerOptions.GetElevation(),
                IndoorMapId = positionerOptions.GetIndoorMapId(),
                IndoorMapFloorId = positionerOptions.GetIndoorMapFloorId()
            };

            var propId = NativePropsApi_CreateProp(NativePluginRunner.API, ref createParamsInterop);

            var positioner = new Prop(
                this,
                propId,
                positionerOptions);

            m_propIdToObject.Add(propId, positioner);

            return positioner;
        }

        internal void SetLocation(Prop prop, double latitudeDegrees, double longitudeDegrees)
        {
            if (!m_propIdToObject.ContainsKey(prop.Id))
            {
                return;
            }

            NativePropsApi_SetLocation(NativePluginRunner.API, prop.Id, latitudeDegrees, longitudeDegrees);
        }

        internal void SetElevation(Prop prop, double elevation)
        {
            if (!m_propIdToObject.ContainsKey(prop.Id))
            {
                return;
            }

            NativePropsApi_SetElevation(NativePluginRunner.API, prop.Id, elevation);
        }

        internal void SetElevationMode(Prop prop, ElevationMode elevationMode)
        {
            if (!m_propIdToObject.ContainsKey(prop.Id))
            {
                return;
            }

            NativePropsApi_SetElevationMode(NativePluginRunner.API, prop.Id, (int)elevationMode);
        }

        internal void SetGeometryId(Prop prop, string geometryId)
        {
            if (!m_propIdToObject.ContainsKey(prop.Id))
            {
                return;
            }

            NativePropsApi_SetGeometryId(NativePluginRunner.API, prop.Id, geometryId);
        }

        internal void SetHeadingDegrees(Prop prop, double headingDegrees)
        {
            if (!m_propIdToObject.ContainsKey(prop.Id))
            {
                return;
            }

            NativePropsApi_SetHeadingDegrees(NativePluginRunner.API, prop.Id, headingDegrees);
        }

        internal void SetAutomaticIndoorMapPopulationEnabled(bool enabled)
        {
            NativePropsApi_SetAutomaticIndoorMapPopulationEnabled(NativePluginRunner.API, enabled);
        }

        internal bool IsAutomaticIndoorMapPopulationEnabled()
        {
            return NativePropsApi_IsAutomaticIndoorMapPopulationEnabled(NativePluginRunner.API);
        }

        public void DestroyProp(Prop prop)
        {
            if (!m_propIdToObject.ContainsKey(prop.Id))
            {
                return;
            }

            m_propIdToObject.Remove(prop.Id);
            NativePropsApi_DestroyProp(NativePluginRunner.API, prop.Id);
        }

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern int NativePropsApi_CreateProp(IntPtr ptr, ref PropCreateParamsInterop createParams);
        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void NativePropsApi_DestroyProp(IntPtr ptr, int propId);
        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void NativePropsApi_SetLocation(IntPtr ptr, int propId, double latitudeDegrees, double longitudeDegrees);
        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void NativePropsApi_SetElevation(IntPtr ptr, int propId, double heightOffset);
        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void NativePropsApi_SetElevationMode(IntPtr ptr, int propId, int elevationModeInt);
        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void NativePropsApi_SetGeometryId(IntPtr ptr, int propId, [MarshalAs(UnmanagedType.LPStr)] string geometryId);
        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void NativePropsApi_SetHeadingDegrees(IntPtr ptr, int propId, double headingDegrees);
        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void NativePropsApi_SetAutomaticIndoorMapPopulationEnabled(IntPtr ptr, [MarshalAs(UnmanagedType.I1)] bool enabled);
        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativePropsApi_IsAutomaticIndoorMapPopulationEnabled(IntPtr ptr);
    }
}