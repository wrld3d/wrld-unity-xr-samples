using UnityEngine;

public class RotateObjectAR : MonoBehaviour 
{
    const float RotationSpeed = 180f;
    
    void Update () 
    {
        transform.Rotate (Vector3.up, RotationSpeed * Time.deltaTime);
    }
}
