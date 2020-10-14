using UnityEditor;
using System.IO;
using UnityEngine;
using System.Reflection;
using System;

// adapted from ideas discussed in https://forum.unity3d.com/threads/unity-xcode-api.281305/
public class XcodeProjectUpdater
{
    public static void TweakXcodeProjectSettings(BuildTarget buildTarget, string path)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            string projectPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
            string originalProject = File.ReadAllText(projectPath);
            string adaptedProject = GetAdaptedProjectContent(originalProject);
            File.WriteAllText(projectPath, adaptedProject);
        }
    }

    private static string GetAdaptedProjectContent(string originalProjectContent)
    {
        // Unity used to allow us to use UNITY_IOS #ifs to protect stuff that would 
        // only  compile when building to iOS from the editor (and with the iOS module installed).
        // Looks like latest iOS doesn't actually support this (or doesn't set the defines
        // in time), so resorting to reflection to get the work done.
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var pbxProjectType = assembly.GetType("UnityEditor.iOS.Xcode.PBXProject");

            if (pbxProjectType != null)
            {
                var pbxProject = Activator.CreateInstance(pbxProjectType);
                CallMethodName(pbxProject, pbxProjectType, "ReadFromString", 
                    new [] { typeof(string) }, 
                    originalProjectContent);

                MutateProject(pbxProject, pbxProjectType);

                string result = (string)CallMethodName(pbxProject, pbxProjectType, "WriteToString", 
                    new Type[] {});

                return result;
            }
        }

        return originalProjectContent;
    }

    private static bool TryRemoveFileFromProject(object pbxProject, Type pbxProjectType, string targetFilePath)
    {
        string targetFileGuid = (string)CallMethodName(
            pbxProject, pbxProjectType, "FindFileGuidByProjectPath",
            new[] { typeof(string) },
            targetFilePath);

        if (targetFileGuid != null)
        {
            CallMethodName(
                pbxProject, pbxProjectType, "RemoveFile",
                new[] { typeof(string) },
                targetFileGuid);

            return true;
        }

        return false;
    }

    private static void MutateProject(object pbxProject, Type pbxProjectType)
    {   
        string mainTarget = (string)CallMethodName(pbxProject, pbxProjectType, "GetUnityMainTargetGuid");
        string frameworkTarget = (string)CallMethodName(pbxProject, pbxProjectType, "GetUnityFrameworkTargetGuid");

        AddFrameworks(pbxProject, pbxProjectType, frameworkTarget);
        AddDylibs(pbxProject, pbxProjectType, frameworkTarget);

        CallMethodName(
            pbxProject, pbxProjectType, "SetBuildProperty", 
            new[] { typeof(string), typeof(string), typeof(string) }, 
            mainTarget, "ENABLE_BITCODE", "false");

        CallMethodName(
            pbxProject, pbxProjectType, "SetBuildProperty", 
            new[] { typeof(string), typeof(string), typeof(string) }, 
            frameworkTarget, "ENABLE_BITCODE", "false");

        // Unity will find and copy this x86_64 bundle into the project as a framework, if 
        // it is allowed, preventing it from archiving correctly.  As we're on iOS in this 
        // case we don't need it and can remove it.
        TryRemoveFileFromProject(pbxProject, pbxProjectType, "Frameworks/Wrld/Plugins/x86_64/StreamAlpha.bundle");
        TryRemoveFileFromProject(pbxProject, pbxProjectType, "Frameworks/StreamAlpha.bundle");
    }

    private static object CallMethodName(object pbxProject, Type pbxProjectType, string methodName, Type[] parameterTypes, params object[] arguments)
    {
        var method = pbxProjectType.GetMethod(methodName, parameterTypes);

        return method.Invoke(pbxProject, arguments);
    }

    private static object CallMethodName(object pbxProject, Type pbxProjectType, string methodName)
    {
        var method = pbxProjectType.GetMethod(methodName);

        return method.Invoke(pbxProject, null);
    }
        
    private static void AddFrameworks(object pbxProject, Type pbxProjectType, string target)
    {
        const bool weak = false;
        var parameterTypes = new [] { typeof(string), typeof(string), typeof(bool) };

        CallMethodName(pbxProject, pbxProjectType, "AddFrameworkToProject", 
            parameterTypes, 
            target, "MobileCoreServices.framework", weak);
        
        CallMethodName(pbxProject, pbxProjectType, "AddFrameworkToProject", 
            parameterTypes, 
            target, "Security.framework", weak);
    }

    private static void AddDylibs(object pbxProject, Type pbxProjectType, string target)
    {
        AddDylib(pbxProject, pbxProjectType, target, "libz.dylib");
    }

    private static object GetEnumValue(Type enumType, string enumName)
    {
        if (enumType != null)
        {
            int index = 0;

            foreach (var name in Enum.GetNames(enumType))
            {
                if (name == enumName)
                {
                    return Enum.GetValues(enumType).GetValue(index);
                }

                ++index;
            }
        }

        return null;
    }

    private static void AddDylib(object pbxProject, Type pbxProjectType, string target, string dylib)
    {
        string path = "usr/lib/" + dylib;
        string projectPath = "Frameworks/" + dylib;
        var enumType = pbxProjectType.Assembly.GetType("UnityEditor.iOS.Xcode.PBXSourceTree");
        object enumValue = GetEnumValue(enumType, "Sdk");
        
        string fileGuid = (string)CallMethodName(
            pbxProject, pbxProjectType, "AddFile", 
            new[] { typeof(string), typeof(string), enumType }, 
            path, projectPath, enumValue);

        CallMethodName(
            pbxProject, pbxProjectType, "AddFileToBuild", 
            new [] { typeof(string), typeof(string) }, 
            target, fileGuid);
    }
}

