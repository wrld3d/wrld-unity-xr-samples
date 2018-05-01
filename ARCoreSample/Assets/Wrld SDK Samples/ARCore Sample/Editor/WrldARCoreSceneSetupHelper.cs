using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Reflection;

using GoogleARCore; // Please import Google ARCore plugin if you are seeing a compiler error here.
using Wrld.Space; // Please import WRLD3D plugin if you are seeing a compiler error here.

namespace WRLD.ARCore.Editor
{
	[InitializeOnLoad]
	[CustomEditor(typeof(WRLDARCoreSetupHelper))]
	public class WRLDARCoreSceneSetupHelper : UnityEditor.Editor 
	{
		private static bool m_editorEventsSubscribed = false;

		static WRLDARCoreSceneSetupHelper()
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

			if (GUILayout.Button("Setup ARCore"))
			{
				CheckSceneStatus ();
			}
		}

		private static void CheckSceneStatus()
		{
			WRLDARCoreSetupHelper wrldARCoreSetupHelper = GameObject.FindObjectOfType<WRLDARCoreSetupHelper>();
			if (wrldARCoreSetupHelper != null) 
			{
				DisplaySetupDialog ();
			}
		}

		private static void DisplaySetupDialog()
		{
			string messageWithARCore = "Would you like to setup the scene for ARCore?";

			if (EditorUtility.DisplayDialog ("WRLD ARCore Sample", messageWithARCore, "Setup", "Later")) 
			{
				SetupARCoreScene ();
			}
		}

		private static void SetupARCoreScene() 
		{
			EditorSceneManager.MarkAllScenesDirty ();
			WRLDARCoreSetupHelper wrldARCoreSetupHelper = GameObject.FindObjectOfType<WRLDARCoreSetupHelper>();

			SetupWRLDMap (wrldARCoreSetupHelper);
			SetupGeographicsTransforms (wrldARCoreSetupHelper);
			SetupWRLDARCoreManager (wrldARCoreSetupHelper);
			SetupWRLDARMapPositioner (wrldARCoreSetupHelper);

			GameObject.DestroyImmediate (wrldARCoreSetupHelper.gameObject);
		}

		private static void SetupWRLDMap(WRLDARCoreSetupHelper wrldARCoreSetupHelper)
		{
			// Please import WRLD3D plugin if you are seeing a compiler error here.
			WrldMap wrldMap = wrldARCoreSetupHelper.wrldMapGameObject.AddComponent<WrldMap> ();
			SerializedObject serializedWrldMapObject = new UnityEditor.SerializedObject(wrldMap);
			SerializedProperty streamingCameraProperty = serializedWrldMapObject.FindProperty("m_streamingCamera");
			SerializedProperty latitudeDegreesProperty = serializedWrldMapObject.FindProperty("m_latitudeDegrees");
			SerializedProperty longitudeDegreesProperty = serializedWrldMapObject.FindProperty("m_longitudeDegrees");
			SerializedProperty materialDirectoryProperty = serializedWrldMapObject.FindProperty("m_materialDirectory");

			streamingCameraProperty.objectReferenceValue = wrldARCoreSetupHelper.streamingCamera;
			latitudeDegreesProperty.doubleValue = wrldARCoreSetupHelper.wrldStartLatitudeDegrees;
			longitudeDegreesProperty.doubleValue = wrldARCoreSetupHelper.wrldStartLongitudeDegrees;
			materialDirectoryProperty.stringValue = wrldARCoreSetupHelper.wrldMaterialDirectory;

			serializedWrldMapObject.ApplyModifiedProperties ();
		}


		private static void SetupGeographicsTransforms(WRLDARCoreSetupHelper wrldARCoreSetupHelper)
		{
			WRLDARCoreSetupHelper.CubeInfo[] cubes;
			foreach (WRLDARCoreSetupHelper.CubeInfo cubeInfo in wrldARCoreSetupHelper.cubeInfos) 
			{
				// Please import WRLD3D plugin if you are seeing a compiler error here.
				GeographicTransform geographicTransform = cubeInfo.cubeGameObject.AddComponent<GeographicTransform> ();
				SerializedObject serializedObject = new UnityEditor.SerializedObject(geographicTransform);
				SerializedProperty latitudeProperty = serializedObject.FindProperty("InitialLatitude");
				SerializedProperty longitudeProperty = serializedObject.FindProperty("InitialLongitude");

				latitudeProperty.doubleValue = cubeInfo.latitudeDegrees;
				longitudeProperty.doubleValue = cubeInfo.longitudeDegrees;

				serializedObject.ApplyModifiedProperties ();
			}
		}

		private static void SetupWRLDARCoreManager(WRLDARCoreSetupHelper wrldARCoreSetupHelper)
		{
			GameObject wrldARCoreManagerGameObject = new GameObject ("WRLDARCoreManager");
			WRLDARCoreManager wrldARCoreManager = wrldARCoreManagerGameObject.AddComponent<WRLDARCoreManager> ();

			wrldARCoreManager.wrldMap = wrldARCoreSetupHelper.wrldMapParentTransform;
			wrldARCoreManager.wrldMapMask = wrldARCoreSetupHelper.wrldMapMaskTransform;
		}

		private static void SetupWRLDARMapPositioner (WRLDARCoreSetupHelper wrldARCoreSetupHelper)
		{
			WRLDARCorePositioner wrldARCorePositioner = wrldARCoreSetupHelper.wrldMapParentTransform.gameObject.AddComponent<WRLDARCorePositioner> ();
			wrldARCorePositioner.wrldMapMask = wrldARCoreSetupHelper.wrldMapMaskTransform;
		}
	}
}