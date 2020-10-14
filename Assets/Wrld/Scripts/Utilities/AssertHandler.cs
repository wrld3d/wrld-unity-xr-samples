using AOT;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wrld
{
    public class AssertHandler
    {
        public delegate void HandleAssertCallback([MarshalAs(UnmanagedType.LPStr)] string message, [MarshalAs(UnmanagedType.LPStr)] string file, int line);

        [MonoPInvokeCallback(typeof(HandleAssertCallback))]
        public static void HandleAssert([MarshalAs(UnmanagedType.LPStr)] string message, [MarshalAs(UnmanagedType.LPStr)] string file, int line)
        {
            Debug.LogErrorFormat("Wrld ASSERT {0} ({1}): {2}", file, line, message);

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            Debug.Break();
        }
    }

}