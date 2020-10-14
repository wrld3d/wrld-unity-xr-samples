using UnityEngine;

[ExecuteInEditMode]
public class ViewNormals : MonoBehaviour
{
    // Use this for initialization
    private MeshFilter objectMesh;

    void OnDrawGizmos()
    {
        if (objectMesh != null)
        {
            for (int i = 0; i < objectMesh.mesh.vertexCount; ++i)
            {
                var transformedPosition = objectMesh.gameObject.transform.TransformPoint(objectMesh.sharedMesh.vertices[i]);
                var transformedNormal = objectMesh.gameObject.transform.TransformDirection(objectMesh.sharedMesh.normals[i]);
                const float SCALE = 0.01f;
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transformedPosition, transformedPosition + transformedNormal * SCALE);
            }
        }
        else
        {
            objectMesh = GetComponent<MeshFilter>();
        }
    }
}