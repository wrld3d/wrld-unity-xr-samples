using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using Wrld.Interop;
using Wrld.Utilities;

namespace Wrld.Resources.IndoorMaps
{
    internal class IndoorMapsApiInternal
    {
        struct EntitiesClickedInterop
        {
            EntitiesClickedInterop(int _stringCount, IntPtr _stringValues)
            {
                stringCount = _stringCount;
                stringValues = _stringValues;
            }

            internal int stringCount;
            internal IntPtr stringValues;
        };

        public delegate void IndoorMapEventHandler(IntPtr internalApiHandle);        
        public delegate void IndoorMapEntitiesClickHandler(IntPtr internalApiHandle, IntPtr entityIds);

        public event Action OnIndoorMapEnteredInternal;
        public event Action OnIndoorMapExitedInternal;
        public event Action OnIndoorMapFloorChangedInternal;
        public event Action<string> OnIndoorMapEntityClickedInternal;
        public event Action<List<string>> OnIndoorMapEntitiesClickedInternal;

        public IIndoorMapMaterialFactory IndoorMapMaterialFactory { get; set; }

        public IIndoorMapTextureStreamingService IndoorMapTextureStreamingService { get; private set; }

        public IIndoorMapTextureFetcher IndoorMapTextureFetcher { get; set; }

        [MonoPInvokeCallback(typeof(IndoorMapEventHandler))]
        public static void OnIndoorMapEnteredCallback(IntPtr internalApiHandle)
        {
            var indoorMapsApiInternal = internalApiHandle.NativeHandleToObject<IndoorMapsApiInternal>();
            indoorMapsApiInternal.OnIndoorMapEnteredInternal();
        }

        [MonoPInvokeCallback(typeof(IndoorMapEventHandler))]
        public static void OnIndoorMapExitedCallback(IntPtr internalApiHandle)
        {
            var indoorMapsApiInternal = internalApiHandle.NativeHandleToObject<IndoorMapsApiInternal>();
            indoorMapsApiInternal.OnIndoorMapExitedInternal();
        }

        [MonoPInvokeCallback(typeof(IndoorMapEventHandler))]
        public static void OnIndoorMapFloorChangedCallback(IntPtr internalApiHandle)
        {
            var indoorMapsApiInternal = internalApiHandle.NativeHandleToObject<IndoorMapsApiInternal>();
            indoorMapsApiInternal.OnIndoorMapFloorChangedInternal(); 
        }

        private static List<string> ExtractEntityIdsFromInteropPtr(IntPtr entityIdsInteropPtr)
        {
            // Doing manual marshalling here, as previous approach using SizeParamIndex appears not to work on Unity 5.5.0f3.
            var entitiesClickedInteropPtr = (EntitiesClickedInterop)Marshal.PtrToStructure(entityIdsInteropPtr, typeof(EntitiesClickedInterop));
            int stringCount = entitiesClickedInteropPtr.stringCount;

            var entityIds = new List<string>(stringCount);

            var intPtrSize = Marshal.SizeOf(typeof(IntPtr));

            for (int i = 0; i < stringCount; ++i)
            {
                IntPtr stringKeyPtr = Marshal.ReadIntPtr(entitiesClickedInteropPtr.stringValues, i * intPtrSize);

                string entityId = Marshal.PtrToStringAnsi(stringKeyPtr);
                entityIds.Add(entityId);
            }

            return entityIds;
        }

        [MonoPInvokeCallback(typeof(IndoorMapEntitiesClickHandler))]
        public static void OnIndoorMapEntitiesClickedCallback(IntPtr internalApiHandle, IntPtr entityIdsInteropPtr)
        {            
            var indoorMapsApiInternal = internalApiHandle.NativeHandleToObject<IndoorMapsApiInternal>();
            var entityIds = ExtractEntityIdsFromInteropPtr(entityIdsInteropPtr);
                        
            // invoke the old, deprecated callback N times.
            foreach (var entityId in entityIds)
            {
                if (entityId != null && entityId.Length > 0)
                {
                    indoorMapsApiInternal.OnIndoorMapEntityClickedInternal(entityId);
                }
            }

            // this is the new method, 1 call with N items
            indoorMapsApiInternal.OnIndoorMapEntitiesClickedInternal(entityIds);            
        }     

