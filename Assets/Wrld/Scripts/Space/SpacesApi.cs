using Assets.Wrld.Scripts.Maths;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Wrld.Utilities;
using Wrld.Interop;
using Wrld.Common.Maths;

namespace Wrld.Space
{
    /// <summary>
    /// Contains functionality for working with object transforms and positions in various coordinate systems.
    /// </summary>
    public class SpacesApi
    {
        private ApiImplementation m_apiImplementation;

        internal SpacesApi(ApiImplementation apiImplementation)
        {
            m_apiImplementation = apiImplementation;
        }

        /// <summary>
        /// Obtain a ray in ECEF coordinates from the current camera location and passing through the specified screen point.
        /// </summary>
        /// <param name="screenPoint">The screen point, in pixels, with screen origin bottom-left.</param>
        /// <returns>An ECEF ray.</returns>
        public DoubleRay ScreenPointToRay(Vector2 screenPoint)
        {
            var screenPointOriginTopLeft = new Vector2(screenPoint.x, Screen.height - screenPoint.y);
            DoubleRay ray = NativeSpacesApi_ScreenPointToRay(NativePluginRunner.API, ref screenPointOriginTopLeft);
            return ray;
        }

        /// <summary>
        /// Obtain a ray in ECEF coordinates in a vertically downwards direction, and starting at a point high above the Earth's surface at the specified LatLong point.
        /// </summary>
        /// <param name="latLong">A LatLong point through which the vertical ray passes.</param>
        /// <returns>An ECEF ray.</returns>
        public DoubleRay LatLongToVerticallyDownRay(LatLong latLong)
        {
            var latLongInterop = latLong.ToLatLongInterop();
            DoubleRay ray = NativeSpacesApi_LatLongToVerticallyDownRay(NativePluginRunner.API, ref latLongInterop);
            return ray;
        }

        /// <summary>
        /// Transforms a point from a LatLongAltitude to a Unity transform position.
        /// Note: If using the ECEF coordinate system, the returned position will only be valid until the camera is moved. To robustly position an object on the map, use the GeographicTransform component.
        /// </summary>
        /// <param name="position">The geographical coordinates of the position to transform to local space.</param>
        /// <returns>The transformed geographic LatLongAltitude in local space.</returns>
        public Vector3 GeographicToWorldPoint(LatLongAltitude position)
        {
            return m_apiImplementation.GeographicToWorldPoint(position);
        }

        /// <summary>
        /// Transforms a point from a Unity transform position to a LatLongAltitude.
        /// Note: If using the ECEF coordinate system, the returned position will only be valid until the camera is moved. To robustly position an object on the map, use the GeographicTransform component.
        /// </summary>
        /// <param name="position">The world position to transform into a geographic coordinate.</param>
        /// <returns>The transformed world position as a LatLongAltitude.</returns>
        public LatLongAltitude WorldToGeographicPoint(Vector3 position)
        {
            return m_apiImplementation.WorldToGeographicPoint(position);
        }

        /// <summary>
        /// Calculates the absolute heading angle of a direction at a geographic coordinate.
        /// </summary>
        /// <param name="directionEcef">A unit direction vector in ECEF coordinates.</param>
        /// <param name="pointEcef">A point in ECEF coordinates.</param>
        /// <returns>The absolute heading angle in degrees clockwise from North of directionEcef at pointEcef.</returns>
        public double HeadingDegreesFromDirectionAtPoint(DoubleVector3 directionEcef, DoubleVector3 pointEcef)
        {
            return NativeSpacesApi_HeadingDegreesFromDirectionAtPoint(NativePluginRunner.API, ref directionEcef, ref pointEcef);
        }

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern DoubleRay NativeSpacesApi_ScreenPointToRay(IntPtr ptr, ref Vector2 screenPoint);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern DoubleRay NativeSpacesApi_LatLongToVerticallyDownRay(IntPtr ptr, ref LatLongInterop latLongInterop);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern double NativeSpacesApi_HeadingDegreesFromDirectionAtPoint(IntPtr ptr, ref DoubleVector3 directionEcef, ref DoubleVector3 pointEcef);

    }
}
