using System;
using UnityEditor;

namespace Wrld.Editor
{
    public class WillPlayInEditorListener
    {
        public WillPlayInEditorListener()
        {
            #if UNITY_2017_2_OR_NEWER
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
            #else
            EditorApplication.playmodeStateChanged += NotifyObserversIfWillPlayInEditor;
            #endif
        }

        #if UNITY_2017_2_OR_NEWER
        private void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
        {
            NotifyObserversIfWillPlayInEditor();
        }
        #endif

        private void NotifyObserversIfWillPlayInEditor()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
            {
                var willPlayInEditor = WillPlayInEditor;

                if (willPlayInEditor != null)
                {
                    willPlayInEditor();
                }
            }
        }

        public event Action WillPlayInEditor;
    }
}