        public IndoorMapsApiInternal(IIndoorMapTextureStreamingService textureStreamingService, string indoorMapMaterialDirectory)
        {
            m_handleToSelf = NativeInteropHelpers.AllocateNativeHandleForObject(this);

            IndoorMapTextureStreamingService = textureStreamingService;
            IndoorMapTextureFetcher = new DefaultIndoorMapTextureFetcher(IndoorMapTextureStreamingService);
            IndoorMapMaterialFactory = new DefaultIndoorMapMaterialFactory(indoorMapMaterialDirectory);
        }

        public IntPtr GetHandle()
        {
            return m_handleToSelf;
        }

        public void EnterIndoorMap(string indoorMapId)
        {
            NativeIndoorMapsApi_EnterIndoorMap(NativePluginRunner.API, indoorMapId);
        }

        public void ExitIndoorMap()
        {
            NativeIndoorMapsApi_ExitIndoorMap(NativePluginRunner.API);
        }

        public void SetSelectedFloorId(int floorId)
        {
            var map = GetActiveIndoorMap();

            if (map != null)
            {
                int index = Array.IndexOf(map.FloorIds, floorId);
                SetSelectedFloorIndex(index);
            }
        }

        public int GetSelectedFloorId()
        {
            var map = GetActiveIndoorMap();

            if (map != null)
            {
                int index = GetSelectedFloorIndex();
                return map.FloorIds[index];
            }

            return 0;
        }

        private void SetSelectedFloorIndex(int floorIndex)
        {
            NativeIndoorMapsApi_SetSelectedFloorIndex(NativePluginRunner.API, floorIndex);
        }

        public void ExpandIndoor()
        {
            NativeIndoorMapsApi_ExpandIndoor(NativePluginRunner.API);
        }

        public void CollapseIndoor()
        {
            NativeIndoorMapsApi_CollapseIndoor(NativePluginRunner.API);
        }

        private int GetSelectedFloorIndex()
        {
            return NativeIndoorMapsApi_GetSelectedFloorIndex(NativePluginRunner.API);
        }

        public void MoveUpFloor(int numberOfFloors = 1)
        {
            MoveIndoorFloors(numberOfFloors);
        }

        public void MoveDownFloor(int numberOfFloors = 1)
        {
            MoveIndoorFloors(-numberOfFloors);
        }

        private void MoveIndoorFloors(int delta)
        {
            int currentFloor = GetSelectedFloorIndex();
            SetSelectedFloorIndex(currentFloor + delta);
        }

        public void SetIndoorFloorInterpolation(float dragParameter)
        {
            NativeIndoorMapsApi_SetFloorInterpolationParameter(NativePluginRunner.API, dragParameter);
        }

        public IndoorMap GetActiveIndoorMap()
        {
            string mapId = GetActiveIndoorMapId();

            if (string.IsNullOrEmpty(mapId))
            {
                return null;
            }

            string mapName = GetActiveIndoorMapName();
            string userData = GetActiveIndoorMapUserData();
            int floorCount = NativeIndoorMapsApi_GetActiveIndoorMapFloorCount(NativePluginRunner.API);

            string[] floorIds = new string[floorCount];
            string[] floorNames = new string[floorCount];
            int[] floorNumbers = new int[floorCount];

            for (int floorIndex = 0; floorIndex < floorCount; ++floorIndex)
            {
                string floorId = GetActiveIndoorMapFloorId(floorIndex);
                string floorName = GetActiveIndoorMapFloorName(floorIndex);
                int floorNumber = NativeIndoorMapsApi_GetActiveIndoorMapFloorNumber(NativePluginRunner.API, floorIndex);

                floorIds [floorIndex] = floorId;
                floorNames [floorIndex] = floorName;
                floorNumbers [floorIndex] = floorNumber;
            }

            return new IndoorMap(mapId, mapName, floorCount, floorIds, floorNames, floorNumbers, userData);
        }

