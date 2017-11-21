using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Reflection;
using UnityEngine.XR.iOS; // Please import Unity ARKit Plugin if you are seeing a compiler error here.
using Wrld.Space; // Please import WRLD Unity SDK if you are seeing a compiler error here.

namespace Wrld.AR.EditorScripts
{
    [InitializeOnLoad]
    [CustomEditor(typeof(WRLDARKitSetupHelper))]
    public class ARKitSceneSetupHandler : UnityEditor.Editor
    {
        private static bool m_editorEventsSubscribed = false;

        static ARKitSceneSetupHandler()
        {
            if (!m_editorEventsSubscribed) 
            {
                EditorSceneManager.sceneOpened += OnEditorSceneOpened;
                m_editorEventsSubscribed = true;
            }
        }

        static void OnEditorSceneOpened (UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
        {
            CheckSceneStatus ();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI ();

            if (GUILayout.Button("Setup ARKit"))
            {
                CheckSceneStatus ();
            }
        }

        private static void CheckSceneStatus()
        {
            WRLDARKitSetupHelper wrldARKitSetupHelper = GameObject.FindObjectOfType<WRLDARKitSetupHelper>();

            if (wrldARKitSetupHelper != null) 
            {
                DisplaySetupDialog ();
            }
        }

        private static void DisplaySetupDialog()
        {
            string messageWithARKit = "Would you like to setup the scene for ARKit?";

            if (EditorUtility.DisplayDialog ("WRLD ARKit Sample", messageWithARKit, "Setup", "Later")) 
            {
                SetupARKitScene ();
            }
        }

        private static void SetupARKitScene() 
        {
            WRLDARKitSetupHelper wrldARKitSetupHelper = GameObject.FindObjectOfType<WRLDARKitSetupHelper>();
            EditorSceneManager.MarkAllScenesDirty ();

            SetupWrldMap (wrldARKitSetupHelper.wrldMap, wrldARKitSetupHelper.streamingCamera, wrldARKitSetupHelper.latitudeDegrees, 
                          wrldARKitSetupHelper.longitudeDegrees, wrldARKitSetupHelper.materialDirectory);
            SetupGeographicsTransforms (wrldARKitSetupHelper.cubes);

            Camera arCamera = wrldARKitSetupHelper.mainCamera;
            SetupUnityARCameraManager (arCamera);
            SetupUnityARCameraNearFar (arCamera.gameObject);
            SetupUnityARVideo (arCamera.gameObject);

            GameObject.DestroyImmediate (wrldARKitSetupHelper.gameObject);
        }

        private static void SetupWrldMap(GameObject wrldMapGameobject, Camera streamingCamera, double latitudeDegrees, 
                                         double longitudeDegrees, string materialDirectory)
        {
            if (wrldMapGameobject != null) 
            {
                WrldMap wrldMap = wrldMapGameobject.AddComponent<WrldMap> ();
                SerializedObject serializedWrldMapObject = new UnityEditor.SerializedObject(wrldMap);
                SerializedProperty streamingCameraProperty = serializedWrldMapObject.FindProperty("m_streamingCamera");
                SerializedProperty latitudeDegreesProperty = serializedWrldMapObject.FindProperty("m_latitudeDegrees");
                SerializedProperty longitudeDegreesProperty = serializedWrldMapObject.FindProperty("m_longitudeDegrees");
                SerializedProperty materialDirectoryProperty = serializedWrldMapObject.FindProperty("m_materialDirectory");

                streamingCameraProperty.objectReferenceValue = streamingCamera;
                latitudeDegreesProperty.doubleValue = latitudeDegrees;
                longitudeDegreesProperty.doubleValue = longitudeDegrees;
                materialDirectoryProperty.stringValue = materialDirectory;

                serializedWrldMapObject.ApplyModifiedProperties ();
            }
            else 
            {
                Debug.LogError ("WrldMap gameobject not referenced in WRLDARKitSetupHelper.");
            }
        }

        private static void SetupGeographicsTransforms(WRLDARKitSetupHelper.CubeInfo[] cubes)
        {
            foreach (var cube in cubes) 
            {
                GeographicTransform geographicTransform = cube.cubeGameObject.AddComponent<GeographicTransform> ();
                SerializedObject serializedObject = new UnityEditor.SerializedObject(geographicTransform);
                SerializedProperty latitudeProperty = serializedObject.FindProperty("InitialLatitude");
                SerializedProperty longitudeProperty = serializedObject.FindProperty("InitialLongitude");

                latitudeProperty.doubleValue = cube.latitudeDegrees;
                longitudeProperty.doubleValue = cube.longitudeDegrees;

                serializedObject.ApplyModifiedProperties ();
            }
        }

        private static void SetupUnityARCameraManager(Camera mainCamera)
        {
            if (mainCamera != null) {
                GameObject arCameraManagerObject = new GameObject ("ARCameraManager");
                UnityARCameraManager arCamManager = arCameraManagerObject.AddComponent<UnityARCameraManager> ();
                arCamManager.m_camera = mainCamera;
            }
            else 
            {
                Debug.LogError ("MainCamera gameobject not referenced in WRLDARKitSetupHelper.");
            }
        }

        private static void SetupUnityARCameraNearFar(GameObject mainCamera)
        {
            if (mainCamera != null) 
            {
                mainCamera.AddComponent<UnityARCameraNearFar> ();
            } 
            else 
            {
                Debug.LogError ("MainCamera gameobject not referenced in WRLDARKitSetupHelper.");
            }
        }

        private static void SetupUnityARVideo(GameObject mainCamera)
        {
            if (mainCamera != null) 
            {
                UnityARVideo unityARVideo = mainCamera.AddComponent<UnityARVideo> ();

                string[] guids = AssetDatabase.FindAssets ("YUVMaterial");

                if (guids != null && guids.Length > 0) 
                {
                    string path = AssetDatabase.GUIDToAssetPath (guids [0]);
                    unityARVideo.m_ClearMaterial = AssetDatabase.LoadAssetAtPath<Material> (path);
                }
                else 
                {
                    Debug.LogError ("YUVMaterial not found in ARKit plugin.");
                }
            }
            else 
            {
                Debug.LogError ("MainCamera gameobject not referenced in WRLDARKitSetupHelper.");
            }
        }
    }
}
