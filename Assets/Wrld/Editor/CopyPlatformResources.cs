using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using System.IO;
using System;

namespace Wrld.Editor
{
    [InitializeOnLoad]
    public class CopyPlatformResources : UnityEditor.Editor
    {
        const string StreamingResourcesDirectory = "Assets/StreamingAssets/WrldResources";
        const string WindowsResourcesDirectory = "Assets/Wrld/AssetData/Windows/";
        const string iOSResourcesDirectory = "Assets/Wrld/AssetData/iOS/";
        const string OSXResourcesDirectory = "Assets/Wrld/Plugins/x86_64/StreamAlpha.bundle/Contents/Resources/";
        const string EmptyResourcesDirectory = "Assets/Wrld/Version/";
        private static ActiveBuildTargetListener m_activeBuildTargetListener = new ActiveBuildTargetListener();
        private static WillPlayInEditorListener m_willPlayInEditorListener = new WillPlayInEditorListener();
        private static bool m_isSubscribedToBuildTargetChanges = false;

        static CopyPlatformResources()
        {
            if (!m_isSubscribedToBuildTargetChanges)
            {
                m_activeBuildTargetListener.ActiveBuildTargetChanged += OnActiveBuildTargetChanged;
                m_willPlayInEditorListener.WillPlayInEditor += OnWillPlayInEditor;
                m_isSubscribedToBuildTargetChanges = true;
            }
        }

        static DateTime GetLastSourceWriteTimeUtc(string sourceDir)
        {
            var sourceDirectories = Directory.GetDirectories(sourceDir);
            var lastWriteTime = Directory.GetLastWriteTimeUtc(sourceDir);

            foreach (var directory in sourceDirectories)
            {
                var lastWriteTimeForDirectory = GetLastSourceWriteTimeUtc(directory);
                lastWriteTime = lastWriteTimeForDirectory > lastWriteTime ? lastWriteTimeForDirectory : lastWriteTime;
            }

            var sourceFiles = Directory.GetFiles(sourceDir);

            foreach (var file in sourceFiles)
            {
                var lastWriteTimeForFile = File.GetLastWriteTimeUtc(file);
                lastWriteTime = lastWriteTimeForFile > lastWriteTime ? lastWriteTimeForFile : lastWriteTime;
            }

            return lastWriteTime;
        }

        static void CopyDirectoryRecursive(string from, string to)
        {
            Directory.CreateDirectory(to);

            var sourceDirectories = Directory.GetDirectories(from);

            foreach (var directory in sourceDirectories)
            {
                var dirName = Path.GetFileName(directory);
                CopyDirectoryRecursive(directory, Path.Combine(to, dirName));
            }

            var sourceFiles = Directory.GetFiles(from);

            foreach (var file in sourceFiles)
            {
                var fileName = Path.GetFileName(file);

                File.Copy(file, Path.Combine(to, fileName));
            }
        }

        static void CleanResourcesDirectory()
        {
            FileUtil.DeleteFileOrDirectory(StreamingResourcesDirectory);
        }

        static string GetSourceDetailsFilePath(string toDirectory)
        {
            return Path.Combine(toDirectory, "sourceDetails");
        }

        static string GetCurrentSourceDetails(string sourceDetailsFile)
        {
            try
            {
                return File.ReadAllText(sourceDetailsFile);
            }
            catch
            {
                return null;
            }
        }

        static void UpdateAssets(string fromDirectory, string toDirectory)
        {
            string sourceDetailsPath = GetSourceDetailsFilePath(toDirectory);
            string sourceDetails = string.Format("{0} {1:O}", fromDirectory, GetLastSourceWriteTimeUtc(fromDirectory));

            if (sourceDetails != GetCurrentSourceDetails(sourceDetailsPath))
            {
                AssetDatabase.StartAssetEditing();
                CleanResourcesDirectory();
                CopyDirectoryRecursive(fromDirectory, toDirectory);
                File.WriteAllText(sourceDetailsPath, sourceDetails);
                AssetDatabase.StopAssetEditing();
            }
        }

        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
        {
            CopyPluginsForStandaloneBuildIfRequired(buildTarget);
        }

        // workaround for https://issuetracker.unity3d.com/issues/plugins-not-copied-to-the-standalone-plugin-folder-when-they-are-marked-to-be-included-in-build
        private static void CopyPluginsForStandaloneBuildIfRequired(BuildTarget buildTarget)
        {
            if (PlatformHelpers.IsStandaloneOSX(buildTarget))
            {
                const string pluginBundle = "StreamAlpha.bundle";

                foreach (var pluginImporter in PluginImporter.GetImporters(buildTarget))
                {
                    if (pluginImporter.assetPath.EndsWith(pluginBundle))
                    {
                        var appDirectory = EditorUserBuildSettings.GetBuildLocation(buildTarget);
                        var destination = Path.Combine(appDirectory, Path.Combine("Contents/Plugins/", pluginBundle));
                        var source = pluginImporter.assetPath;

                        bool shouldCopy = !Directory.Exists(destination) ||
                            GetLastSourceWriteTimeUtc(source) > GetLastSourceWriteTimeUtc(destination);

                        if (shouldCopy)
                        {
                            CopyDirectoryRecursive(source, destination);
                        }
                    }
                }
            }
        }

