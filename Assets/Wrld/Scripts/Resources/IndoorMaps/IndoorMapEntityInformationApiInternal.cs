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
    internal class IndoorMapEntityInformationApiInternal
    {
        public event Action<IndoorMapEntityInformation> OnIndoorMapEntityInformationUpdated;

        private IDictionary<int, IndoorMapEntityInformation> m_indoorMapEntityInformationIdToObject = new Dictionary<int, IndoorMapEntityInformation>();
        private IntPtr m_handleToSelf;

        internal IndoorMapEntityInformationApiInternal()
        {
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

        public IndoorMapEntityInformation AddIndoorMapEntityInformation(
            string indoorMapId,
            Action<IndoorMapEntityInformation> indoorMapEntityInformationChangedDelegate
            )
        {
            var indoorMapEntityInformationId = NativeIndoorMapEntityInformationApi_CreateIndoorMapEntityInformation(NativePluginRunner.API, indoorMapId);
            var indoorMapEntityInformation = new IndoorMapEntityInformation(
                this,
                indoorMapEntityInformationId,
                indoorMapId,
                indoorMapEntityInformationChangedDelegate
                );

            m_indoorMapEntityInformationIdToObject.Add(indoorMapEntityInformationId, indoorMapEntityInformation);

            TryUpdateIndoorMapEntityInformation(indoorMapEntityInformation);

            return indoorMapEntityInformation;
        }


        private bool TryUpdateIndoorMapEntityInformation(IndoorMapEntityInformation indoorMapEntityInformation)
        {
            // fetch from native
            var indoorMapEntities = new List<IndoorMapEntity>();

            var interop = new IndoorMapEntityInformationInterop
            {
                indoorMapEntityInformationModelId = indoorMapEntityInformation.Id
            };

            var success = NativeIndoorMapEntityInformationApi_TryGetIndoorMapEntityInformationBufferSizes(NativePluginRunner.API, ref interop);
            if (!success)
            {
                return false;
            }

            var indoorMapEntityIdsBuffer = new int[interop.indoorMapEntityModelIdsBufferSize];
            var indoorMapEntityIdsBufferGCHandle = GCHandle.Alloc(indoorMapEntityIdsBuffer, GCHandleType.Pinned);
            interop.indoorMapEntityModelIds = indoorMapEntityIdsBufferGCHandle.AddrOfPinnedObject();

            success = NativeIndoorMapEntityInformationApi_TryGetIndoorMapEntityInformation(NativePluginRunner.API, ref interop);
            if (success)
            {

                foreach (var indoorMapEntityId in indoorMapEntityIdsBuffer)
                {
                    var indoorMapEntityInterop = new IndoorMapEntityInterop
                    {
                        indoorMapEntityModelId = indoorMapEntityId
                    };
                    success = NativeIndoorMapEntityInformationApi_TryGetIndoorMapEntityBufferSizes(NativePluginRunner.API, ref indoorMapEntityInterop);
                    if (!success)
                    {
                        break;
                    }


                    var indoorMapEntityIdBuffer = new byte[indoorMapEntityInterop.indoorMapEntityIdBufferSize];
                    var indoorMapEntityIdBufferGCHandle = GCHandle.Alloc(indoorMapEntityIdBuffer, GCHandleType.Pinned);
                    indoorMapEntityInterop.indoorMapEntityId = indoorMapEntityIdBufferGCHandle.AddrOfPinnedObject();

                    success = NativeIndoorMapEntityInformationApi_TryGetIndoorMapEntity(NativePluginRunner.API, ref indoorMapEntityInterop);
                    if (!success)
                    {
                        indoorMapEntityIdBufferGCHandle.Free();
                        break;
                    }

                    var indoorMapEntity = FromInterop(indoorMapEntityInterop);
                    indoorMapEntities.Add(indoorMapEntity);
                    indoorMapEntityIdBufferGCHandle.Free();
                }
            }

            if (success)
            {
                indoorMapEntityInformation.SetEntityInformation(indoorMapEntities, interop.indoorMapEntityLoadState);

                if (OnIndoorMapEntityInformationUpdated != null)
                {
                    OnIndoorMapEntityInformationUpdated(indoorMapEntityInformation);
                }
            }
            
            indoorMapEntityIdsBufferGCHandle.Free();

            return success;
        }

        public static IndoorMapEntity FromInterop(IndoorMapEntityInterop interop)
        {
            var indoorMapEntity = new IndoorMapEntity(
                Marshal.PtrToStringAnsi(interop.indoorMapEntityId, interop.indoorMapEntityIdBufferSize - 1),
                interop.indoorMapFloorId,
                interop.position.ToLatLong()
                );

            return indoorMapEntity;
        }

        public void RemoveIndoorMapEntityInformation(IndoorMapEntityInformation indoorMapEntityInformation)
        {
            if (!m_indoorMapEntityInformationIdToObject.ContainsKey(indoorMapEntityInformation.Id))
            {
                return;
            }

            m_indoorMapEntityInformationIdToObject.Remove(indoorMapEntityInformation.Id);
            NativeIndoorMapEntityInformationApi_DestroyIndoorMapEntityInformation(NativePluginRunner.API, indoorMapEntityInformation.Id);
        }

        public void NotifyIndoorMapEntityInformationChanged(int indoorMapEntityInformationId)
        {
            if (m_indoorMapEntityInformationIdToObject.ContainsKey(indoorMapEntityInformationId))
            {
                var indoorMapEntityInformation = m_indoorMapEntityInformationIdToObject[indoorMapEntityInformationId];
                TryUpdateIndoorMapEntityInformation(indoorMapEntityInformation);
            }
        }


        public delegate void NativeIndoorMapEntityInformationChangedDelegate(IntPtr indoorMapEntityInformationApiInternalHandle, int indoorMapEntityInformationId);

        [MonoPInvokeCallback(typeof(NativeIndoorMapEntityInformationChangedDelegate))]
        public static void OnNativeIndoorMapEntityInformationChanged(IntPtr indoorMapEntityInformationApiInternalHandle, int indoorMapEntityInformationId)
        {
            var indoorMapEntityInformationApiInternal = indoorMapEntityInformationApiInternalHandle.NativeHandleToObject<IndoorMapEntityInformationApiInternal>();
            indoorMapEntityInformationApiInternal.NotifyIndoorMapEntityInformationChanged(indoorMapEntityInformationId);
        }

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern int NativeIndoorMapEntityInformationApi_CreateIndoorMapEntityInformation(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr)] string indoorMapId);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern int NativeIndoorMapEntityInformationApi_DestroyIndoorMapEntityInformation(IntPtr ptr, int indoorMapEntityInformationId);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativeIndoorMapEntityInformationApi_TryGetIndoorMapEntityInformationBufferSizes(IntPtr ptr, ref IndoorMapEntityInformationInterop inout_indoorMapEntityInformationInterop);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativeIndoorMapEntityInformationApi_TryGetIndoorMapEntityInformation(IntPtr ptr, ref IndoorMapEntityInformationInterop inout_indoorMapEntityInformationInterop);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativeIndoorMapEntityInformationApi_TryGetIndoorMapEntityBufferSizes(IntPtr ptr, ref IndoorMapEntityInterop inout_indoorMapEntityInterop);

        [DllImport(NativePluginRunner.DLL, CallingConvention = CallingConvention.StdCall)]
        private static extern bool NativeIndoorMapEntityInformationApi_TryGetIndoorMapEntity(IntPtr ptr, ref IndoorMapEntityInterop inout_indoorMapEntityInterop);
    }
}
