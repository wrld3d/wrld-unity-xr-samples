using Wrld.Common.Maths;
using System;
using UnityEngine;

namespace Wrld.Meshes
{
    public class PreparedMesh
    {
        private Vector3[] m_verts;
        private Vector2[] m_uvs;
        private Vector2[] m_uv2s;
        private Vector3[] m_normals;
        private Color32[] m_colors;
        private int[] m_indices;
        private string m_name;

        public string Name { get { return m_name; } }

        public static PreparedMesh CreateFromArrays(Vector3[] verts, Vector2[] uvs, Vector2[] uv2s, Vector3[] normals, Color32[] colors, int[] indices, string name, string materialName, DoubleVector3 originECEF)
        {
            var preparedMesh = new PreparedMesh();
            preparedMesh.m_verts = verts;
            preparedMesh.m_uvs = uvs;
            preparedMesh.m_uv2s = uv2s;
            preparedMesh.m_colors = colors;
            preparedMesh.m_indices = indices;
            preparedMesh.m_name = name;

            bool isInstancedIndoorMesh = name.StartsWith("InteriorInstance");

            if (isInstancedIndoorMesh)
            {
                for (int i = 0; i < preparedMesh.m_indices.Length; i += 3)
                {
                    int temp = preparedMesh.m_indices[i + 0];
                    preparedMesh.m_indices[i + 0] = preparedMesh.m_indices[i + 1];
                    preparedMesh.m_indices[i + 1] = temp;
                }

                preparedMesh.m_normals = CalculateNormals(verts, indices);
            }
            else if (normals != null && normals.Length > 0)
            {
                preparedMesh.m_normals = normals;
            }
            else
            {
                bool isTree = materialName.StartsWith("tr");
                bool isTerrain = (name[0] == 'L' && !isTree);

                if (isTerrain)
                {
                    Vector3 upECEF = originECEF.normalized.ToSingleVector();
                    preparedMesh.m_normals = CalculateTerrainNormalsAndRemoveDegenerateTriangles(name, verts, ref preparedMesh.m_indices, upECEF);
                }
                else if (materialName.StartsWith("Raster"))
                {
                    preparedMesh.m_normals = CalculateRasterTerrainNormals(verts, indices, originECEF);
                }
                else
                {
                    preparedMesh.m_normals = CalculateNormals(verts, indices);
                }
            }

            return preparedMesh;
        }

        public Mesh ToUnityMesh()
        {
            var unityMesh = new Mesh();
            unityMesh.vertices = m_verts;

            if (m_uvs != null)
            {
                unityMesh.uv = m_uvs;
            }

            if (m_uv2s != null)
            {
                unityMesh.uv2 = m_uv2s;
            }

            if (m_normals != null)
            {
                unityMesh.normals = m_normals;
            }

            if (m_colors != null)
            {
                unityMesh.colors32 = m_colors;
            }

            unityMesh.triangles = m_indices;
            unityMesh.name = m_name;

            //unityMesh.Optimize();

            return unityMesh;
        }

        private static Vector3[] CalculateTerrainNormalsAndRemoveDegenerateTriangles(string meshName, Vector3[] vertices, ref int[] indices, Vector3 ecefUp)
        {
            var normals = new Vector3[vertices.Length];
            const float edgeContribution = 0.001f;
            Vector3 skirtEdgeNormal = ecefUp * edgeContribution;
            float angleLimit = 1.0f - Mathf.Cos(85 * Mathf.Deg2Rad);
            int writeIndex = 0;

            for (int readIndex = 0; readIndex < indices.Length; readIndex += 3)
            {
                var p1 = vertices[indices[readIndex + 0]];
                var p2 = vertices[indices[readIndex + 1]];
                var p3 = vertices[indices[readIndex + 2]];

                var v1 = p2 - p1;
                var v2 = p3 - p1;
                var normal = Vector3.Cross(v1, v2);

                if (normal == Vector3.zero)
                {
                    // degenerate triangle - see MPLY-9034
                    continue;
                }

                var dot = Vector3.Dot(ecefUp, normal.normalized);

                if (Math.Abs(dot) < angleLimit)
                {
                    normal = skirtEdgeNormal;
                }

                indices[writeIndex + 0] = indices[readIndex + 0];
                indices[writeIndex + 1] = indices[readIndex + 1];
                indices[writeIndex + 2] = indices[readIndex + 2];

                normals[indices[writeIndex + 0]] += normal;
                normals[indices[writeIndex + 1]] += normal;
                normals[indices[writeIndex + 2]] += normal;

                writeIndex += 3;
            }

            if (writeIndex < indices.Length)
            {
                var replacement = new int[writeIndex];
                Array.Copy(indices, replacement, writeIndex);
                indices = replacement;
            }

            for (var i = 0; i < normals.Length; ++i)
            {
                normals[i].Normalize();
            }

            return normals;
        }

        private static Vector3[] CalculateNormals(Vector3[] vertices, int[] indices)
        {
            var normals = new Vector3[vertices.Length];
            
            for (var i = 0; i < indices.Length; i += 3)
            {
                var p1 = vertices[indices[i + 0]];
                var p2 = vertices[indices[i + 1]];
                var p3 = vertices[indices[i + 2]];

                var v1 = p2 - p1;
                var v2 = p3 - p1;
                var normal = Vector3.Cross(v1, v2);

                normals[indices[i + 0]] += normal;
                normals[indices[i + 1]] += normal;
                normals[indices[i + 2]] += normal;
            }

            for (var i = 0; i < normals.Length; ++i)
            {
                normals[i].Normalize();
            }

            return normals;
        }

        private static Vector3[] CalculateRasterTerrainNormals(Vector3[] vertices, int[] indices, DoubleVector3 originECEF)
        {
            var normals = new Vector3[vertices.Length];

            for (int i = 0; i < vertices.Length; ++i)
            {
                var normalECEF = (vertices[i] + originECEF).normalized.ToSingleVector();
                normals[i] = normalECEF;
            }

            return normals;
        }
    }
}