using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Wrld.Common.Camera;
using Wrld.Common.Maths;
using Wrld.Utilities;
using Wrld.Interop;

namespace Wrld.Space.Positioners
{
    internal class PositionerApiInternal
    {
        public delegate void PositionerChangedHandler(Positioner positioner);

        public event PositionerChangedHandler OnPositionerTransformedPointChanged;
        public event PositionerChangedHandler OnPositionerScreenPointChanged;

        private IDictionary<int, Positioner> m_positionerIdToObject = new Dictionary<int, Positioner>();
        private IntPtr m_handleToSelf;

        internal PositionerApiInternal()
        {
            m_handleToSelf = NativeInteropHelpers.AllocateNativeHandleForObject(this);
        }

        internal IntPtr GetHandle()
        {
            return m_handleToSelf;
        }

        internal void Destroy()
        {
            NativeInteropHelpers.FreeNativeHandle(m_handleToSelf);
        }

        public Positioner CreatePositioner(PositionerOptions positionerOptions)
        {
            var createParamsInterop = new PositionerCreateParamsInterop
            {
                ElevationMode = positionerOptions.GetElevationMode(),
                LatitudeDegrees = positionerOptions.GetLatitudeDegrees(),
                LongitudeDegrees = positionerOptions.GetLongitudeDegrees(),
                Elevation = positionerOptions.GetElevation(),
                IndoorMapId = positionerOptions.GetIndoorMapId(),
                IndoorMapFloorId = positionerOptions.GetIndoorMapFloorId(),
                UsingFloorId = positionerOptions.IsUsingFloorId()
            };

            var positionerId = NativePositionerApi_CreatePositioner(NativePluginRunner.API, ref createParamsInterop);

            var positioner = new Positioner(
                this,
                positionerId,
                positionerOptions.GetElevationMode());

            m_positionerIdToObject.Add(positionerId, positioner);

            return positioner;
        }

        public bool TryFetchECEFLocationForPositioner(Positioner positioner, out DoubleVector3 positionerECEFLocation)
        {
            DoubleVector3Interop positionerECEFLocationInterop = new DoubleVector3Interop();

            var success = NativePositionerApi_TryFetchECEFLocationForPositioner(NativePluginRunner.API, positioner.Id, ref positionerECEFLocationInterop);

            if (success)
            {
                positionerECEFLocation.x = positionerECEFLocationInterop.x;
                positionerECEFLocation.y = positionerECEFLocationInterop.y;
                positionerECEFLocation.z = positionerECEFLocationInterop.z;

                return true;
            }

            positionerECEFLocation = DoubleVector3.zero;

            return false;
        }

        public bool TryFetchScreenPointForPositioner(Positioner positioner, out Vector3 screenPoint)
        {
            screenPoint = Vector3.zero;

            if(NativePositionerApi_TryFetchScreenPointForPositioner(NativePluginRunner.API, positioner.Id, out screenPoint))
            {
                screenPoint = CameraHelpers.ScreenPointOriginTopLeft(screenPoint);
                return true;
            }

            return false;
        }

        public bool TryFetchLatLongAltitudeForPositioner(Positioner positioner, out LatLongAltitude latLongAlt)
        {
            LatLongAltitudeInterop latLongAltInterop = new LatLongAltitudeInterop();
            latLongAlt = new LatLongAltitude();

            var success = NativePositionerApi_TryFetchLatLongAltitude(NativePluginRunner.API, positioner.Id, out latLongAltInterop);

            if (success)
            {
                latLongAlt = latLongAltInterop.ToLatLongAltitude();
                return true;
            }
            
            return false;
        }

        private void NotifyPositionerTransformedPointChanged(int positionerId)
        {
            Positioner positioner;
            if (!m_positionerIdToObject.TryGetValue(positionerId, out positioner))
            {
                return;
            }

            if (OnPositionerTransformedPointChanged != null)
            {
                OnPositionerTransformedPointChanged(positioner);
            }

            if (positioner.OnTransformedPointChanged != null)
            {
                positioner.OnTransformedPointChanged();
            }
        }

        private void NotifyPositionersProjectionsChanged()
        {
            foreach (int positionerId in m_positionerIdToObject.Keys)
            {
                var positioner = m_positionerIdToObject[positionerId];
                if (OnPositionerScreenPointChanged != null)
                {
                    OnPositionerScreenPointChanged(positioner);
                }

                if (positioner.OnScreenPointChanged != null)
                {
                    positioner.OnScreenPointChanged();
                }

#pragma warning disable 618
                // call deprecated event
                if (positioner.OnPositionerPositionChangedDelegate != null)
                {
                    positioner.OnPositionerPositionChangedDelegate();
                }
#pragma warning restore 618
            }
        }