        [MenuItem("Assets/Setup WRLD Resources For/Android")]
        public static void CopyResourcesAndroid()
        {
            UpdateAssets(EmptyResourcesDirectory, StreamingResourcesDirectory);
        }

        [MenuItem("Assets/Setup WRLD Resources For/iOS")]
        public static void CopyResourcesiOS()
        {
            UpdateAssets(iOSResourcesDirectory, StreamingResourcesDirectory);
        }

        [MenuItem("Assets/Setup WRLD Resources For/OS X")]
        public static void CopyResourcesOSX()
        {
            UpdateAssets(OSXResourcesDirectory, StreamingResourcesDirectory);
        }

        [MenuItem("Assets/Setup WRLD Resources For/Windows")]
        public static void CopyResourcesWin()
        {
            UpdateAssets(WindowsResourcesDirectory, StreamingResourcesDirectory + "/Resources");
        }

        public static void OnWillPlayInEditor()
        {
            PrepareForPlatform(Application.platform);
        }

        public static void CheckBuildTarget(BuildTarget target)
        {
            if (!PlatformHelpers.IsSupportedBuildTarget(target))
            {
                BuildTarget buildTargetOverride;

                if (PlatformHelpers.TryGetBuildTargetOverride(target, out buildTargetOverride))
                {
                    string currentArch = PlatformHelpers.GetOverridableTargetArchitectureString(target);
                    string supportedArch = "x86_64";
                    string message = "Your Build Architecture has been set to " + currentArch + ".  This is currently not supported and builds made using this architecture will not run.\n\n" +
                        "Do you wish to change to " + supportedArch + " (recommended)? ";

                    if (EditorUtility.DisplayDialog("WRLD - Target Settings", message, "Change", "Skip"))
                    {
                        PlatformHelpers.SwitchActiveBuildTarget(buildTargetOverride);
                    }
                }
            }
        }

        public static void PrepareForPlatform(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.Android:
                    CopyResourcesAndroid();
                    break;
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    CopyResourcesWin();
                    break;
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    CopyResourcesOSX();
                    break;
                case RuntimePlatform.IPhonePlayer:
                    CopyResourcesiOS();
                    break;
                default:
                    Debug.LogErrorFormat("Unsupported platform {0:G}", platform);
                    break;
            }
        }

        public static void PrepareForBuildTarget(BuildTarget target)
        {
            RuntimePlatform platform;

            if (PlatformHelpers.TryGetRuntimePlatformForBuildTarget(target, out platform))
            {
                PrepareForPlatform(platform);
            }
            else
            {
                Debug.LogErrorFormat("Unsupported platform {0:G}", target);
            }
        }

        public static void OnActiveBuildTargetChanged()
        {
            CheckBuildTarget(EditorUserBuildSettings.activeBuildTarget);
            PrepareForBuildTarget(EditorUserBuildSettings.activeBuildTarget);
        }

        [PostProcessSceneAttribute]
        public static void OnPostProcessScene()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                PrepareForBuildTarget(EditorUserBuildSettings.activeBuildTarget);
            }
        }

        private static string ReplaceExtension(string fileName, string oldExtension, string newExtension)
        {
            var result = fileName.IndexOf(oldExtension);
            if (result < 0)
            {
                throw new ArgumentException(string.Format("File {0} doesn't contain *.{1} extension", fileName, oldExtension));
            }
            return fileName.Substring(0, result) + newExtension;
        }

        private static void ReplaceFileExtensionsRecursively(string rootDir, string oldExtension, string newExtension)
        {
            var allFiles = Directory.GetFiles(rootDir, "*." + oldExtension, SearchOption.AllDirectories);
            foreach (var file in allFiles)
            {
                var newFileName = ReplaceExtension(file, oldExtension, newExtension);
                File.Move(file, newFileName);
            }
        }

        public static void RenameFiles()
        {
            const string PackageUploadFile = "Assets/WRLDPackageUpload.txt";
            bool isPackageUpload = File.Exists(PackageUploadFile);

            if (!isPackageUpload)
            {
                const string WrldDirectory = "Assets/Wrld";
                ReplaceFileExtensionsRecursively(WrldDirectory, "dds_wrld", "dds");
                ReplaceFileExtensionsRecursively(WrldDirectory, "pvr_wrld", "pvr");
            }

            Debug.Log("WRLD platform is setup and ready to use");
        }
    }
}

