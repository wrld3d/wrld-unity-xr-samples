using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace WRLD.ARCore
{
	public class WRLDARCoreSetupHelper : MonoBehaviour 
	{

		[Serializable]
		public struct CubeInfo
		{
			public GameObject cubeGameObject;
			public double latitudeDegrees;
			public double longitudeDegrees;
		}

		public GameObject arCoreDeviceGameObject;
		public Camera mainCamera;
		public Camera streamingCamera;

		public GameObject wrldMapGameObject;
		public Transform wrldMapParentTransform;
		public Transform wrldMapMaskTransform;

		public double wrldStartLatitudeDegrees;
		public double wrldStartLongitudeDegrees;

		public string wrldMaterialDirectory;

		public CubeInfo[] cubeInfos;
	}
}