        public void DestroyPositioner(Positioner positioner)
        {
            if(!m_positionerIdToObject.ContainsKey(positioner.Id))
            {
                return;
            }

            m_positionerIdToObject.Remove(positioner.Id);
            NativePositionerApi_DestroyPositioner(NativePluginRunner.API, positioner.Id);
        }

        public void SetPositionerLocation(Positioner positioner, double latitudeDegrees, double longitudeDegrees)
        {
            if (!m_positionerIdToObject.ContainsKey(positioner.Id))
            {
                return;
            }
            NativePositionerApi_SetLocation(NativePluginRunner.API, positioner.Id, latitudeDegrees, longitudeDegrees);
        }

        public void SetPositionerElevation(Positioner positioner, double elevation)
        {
            if (!m_positionerIdToObject.ContainsKey(positioner.Id))
            {
                return;
            }

            NativePositionerApi_SetElevation(NativePluginRunner.API, positioner.Id, elevation);
        }

        public void SetPositionerElevationMode(Positioner positioner, ElevationMode elevationMode)
        {
            if (!m_positionerIdToObject.ContainsKey(positioner.Id))
            {
                return;
            }

            NativePositionerApi_SetElevationMode(NativePluginRunner.API, positioner.Id, elevationMode);
        }

        public void SetPositionerIndoorMap(Positioner positioner, string indoorMapId, int indoorMapFloorId)
        {
            if (!m_positionerIdToObject.ContainsKey(positioner.Id))
            {
                return;
            }

            NativePositionerApi_SetIndoorMap(NativePluginRunner.API, positioner.Id, indoorMapId, indoorMapFloorId);
        }

        public bool IsTransformedPointDefined(Positioner positioner)
        {
            return NativePositionerApi_IsTransformedPointDefined(NativePluginRunner.API, positioner.Id);
        }

        public bool IsPositionerBehindGlobeHorizon(Positioner positioner)
        {
            return NativePositionerApi_IsBehindGlobeHorizon(NativePluginRunner.API, positioner.Id);
        }

        public delegate void PositionerTransformedPointChangedDelegate(IntPtr positionerApiHandle, int positionerId);

        [MonoPInvokeCallback(typeof(PositionerTransformedPointChangedDelegate))]
        public static void OnNativePositionerTransformedPointChanged(IntPtr positionerApiHandle, int positionerId)
        {
            var positionerApiInternal = positionerApiHandle.NativeHandleToObject<PositionerApiInternal>();

            positionerApiInternal.NotifyPositionerTransformedPointChanged(positionerId);
        }


        public delegate void PositionersProjectionChangedDelegate(IntPtr positionerApiHandle);

        [MonoPInvokeCallback(typeof(PositionersProjectionChangedDelegate))]
        public static void OnNativePositionersProjectionChanged(IntPtr positionerApiHandle)
        {
            var positionerApiInternal = positionerApiHandle.NativeHandleToObject<PositionerApiInternal>();

            positionerApiInternal.NotifyPositionersProjectionsChanged();
        }

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern int NativePositionerApi_CreatePositioner(IntPtr ptr, ref PositionerCreateParamsInterop createParamsInterop);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativePositionerApi_TryFetchECEFLocationForPositioner(IntPtr ptr, int positionerId, ref DoubleVector3Interop out_positionerECEFLocationInterop);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativePositionerApi_TryFetchScreenPointForPositioner(IntPtr ptr, int positionerId, out Vector3 out_screenPoint);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativePositionerApi_TryFetchLatLongAltitude(IntPtr ptr, int positionerId, out Interop.LatLongAltitudeInterop out_latLongAltInterop);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativePositionerApi_DestroyPositioner(IntPtr ptr, int positionerId);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativePositionerApi_SetLocation(IntPtr ptr, int positionerId, double latitudeDegrees, double longitudeDegrees);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativePositionerApi_SetElevation(IntPtr ptr, int positionerId, double elevation);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativePositionerApi_SetElevationMode(IntPtr ptr, int positionerId, ElevationMode elevationMode);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativePositionerApi_SetIndoorMap(IntPtr ptr, int positionerId, [MarshalAs(UnmanagedType.LPStr)] string indoorMapId, int indoorMapFloorId);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativePositionerApi_IsTransformedPointDefined(IntPtr ptr, int positionerId);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativePositionerApi_IsBehindGlobeHorizon(IntPtr ptr, int positionerId);

    }
}
