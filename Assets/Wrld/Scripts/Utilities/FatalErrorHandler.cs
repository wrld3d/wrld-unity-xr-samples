using AOT;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wrld
{
    public class FatalErrorHandler
    {
        public delegate void HandleFatalErrorCallback([MarshalAs(UnmanagedType.LPStr)] string message);

        [MonoPInvokeCallback(typeof(HandleFatalErrorCallback))]
        public static void HandleFatalError([MarshalAs(UnmanagedType.LPStr)] string message)
        {
            Debug.LogErrorFormat("Wrld FATAL ERROR: {0}", message);
            
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }
    }

}