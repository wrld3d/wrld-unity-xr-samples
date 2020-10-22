using System;

#if UNITY_2017_1_OR_NEWER

using UnityEditor.Build;

namespace Wrld.Editor
{
    public class ActiveBuildTargetListener : IActiveBuildTargetChanged
    {
        public int callbackOrder { get { return 0; } }

        public void OnActiveBuildTargetChanged(UnityEditor.BuildTarget previousTarget, UnityEditor.BuildTarget newTarget)
        {
            var buildTargetChanged = ActiveBuildTargetChanged;

            if (buildTargetChanged != null)
            {
                buildTargetChanged();
            }
        }

        public event Action ActiveBuildTargetChanged;
    }
}

#else

using UnityEditor;

namespace Wrld.Editor
{
    public class ActiveBuildTargetListener
    {
        public ActiveBuildTargetListener()
        {
            EditorUserBuildSettings.activeBuildTargetChanged += OnActiveBuildTargetChanged;
        }

        public void OnActiveBuildTargetChanged()
        {
            var buildTargetChanged = ActiveBuildTargetChanged;

            if (buildTargetChanged != null)
            {
                buildTargetChanged();
            }
        }

        public event Action ActiveBuildTargetChanged;
    }
}

#endif

