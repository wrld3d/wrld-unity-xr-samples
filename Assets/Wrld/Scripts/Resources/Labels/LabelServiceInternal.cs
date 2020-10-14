using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Wrld.Interop;
using Wrld.Materials;
using Wrld.Utilities;

namespace Wrld.Resources.Labels
{
    internal class LabelServiceInternal
    {
        private Dictionary<string, LabelView> m_labelViews = new Dictionary<string, LabelView>();
        private Canvas m_unityCanvas = null;
        private List<Texture> m_iconTexturePages = new List<Texture>();
        private TextureLoadHandler m_textureLoadHandler;

        private IntPtr m_handleToSelf;

        private bool m_enableLabels;
        private bool m_spawnedCanvas;

        const string DesiredFontName = "OpenSans-SemiBold";
        const string IncorrectFontMessage = "Assets/Wrld/Resources/Labels/ScreenTextPrefab.prefab has a non-default font: \"{0}\". Visual artifacts with labels may occur. Please use \"" + DesiredFontName + "\" instead.";
        const string LabelTextPrefabPath = "Labels/ScreenTextPrefab";
        const string CanvasName = "WRLDLabelCanvas";
        const string CanvasPath = "Labels/WRLDLabelCanvas";

        public LabelServiceInternal(GameObject unityCanvas, bool enabled, TextureLoadHandler textureLoadHandler)
        {
            if (unityCanvas == null)
            {
                if (enabled)
                {
                    var newUnityCanvas = GameObject.Instantiate(UnityEngine.Resources.Load<GameObject>(CanvasPath));
                    newUnityCanvas.name = CanvasName;
                    newUnityCanvas.transform.SetAsFirstSibling();
                    m_unityCanvas = newUnityCanvas.GetComponent<Canvas>();
                    m_spawnedCanvas = true;

                    ValidateFont();
                }
            }
            else
            {
                m_unityCanvas = unityCanvas.GetComponent<Canvas>();
            }

            m_handleToSelf = NativeInteropHelpers.AllocateNativeHandleForObject(this);
            m_enableLabels = enabled && m_unityCanvas != null;
            m_textureLoadHandler = textureLoadHandler;
        }

        public IntPtr GetHandle()
        {
            return m_handleToSelf;
        }

        public void AddLabel(ref LabelCreateOptionsInterop createOptions)
        {
            var labelID = Marshal.PtrToStringAnsi(createOptions.LabelID);

            if (m_enableLabels)
            {
                if (m_labelViews.ContainsKey(labelID))
                {
                    DestroyLabel(labelID);
                }

                var labelView = new LabelView(ref createOptions, m_unityCanvas, m_iconTexturePages);

                m_labelViews.Add(labelID, labelView);
            }
        }

        void UpdateLabel(ref LabelUpdateStateInterop updateState)
        {
            LabelView labelView;
            var labelID = Marshal.PtrToStringAnsi(updateState.LabelID);

            if (m_labelViews.TryGetValue(labelID, out labelView))
            {
                labelView.Update(ref updateState, m_unityCanvas);
            }
        }

        public void DestroyLabel(string labelId)
        {
            if (m_labelViews.ContainsKey(labelId))
            {
                m_labelViews[labelId].Destroy();
                m_labelViews.Remove(labelId);
            }
        }

        void AddIconTexturePage(UInt32 textureId)
        {
            m_textureLoadHandler.Update();
            var texturePage = m_textureLoadHandler.GetTexture(textureId);
            if(texturePage != null)
            {
                m_iconTexturePages.Add(texturePage);
            }
        }

        internal void ValidateFont()
        {
            var screenTextPrefab = (GameObject)UnityEngine.Resources.Load(LabelTextPrefabPath, typeof(GameObject));
            var screenTextElement = screenTextPrefab.GetComponent<UnityEngine.UI.Text>();
            
            if(screenTextElement.font.name != DesiredFontName)
            {
                Debug.LogWarningFormat(IncorrectFontMessage, screenTextElement.font.name);
            }
        }

        internal void Destroy()
        {
            var keys = new List<string>(m_labelViews.Keys);

            foreach (string labelId in keys)
            {
                DestroyLabel(labelId);
            }

            if(m_spawnedCanvas && m_unityCanvas != null)
            {
                GameObject.DestroyImmediate(m_unityCanvas.gameObject);
            }

            NativeInteropHelpers.FreeNativeHandle(m_handleToSelf);
        }

        public delegate void AddLabelDelegate(IntPtr labelServiceHandle, ref LabelCreateOptionsInterop createOptions);

        [MonoPInvokeCallback(typeof(AddLabelDelegate))]
        public static void AddLabel(IntPtr labelServiceHandle, ref LabelCreateOptionsInterop createOptions)
        {
            var labelService = labelServiceHandle.NativeHandleToObject<LabelServiceInternal>();
            labelService.AddLabel(ref createOptions);
        }

        public delegate void UpdateLabelDelegate(IntPtr labelServiceHandle, ref LabelUpdateStateInterop updateState);

        [MonoPInvokeCallback(typeof(UpdateLabelDelegate))]
        public static void UpdateLabel(IntPtr labelServiceHandle, ref LabelUpdateStateInterop updateState)
        {
            var labelService = labelServiceHandle.NativeHandleToObject<LabelServiceInternal>();
            labelService.UpdateLabel(ref updateState);
        }
        
        public delegate void RemoveLabelDelegate(IntPtr labelServiceHandle, [MarshalAs(UnmanagedType.LPStr)] string labelId);

        [MonoPInvokeCallback(typeof(RemoveLabelDelegate))]
        public static void RemoveLabel(IntPtr labelServiceHandle, [MarshalAs(UnmanagedType.LPStr)] string labelId)
        {
            var labelService = labelServiceHandle.NativeHandleToObject<LabelServiceInternal>();
            labelService.DestroyLabel(labelId);
        }

        public delegate void AddIconTexturePageDelegate(IntPtr labelServiceHandle, UInt32 textureId);

        [MonoPInvokeCallback(typeof(AddIconTexturePageDelegate))]
        public static void AddIconTexturePage(IntPtr labelServiceHandle, UInt32 textureId)
        {
            var labelService = labelServiceHandle.NativeHandleToObject<LabelServiceInternal>();

            labelService.AddIconTexturePage(textureId);
        }
    }
}
