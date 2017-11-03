using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WRLDARKitSetupHelper : MonoBehaviour 
{
    [Serializable]
    public struct CubeInfo
    {
        public GameObject cubeGameObject;
        public double latitudeDegrees;
        public double longitudeDegrees;
    }

    public Camera mainCamera;
    public Camera streamingCamera;
    public GameObject wrldMap;
    public double latitudeDegrees = 37.771092;
    public double longitudeDegrees = -122.468385;
    public string materialDirectory = "WrldARMaterials/";
    public CubeInfo[] cubes;
}
