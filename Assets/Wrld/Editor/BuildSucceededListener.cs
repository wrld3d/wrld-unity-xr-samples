using UnityEditor;
using UnityEngine;
using UnityEditor.Callbacks;

public class BuildSucceededListener
{
    [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
    {
        OnBuildSucceeded(buildTarget, path);
    }

    private static void OnBuildSucceeded(BuildTarget buildTarget, string path)
    {
        XcodeProjectUpdater.TweakXcodeProjectSettings(buildTarget, path);
    }
}