        public void SetEntityHighlights(string[] highlightEntityIds, UnityEngine.Color color, string indoorMapId = null)
        {
            if (string.IsNullOrEmpty(indoorMapId))
            {
                indoorMapId = GetActiveIndoorMapId();
            }

            if (!string.IsNullOrEmpty(indoorMapId))
            {
                foreach (var highlightEntityId in highlightEntityIds)
                {
                    var colorInterop = color.ToColorInterop();
                    NativeIndoorMapsApi_SetIndoorHighlight(NativePluginRunner.API, indoorMapId, highlightEntityId, ref colorInterop);
                }
            }
        }
        public void ClearEntityHighlights(string[] highlightEntityIds = null, string indoorMapId = null)
        {
            if (string.IsNullOrEmpty(indoorMapId))
            {
                indoorMapId = GetActiveIndoorMapId();
            }

            if (!string.IsNullOrEmpty(indoorMapId))
            {
                if (highlightEntityIds == null)
                {
                    NativeIndoorMapsApi_ClearAllIndoorHighlights(NativePluginRunner.API, indoorMapId);
                }
                else
                {
                    foreach (var highlightEntityId in highlightEntityIds)
                    {
                        NativeIndoorMapsApi_ClearIndoorHighlight(NativePluginRunner.API, indoorMapId, highlightEntityId);
                    }
                }
            }
        }

        public Color GetIndoorMapRenderableColor(IndoorMapRenderable renderable)
        {
            return NativeIndoorMapsApi_GetIndoorMapRenderableColor(NativePluginRunner.API, renderable.NativeInstance).ToColor();
        }

        public Color GetIndoorMapHighlightRenderableColor(IndoorMapHighlightRenderable renderable)
        {
            return NativeIndoorMapsApi_GetIndoorMapHighlightRenderableColor(NativePluginRunner.API, renderable.NativeInstance).ToColor();
        }

        public int GetIndoorMapHighlightRenderableFloorIndex(IndoorMapHighlightRenderable renderable)
        {
            return NativeIndoorMapsApi_GetIndoorMapHighlightRenderableFloorIndex(NativePluginRunner.API, renderable.NativeInstance);
        }

        public float GetIndoorMapRenderableSaturation(IndoorMapRenderable renderable)
        {
            return NativeIndoorMapsApi_GetIndoorMapRenderableSaturation(NativePluginRunner.API, renderable.NativeInstance);
        }

        public int GetIndoorMapRenderableFloorIndex(IndoorMapRenderable renderable)
        {
            return NativeIndoorMapsApi_GetIndoorMapRenderableFloorIndex(NativePluginRunner.API, renderable.NativeInstance);
        }

        public Color GetInstancedIndoorMapRenderableColor(InstancedIndoorMapRenderable renderable)
        {
            return NativeIndoorMapsApi_GetInstancedIndoorMapRenderableColor(NativePluginRunner.API, renderable.NativeInstance).ToColor();
        }

        public float GetInstancedIndoorMapRenderableSaturation(InstancedIndoorMapRenderable renderable)
        {
            return NativeIndoorMapsApi_GetInstancedIndoorMapRenderableSaturation(NativePluginRunner.API, renderable.NativeInstance);
        }

        public int GetInstancedIndoorMapRenderableFloorIndex(InstancedIndoorMapRenderable renderable)
        {
            return NativeIndoorMapsApi_GetInstancedIndoorMapRenderableFloorIndex(NativePluginRunner.API, renderable.NativeInstance);
        }

        public bool TryGetInstancedIndoorMapHighlightColor(InstancedIndoorMapRenderable renderable, out Color color)
        {
            var interop = new ColorInterop();

            bool result = NativeIndoorMapsApi_TryGetInstancedIndoorMapRenderableHighlightColor(NativePluginRunner.API, renderable.NativeInstance, renderable.InstanceIndex, out interop);
            color = interop.ToColor();

            return result;
        }

