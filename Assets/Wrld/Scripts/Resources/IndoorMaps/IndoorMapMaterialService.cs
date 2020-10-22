using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Wrld.Utilities;

namespace Wrld.Resources.IndoorMaps
{
    internal class IndoorMapMaterialService
    {
        struct MaterialDescriptorInterop
        {
            MaterialDescriptorInterop(
                IntPtr _indoorMapName,
                IntPtr _materialName,
                int _stringCount,
                IntPtr _stringKeys,
                IntPtr _stringValues,
                int _colorCount,
                IntPtr _colorNames,
                IntPtr _colorValues,
                int _scalarCount,
                IntPtr _scalarNames,
                IntPtr _scalarValues,
                int _booleanCount,
                IntPtr _booleanNames,
                IntPtr _booleanValues,
                IntPtr _materialServiceHandle
            )
            {
                indoorMapName = _indoorMapName;
                materialName = _materialName;
                stringCount = _stringCount;
                stringKeys = _stringKeys;
                stringValues = _stringValues;
                colorCount = _colorCount;
                colorNames = _colorNames;
                colorValues = _colorValues;
                scalarCount = _scalarCount;
                scalarNames = _scalarNames;
                scalarValues = _scalarValues;
                booleanCount = _booleanCount;
                booleanNames = _booleanNames;
                booleanValues = _booleanValues;
                materialServiceHandle = _materialServiceHandle;
            }

            internal IntPtr indoorMapName;
            internal IntPtr materialName;
            internal int stringCount;
            internal IntPtr stringKeys;
            internal IntPtr stringValues;
            internal int colorCount;
            internal IntPtr colorNames;
            internal IntPtr colorValues;
            internal int scalarCount;
            internal IntPtr scalarNames;
            internal IntPtr scalarValues;
            internal int booleanCount;
            internal IntPtr booleanNames;
            internal IntPtr booleanValues;
            internal IntPtr materialServiceHandle;
        };

        internal delegate IntPtr CreateMaterialCallback(
            IntPtr materialDescriptorInterop);

        internal delegate void DeleteMaterialCallback(IntPtr materialHandle, IntPtr materialServiceHandle);

