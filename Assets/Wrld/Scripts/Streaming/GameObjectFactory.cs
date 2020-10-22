using UnityEngine;

namespace Wrld.Streaming
{
    class GameObjectFactory
    {
        private static string CreateGameObjectName(string baseName, int meshIndex)
        {
            return string.Format("{0}_INDEX{1}", baseName, meshIndex);
        }

        private GameObject CreateGameObject(Mesh mesh, Material material, string objectName, Transform parentTransform, CollisionStreamingType collisionType, bool shouldUploadToGPU)
        {
            var gameObject = new GameObject(objectName);
            gameObject.SetActive(false);
            gameObject.transform.SetParent(parentTransform, false);

            gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;

            if (objectName.ToLower().Contains("interior"))
            {
                // Making a copy of the indoor material at this point as each indoor renderable has a separate material
                // state.  This is updated during the render loop for non-unity platforms, but for unity we need our
                // materials to be immutable at render time.
                gameObject.AddComponent<MeshRenderer>().sharedMaterial = new Material(material);
            }
            else
            {
                gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
            }

            switch (collisionType)
            {
                case CollisionStreamingType.NoCollision:
                {
                    if(shouldUploadToGPU)
                    {
                        mesh.UploadMeshData(true);
                    }

                    break;
                }

                case CollisionStreamingType.SingleSidedCollision:
                {
                    gameObject.AddComponent<MeshCollider>().sharedMesh = mesh;
                    break;
                }
                case CollisionStreamingType.DoubleSidedCollision:
                {
                    gameObject.AddComponent<MeshCollider>().sharedMesh = CreateDoubleSidedCollisionMesh(mesh);
                    break;
                }
            }
            return gameObject;
        }

        public GameObject[] CreateGameObjects(Mesh[] meshes, Material material, Transform parentTransform, CollisionStreamingType collisionType, bool shouldUploadToGPU)
        {
            var gameObjects = new GameObject[meshes.Length];

            for (int meshIndex = 0; meshIndex < meshes.Length; ++meshIndex)
            {
                var name = CreateGameObjectName(meshes[meshIndex].name, meshIndex);
                gameObjects[meshIndex] = CreateGameObject(meshes[meshIndex], material, name, parentTransform, collisionType, shouldUploadToGPU);
            }

            return gameObjects;
        }


        private static Mesh CreateDoubleSidedCollisionMesh(Mesh sourceMesh)
        {
            Mesh mesh = new Mesh();
            mesh.name = sourceMesh.name;
            mesh.vertices = sourceMesh.vertices;

            int[] sourceTriangles = sourceMesh.triangles;
            int triangleCount = sourceTriangles.Length;
            int[] triangles = new int[triangleCount * 2];

            for (int index=0; index<triangleCount; index += 3)
            {
                // Copy all source triangles into first half of array
                triangles[index+0] = sourceTriangles[index+0];
                triangles[index+1] = sourceTriangles[index+1];
                triangles[index+2] = sourceTriangles[index+2];

                // Insert flipped triangles into second half of array
                triangles[triangleCount + index + 0] = sourceTriangles[index+0];
                triangles[triangleCount + index + 1] = sourceTriangles[index+2];
                triangles[triangleCount + index + 2] = sourceTriangles[index+1];
            }

            mesh.triangles = triangles;
            return mesh;
        }
    }
}