        // We're not using [return MarshalAs(UnmanagedType.LPStr)] for the methods returning strings here, as 
        // .NET assumes that any string returned from native code via this mechanism is a temporary buffer that
        // it can free with CoTaskMemFree.  If strings allocated via other means are passed, this can lead to a 
        // crash, but returning an IntPtr to the buffer and decoding it via Marshal.PtrToString* methods is fine.
        //
        // https://stuffineededtoknow.blogspot.co.uk/2009/07/net-corrupts-heap-when-marshalling.html
        private string GetActiveIndoorMapName()
        {
            IntPtr stringPointer = NativeIndoorMapsApi_GetActiveIndoorMapName(NativePluginRunner.API);
            return Marshal.PtrToStringAnsi(stringPointer);
        }
        private string GetActiveIndoorMapId()
        {
            IntPtr stringPointer = NativeIndoorMapsApi_GetActiveIndoorMapId(NativePluginRunner.API);
            return Marshal.PtrToStringAnsi(stringPointer);
        }
        private string GetActiveIndoorMapFloorId(int floorIndex)
        {
            IntPtr stringPointer = NativeIndoorMapsApi_GetActiveIndoorMapFloorId(NativePluginRunner.API, floorIndex);
            return Marshal.PtrToStringAnsi(stringPointer);
        }
        private string GetActiveIndoorMapFloorName(int floorIndex)
        {
            IntPtr stringPointer = NativeIndoorMapsApi_GetActiveIndoorMapFloorName(NativePluginRunner.API, floorIndex);
            return Marshal.PtrToStringAnsi(stringPointer);
        }
        private string GetActiveIndoorMapUserData()
        {
            IntPtr stringPointer = NativeIndoorMapsApi_GetActiveIndoorMapUserData(NativePluginRunner.API);
            return Marshal.PtrToStringAnsi(stringPointer);
        }

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeIndoorMapsApi_EnterIndoorMap(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr)]string indoorMapId);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeIndoorMapsApi_ExitIndoorMap(IntPtr ptr);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeIndoorMapsApi_SetSelectedFloorIndex(IntPtr ptr, int floorIndex);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern int NativeIndoorMapsApi_GetSelectedFloorIndex(IntPtr ptr);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeIndoorMapsApi_ExpandIndoor(IntPtr ptr);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeIndoorMapsApi_CollapseIndoor(IntPtr ptr);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeIndoorMapsApi_SetFloorInterpolationParameter(IntPtr ptr, float floorParam);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr NativeIndoorMapsApi_GetActiveIndoorMapName(IntPtr ptr);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr NativeIndoorMapsApi_GetActiveIndoorMapId(IntPtr ptr);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern int NativeIndoorMapsApi_GetActiveIndoorMapFloorCount(IntPtr ptr);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr NativeIndoorMapsApi_GetActiveIndoorMapFloorId(IntPtr ptr, int floorIndex);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr NativeIndoorMapsApi_GetActiveIndoorMapFloorName(IntPtr ptr, int floorIndex);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern int NativeIndoorMapsApi_GetActiveIndoorMapFloorNumber(IntPtr ptr, int floorIndex);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr NativeIndoorMapsApi_GetActiveIndoorMapUserData(IntPtr ptr);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeIndoorMapsApi_SetIndoorHighlight(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr)] string indoorMapId, [MarshalAs(UnmanagedType.LPStr)] string highlightEntityId, ref ColorInterop color);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeIndoorMapsApi_ClearIndoorHighlight(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr)] string indoorMapId, [MarshalAs(UnmanagedType.LPStr)] string highlightEntityId);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern void NativeIndoorMapsApi_ClearAllIndoorHighlights(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr)] string indoorMapId);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern ColorInterop NativeIndoorMapsApi_GetIndoorMapRenderableColor(IntPtr ptr, IntPtr renderable);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern ColorInterop NativeIndoorMapsApi_GetIndoorMapHighlightRenderableColor(IntPtr ptr, IntPtr renderable);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern int NativeIndoorMapsApi_GetIndoorMapHighlightRenderableFloorIndex(IntPtr ptr, IntPtr renderable);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern float NativeIndoorMapsApi_GetIndoorMapRenderableSaturation(IntPtr ptr, IntPtr renderable);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern ColorInterop NativeIndoorMapsApi_GetInstancedIndoorMapRenderableColor(IntPtr ptr, IntPtr renderable);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern float NativeIndoorMapsApi_GetInstancedIndoorMapRenderableSaturation(IntPtr ptr, IntPtr renderable);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern int NativeIndoorMapsApi_GetIndoorMapRenderableFloorIndex(IntPtr ptr, IntPtr renderable);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern int NativeIndoorMapsApi_GetInstancedIndoorMapRenderableFloorIndex(IntPtr ptr, IntPtr renderable);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativeIndoorMapsApi_TryGetInstancedIndoorMapRenderableHighlightColor(IntPtr ptr, IntPtr renderable, int instanceIndex, out ColorInterop color);

        private IntPtr m_handleToSelf;
    }
}
