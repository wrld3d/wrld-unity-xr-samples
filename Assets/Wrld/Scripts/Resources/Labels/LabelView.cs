using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wrld.Interop;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Wrld.Resources.Labels
{
    internal class LabelView
    {
        private UnityEngine.UI.Text m_textComponent;
        private UnityEngine.UI.RawImage m_iconComponent;

        const string LabelTextPrefabPath = "Labels/ScreenTextPrefab";
        const string LabelIconPrefabPath = "Labels/ScreenIconPrefab";

        public LabelView(ref LabelCreateOptionsInterop createOptions, Canvas unityCanvas, List<Texture> iconTexturePages)
        {
            if(createOptions.HasTextComponent)
            {
                var newTextGameObject = GameObject.Instantiate(UnityEngine.Resources.Load<GameObject>(LabelTextPrefabPath));
                var createOptionsText = Marshal.PtrToStringAnsi(createOptions.Text);
                newTextGameObject.name = createOptionsText;

                m_textComponent = newTextGameObject.GetComponent<UnityEngine.UI.Text>();
                m_textComponent.fontSize = createOptions.BaseFontSize;
                m_textComponent.text = createOptionsText;
                m_textComponent.transform.SetParent(unityCanvas.transform, false);
                m_textComponent.transform.SetAsFirstSibling();

                float fontScaleFactor = (float)createOptions.FontScale / unityCanvas.scaleFactor;
                m_textComponent.transform.localScale = new Vector3(fontScaleFactor, fontScaleFactor, fontScaleFactor);

                var newOutline = newTextGameObject.GetComponent<UnityEngine.UI.Outline>();
                if (newOutline != null)
                {
                    newOutline.effectColor = createOptions.HaloColor.ToColor();
                }
            }

            if(createOptions.HasIconComponent)
            {
                var newIconGameObject = GameObject.Instantiate(UnityEngine.Resources.Load<GameObject>(LabelIconPrefabPath));
                newIconGameObject.name = createOptions.Text + " - Icon";

                m_iconComponent = newIconGameObject.GetComponent<UnityEngine.UI.RawImage>();
                m_iconComponent.transform.SetParent(unityCanvas.transform, false);
                m_iconComponent.transform.SetAsFirstSibling();

                m_iconComponent.texture = iconTexturePages[createOptions.iconTexturePage];

                var uvScale = new Vector2(1.0f / 65536.0f, 1.0f / 65536.0f); // UV Scaling done on Native-side when texture is loaded, corrected here.

                m_iconComponent.uvRect = new Rect((float)createOptions.iconU0 * uvScale.x, (float)createOptions.iconV1 * uvScale.y, (float)createOptions.iconU1 * uvScale.x - (float)createOptions.iconU0 * uvScale.x, (float)createOptions.iconV0 * uvScale.y - (float)createOptions.iconV1 * uvScale.y);
                m_iconComponent.rectTransform.sizeDelta = new Vector2((float)createOptions.iconWidth, (float)createOptions.iconHeight);
                
                float iconScaleFactor = 1.0f / unityCanvas.scaleFactor;
                m_iconComponent.transform.localScale = new Vector3(iconScaleFactor, iconScaleFactor, iconScaleFactor);
            }
        }

        public void Update(ref LabelUpdateStateInterop updateState, Canvas unityCanvas)
        {
            if(m_textComponent != null)
            {
                float newTextPosX = updateState.TextPosition.x - (unityCanvas.pixelRect.width / 2);
                float newTextPosY = (unityCanvas.pixelRect.height - updateState.TextPosition.y) - (unityCanvas.pixelRect.height / 2);

                newTextPosX /= unityCanvas.scaleFactor;
                newTextPosY /= unityCanvas.scaleFactor;

                m_textComponent.color = updateState.TextColor.ToColor();

                m_textComponent.rectTransform.localPosition = new Vector3(newTextPosX, newTextPosY, 0);
                m_textComponent.rectTransform.localRotation = Quaternion.Euler(0, 0, (float)updateState.TextRotationAngleDegrees);
            }

            if(m_iconComponent != null)
            {
                float newIconPosX = updateState.IconPosition.x - (unityCanvas.pixelRect.width / 2);
                float newIconPosY = (unityCanvas.pixelRect.height - updateState.IconPosition.y) - (unityCanvas.pixelRect.height / 2);

                newIconPosX /= unityCanvas.scaleFactor;
                newIconPosY /= unityCanvas.scaleFactor;

                m_iconComponent.color = updateState.IconColor.ToColor();

                m_iconComponent.rectTransform.localPosition = new Vector3(newIconPosX, newIconPosY, 0);
                m_iconComponent.rectTransform.localRotation = Quaternion.Euler(0, 0, (float)updateState.IconRotationAngleDegrees);
            }

        }

        public void Destroy()
        {
            if(m_textComponent != null)
            {
                if(!m_textComponent.IsDestroyed())
                {
                    GameObject.DestroyImmediate(m_textComponent);
                }
            }

            if (m_iconComponent != null)
            {
                if (!m_iconComponent.IsDestroyed())
                {
                    GameObject.DestroyImmediate(m_iconComponent);
                }
            }
        }
    }
}