        [MonoPInvokeCallback(typeof(CreateMaterialCallback))]
        internal static IntPtr CreateMaterial(
            IntPtr materialDescriptorInteropPtr)
        {
            // Doing manual marshalling here, as previous approach using SizeParamIndex appears not to work on Unity 5.5.0f3.
            var materialDescriptorInterop = (MaterialDescriptorInterop)Marshal.PtrToStructure(materialDescriptorInteropPtr, typeof(MaterialDescriptorInterop));
            var indoorMapName = Marshal.PtrToStringAnsi(materialDescriptorInterop.indoorMapName);
            var materialName = Marshal.PtrToStringAnsi(materialDescriptorInterop.materialName);
            var stringDict = new Dictionary<string, string>(materialDescriptorInterop.stringCount);
            var intPtrSize = Marshal.SizeOf(typeof(IntPtr));

            for (int i = 0; i < materialDescriptorInterop.stringCount; ++i)
            {
                IntPtr stringKeyPtr = Marshal.ReadIntPtr(materialDescriptorInterop.stringKeys, i * intPtrSize);
                IntPtr stringValuePtr = Marshal.ReadIntPtr(materialDescriptorInterop.stringValues, i * intPtrSize);
                string stringKey = Marshal.PtrToStringAnsi(stringKeyPtr);
                string stringValue  = Marshal.PtrToStringAnsi(stringValuePtr);
                stringDict[stringKey] = stringValue;
            }

            var colorDict = new Dictionary<string, Color>(materialDescriptorInterop.colorCount);

            if (materialDescriptorInterop.colorCount > 0)
            {
                var colorFloats = new float[materialDescriptorInterop.colorCount * 4];
                Marshal.Copy(materialDescriptorInterop.colorValues, colorFloats, 0, colorFloats.Length);

                for (int i = 0; i < materialDescriptorInterop.colorCount; ++i)
                {
                    IntPtr colorNamePtr = Marshal.ReadIntPtr(materialDescriptorInterop.colorNames, i * intPtrSize);
                    string colorName = Marshal.PtrToStringAnsi(colorNamePtr);
                    var colorValue = new Color(colorFloats[i * 4 + 0], colorFloats[i * 4 + 1], colorFloats[i * 4 + 2], colorFloats[i * 4 + 3]);
                    colorDict[colorName] = colorValue;
                }
            }

            var scalarDict = new Dictionary<string, float>(materialDescriptorInterop.scalarCount);

            if (materialDescriptorInterop.scalarCount > 0)
            {
                var scalars = new float[materialDescriptorInterop.scalarCount];
                Marshal.Copy(materialDescriptorInterop.scalarValues, scalars, 0, scalars.Length);

                for (int i = 0; i < materialDescriptorInterop.scalarCount; ++i)
                {
                    IntPtr scalarNamePtr = Marshal.ReadIntPtr(materialDescriptorInterop.scalarNames, i * intPtrSize);
                    string scalarName = Marshal.PtrToStringAnsi(scalarNamePtr);
                    var scalarValue = scalars[i];
                    scalarDict[scalarName] = scalarValue;
                }
            }

            var booleanDict = new Dictionary<string, bool>(materialDescriptorInterop.booleanCount);

            for (int i = 0; i < materialDescriptorInterop.booleanCount; ++i)
            {
                IntPtr booleanNamePtr = Marshal.ReadIntPtr(materialDescriptorInterop.booleanNames, i * intPtrSize);
                string booleanName = Marshal.PtrToStringAnsi(booleanNamePtr);
                var booleanValue = Marshal.ReadInt32(materialDescriptorInterop.booleanValues, i * sizeof(Int32)) != 0;

                booleanDict[booleanName] = booleanValue;
            }

            var indoorMaterialDescriptor = new IndoorMaterialDescriptor(
                indoorMapName,
                materialName,
                stringDict,
                colorDict,
                scalarDict,
                booleanDict
            );

            var materialServiceInstance = materialDescriptorInterop.materialServiceHandle.NativeHandleToObject<IndoorMapMaterialService>();

            return materialServiceInstance.CreateMaterialFromDescriptor(indoorMaterialDescriptor);
        }

        private IntPtr CreateMaterialFromDescriptor(IndoorMaterialDescriptor descriptor)
        {
            var factory = m_indoorMapsApiInternal.IndoorMapMaterialFactory;            
            var material = factory.CreateMaterialFromDescriptor(descriptor);

            m_materialRepository.AddTemplateMaterial(material);

            var textureFetcher = m_indoorMapsApiInternal.IndoorMapTextureFetcher;
            textureFetcher.IssueTextureRequestsForMaterial(material, descriptor);

            return NativeInteropHelpers.AllocateNativeHandleForObject(material);
        }

        public IIndoorMapMaterial InstantiateMaterial(IntPtr materialHandle)
        {
            var templateMaterial = materialHandle.NativeHandleToObject<IIndoorMapMaterial>();
            return m_materialRepository.InstantiateMaterial(templateMaterial);
        }


        [MonoPInvokeCallback(typeof(DeleteMaterialCallback))]
        internal static void DeleteMaterial(IntPtr materialServiceHandle, IntPtr materialHandle)
        {
            var material = materialHandle.NativeHandleToObject<IIndoorMapMaterial>();
            var materialService = materialServiceHandle.NativeHandleToObject<IndoorMapMaterialService>();
            materialService.m_materialRepository.DeleteMaterial(material);
            NativeInteropHelpers.FreeNativeHandle(materialHandle);
        }

        public IndoorMapMaterialService(IndoorMapMaterialRepository materialRepository, IndoorMapsApiInternal indoorMapsApiInternal)
        {
            m_materialRepository = materialRepository;
            m_indoorMapsApiInternal = indoorMapsApiInternal;
            m_handleToSelf = NativeInteropHelpers.AllocateNativeHandleForObject(this);
        }

        internal void Destroy()
        {
            NativeInteropHelpers.FreeNativeHandle(m_handleToSelf);
        }

        public IntPtr GetHandle()
        {
            return m_handleToSelf;
        }

        private IndoorMapMaterialRepository m_materialRepository;
        private IndoorMapsApiInternal m_indoorMapsApiInternal;

        private IntPtr m_handleToSelf;
    }
}