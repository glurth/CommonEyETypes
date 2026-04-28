using UnityEngine;
using EyE.UnityAssetTypes;
using UnityEngine.Rendering;

namespace EyE.Geometry
{
    /// <summary>
    /// Thread-safe container for mesh data. Can be created and modified off the main Unity thread.
    /// Only ToMesh() touches UnityEngine.Mesh and must be called on the main thread.
    /// </summary>
    public class MeshData
    {
        /// <summary>Index format to use for mesh creation.</summary>
        public IndexFormat indexFormat = IndexFormat.UInt16;

        /// <summary>Vertex positions.</summary>
        public Vector3[] vertices;

        /// <summary>Triangle indices.</summary>
        public int[][] triangles;

        /// <summary>Vertex normals. Optional.</summary>
        public Vector3[] meshNormals;

        /// <summary>UV channel 0. Optional.</summary>
        public Vector2[] meshUV0s;

        /// <summary>UV channel 1. Optional.</summary>
        public Vector2[] meshUV1s;

        /// <summary>UV channel 2. Optional.</summary>
        public Vector2[] meshUV2s;

        /// <summary>Vertex colors. Optional.</summary>
        public Color[] meshColors;

        /// <summary>Per-vertex tangents. Optional.</summary>
        public Vector4[] meshTangents;

        /// <summary>Axis-aligned bounding box for the mesh.</summary>
        public Bounds bounds;

        /// <summary>Optional link for existing systems needing to track the resulting mesh.</summary>
        public FacesAndNeighbors facesAndNeighborsRef;

        /// <summary>Name for the mesh.</summary>
        public string name;

        /// <summary>Number of vertices stored.</summary>
        public int vertexCount
        {
            get
            {
                if (vertices == null)
                    return 0;
                return vertices.Length;
            }
        }
        #region SetXXX functions array params
        /// <summary>Sets vertex positions.</summary>
        public void SetVertices(Vector3[] verts)
        {
            vertices = verts;
        }

        /// <summary>Sets triangle indices.</summary>
        public void SetTriangles(int[] tris,int submeshIndex=0)
        {
           // triangles = tris;

            if (triangles == null || submeshIndex >= triangles.Length)
            {
                int newSize = submeshIndex + 1;
                int[][] newArray = new int[newSize][];

                if (triangles != null)
                {
                    for (int i = 0; i < triangles.Length; i++)
                        newArray[i] = triangles[i];
                }

                triangles = newArray;
            }

            triangles[submeshIndex] = tris;
        }

        /// <summary>Sets normals. Length must match vertices.</summary>
        public void SetNormals(Vector3[] normals)
        {
            meshNormals = normals;
        }

        /// <summary>Sets UV data for a given channel. Channel must be 0, 1, or 2.</summary>
        public void SetUVs(int channel, Vector2[] uvs)
        {
            if (channel == 0)
                meshUV0s = uvs;
            else if (channel == 1)
                meshUV1s = uvs;
            else if (channel == 2)
                meshUV2s = uvs;
        }
        /// <summary>Sets vertex colors.</summary>
        public void SetColors(Color[] colors)
        {
            meshColors = colors;
        }

        /// <summary>Sets tangents.</summary>
        public void SetTangents(Vector4[] tangents)
        {
            meshTangents = tangents;
        }
        #endregion
        #region SetXXX functions list params
        /// <summary>Sets vertex positions from a list.</summary>
        public void SetVertices(System.Collections.Generic.List<Vector3> verts)
        {
            if (verts == null)
            {
                vertices = null;
                return;
            }
            vertices = verts.ToArray();
        }

        /// <summary>Sets triangle indices from a list.</summary>
        public void SetTriangles(System.Collections.Generic.List<int> tris, int submeshIndex = 0)
        {
            if (tris == null)
            {
                SetTriangles((int[])null, submeshIndex);
                //triangles = null;
                return;
            }

            SetTriangles(tris.ToArray(),submeshIndex);
            //triangles = tris;
        }

        /// <summary>Sets normals from a list.</summary>
        public void SetNormals(System.Collections.Generic.List<Vector3> normals)
        {
            if (normals == null)
            {
                meshNormals = null;
                return;
            }
            meshNormals = normals.ToArray();
        }

        /// <summary>Sets UV data for a given channel from a list.</summary>
        public void SetUVs(int channel, System.Collections.Generic.List<Vector2> uvs)
        {
            if (channel == 0)
                meshUV0s = uvs != null ? uvs.ToArray() : null;
            else if (channel == 1)
                meshUV1s = uvs != null ? uvs.ToArray() : null;
            else if (channel == 2)
                meshUV2s = uvs != null ? uvs.ToArray() : null;
        }

        /// <summary>Sets vertex colors from a list.</summary>
        public void SetColors(System.Collections.Generic.List<Color> colors)
        {
            if (colors == null)
            {
                meshColors = null;
                return;
            }
            meshColors = colors.ToArray();
        }

        /// <summary>Sets tangents from a list.</summary>
        public void SetTangents(System.Collections.Generic.List<Vector4> tangents)
        {
            if (tangents == null)
            {
                meshTangents = null;
                return;
            }
            meshTangents = tangents.ToArray();
        }

        #endregion

        /// <summary>Sets bounds. You must recompute or manually provide correct value.</summary>
        public void SetBounds(Bounds b)
        {
            bounds = b;
        }

        /// <summary>
        /// Normalizes all vertex positions so the mesh fits within a unit cube [0,1] in each axis,
        /// based on the current <see cref="Bounds"/>.
        /// </summary>
        /// <remarks>
        /// Each vertex is remapped from its original position into normalized space using:
        /// (value - bounds.min) / bounds.size.
        /// <para>
        /// After execution:
        /// - All vertex positions lie within the range [0,1] on each axis (unless the original bounds had zero size on that axis).
        /// - <see cref="Bounds"/> is updated to a unit cube centered at (0.5, 0.5, 0.5) with size (1,1,1).
        /// </para>
        /// <para>
        /// If any axis of the original bounds has zero size, that axis is collapsed to 0 for all vertices
        /// to avoid division by zero.
        /// </para>
        /// <para>
        /// This operation is affine (scale + translation) and does not modify topology, indices,
        /// normals, or other vertex attributes.
        /// </para>
        /// </remarks>
        public void Normalize()
        {
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            Vector3 size = max - min;

            float invX = 0f;
            float invY = 0f;
            float invZ = 0f;

            if (size.x != 0f) { invX = 1f / size.x; }
            if (size.y != 0f) { invY = 1f / size.y; }
            if (size.z != 0f) { invZ = 1f / size.z; }

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 v = vertices[i];

                float x = (v.x - min.x) * invX;
                float y = (v.y - min.y) * invY;
                float z = (v.z - min.z) * invZ;

                vertices[i] = new Vector3(x, y, z);
            }

            bounds = new Bounds(new Vector3(0.5f, 0.5f, 0.5f), Vector3.one);
        }

        /// <summary>
        /// Builds a Unity Mesh using the currently stored data.
        /// Must be called on the main Unity thread.
        /// </summary>
        /// <returns>Created Unity Mesh.</returns>
        public Mesh ToMesh()
        {
            Mesh newMesh = new Mesh();

            if (vertices != null && vertices.Length >= 0xFFFF)
                newMesh.indexFormat = IndexFormat.UInt32;
            else
                newMesh.indexFormat = indexFormat;

            if (vertices != null)
                newMesh.SetVertices(vertices);

            //if (triangles != null)
            //    newMesh.SetTriangles(triangles, 0);
            if (triangles != null)
            {
                int validCount = 0;
                for (int i = 0; i < triangles.Length; i++)
                {
                    if (triangles[i] != null && triangles[i].Length > 0)
                        validCount++;
                }

                newMesh.subMeshCount = validCount;

                int dst = 0;
                for (int i = 0; i < triangles.Length; i++)
                {
                    int[] tris = triangles[i];
                    if (tris == null || tris.Length == 0)
                        continue;

                    newMesh.SetTriangles(tris, dst);
                    dst++;
                }

            }


            if (meshNormals != null && meshNormals.Length == (vertices != null ? vertices.Length : 0))
                newMesh.SetNormals(meshNormals);

            if (meshUV0s != null)
                newMesh.SetUVs(0, meshUV0s);

            if (meshUV1s != null)
                newMesh.SetUVs(1, meshUV1s);

            if (meshUV2s != null)
                newMesh.SetUVs(2, meshUV2s);

            if (meshColors != null)
                newMesh.SetColors(meshColors);

            if (meshTangents != null)
                newMesh.SetTangents(meshTangents);

            newMesh.bounds = bounds;
            newMesh.name = name;

            if (facesAndNeighborsRef != null)
                facesAndNeighborsRef.meshRef = newMesh;

            return newMesh;
        }

        /// <summary>
        /// Recalculate bounding box from vertices only. Safe off-thread.
        /// </summary>
        public void RecalculateBounds()
        {
            if (vertices == null || vertices.Length == 0)
                return;

            Vector3 min = vertices[0];
            Vector3 max = vertices[0];

            for (int i = 1; i < vertices.Length; i++)
            {
                Vector3 v = vertices[i];
                if (v.x < min.x) min.x = v.x;
                if (v.y < min.y) min.y = v.y;
                if (v.z < min.z) min.z = v.z;
                if (v.x > max.x) max.x = v.x;
                if (v.y > max.y) max.y = v.y;
                if (v.z > max.z) max.z = v.z;
            }

            bounds = new Bounds((min + max) * 0.5f, max - min);
        }

        /// <summary>
        /// Recalculates vertex normals using the triangle list and vertex positions.
        /// Safe to run off the Unity main thread.
        /// </summary>
        public void RecalculateNormals()
        {
            if (vertices == null || triangles == null)
                return;

            if (meshNormals == null || meshNormals.Length != vertices.Length)
                meshNormals = new Vector3[vertices.Length];

            // Zero normals
            for (int i = 0; i < meshNormals.Length; i++)
            {
                meshNormals[i] = Vector3.zero;
            }

            // Accumulate face normals
            for (int s = 0; s < triangles.Length; s++)
            {
                int[] tris = triangles[s];
                if (tris == null) continue;

                for (int i = 0; i < tris.Length; i += 3)
                {
                    int i0 = tris[i];
                    int i1 = tris[i + 1];
                    int i2 = tris[i + 2];

                    Vector3 v0 = vertices[i0];
                    Vector3 v1 = vertices[i1];
                    Vector3 v2 = vertices[i2];

                    Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0);

                    meshNormals[i0] += normal;
                    meshNormals[i1] += normal;
                    meshNormals[i2] += normal;
                }
            }
            /*for (int i = 0; i < triangles.Length; i += 3)
            {
                int i0 = triangles[i];
                int i1 = triangles[i + 1];
                int i2 = triangles[i + 2];

                Vector3 v0 = vertices[i0];
                Vector3 v1 = vertices[i1];
                Vector3 v2 = vertices[i2];

                Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0);

                meshNormals[i0] += normal;
                meshNormals[i1] += normal;
                meshNormals[i2] += normal;
            }*/

            // Normalize accumulated normals
            for (int i = 0; i < meshNormals.Length; i++)
            {
                meshNormals[i] = meshNormals[i].normalized;
            }
        }


        /// <summary>
        /// Constructor that copies data from a Unity Mesh.
        /// Only legal to call on the main thread.
        /// </summary>
        /// <param name="mesh">Source mesh.</param>
        public MeshData(Mesh mesh)
        {
            if (mesh == null)
                throw new System.ArgumentNullException("mesh");

            indexFormat = mesh.indexFormat;
            vertices = mesh.vertices;
            int subMeshCount = mesh.subMeshCount;
            triangles = new int[subMeshCount][];

            for (int i = 0; i < subMeshCount; i++)
            {
                triangles[i] = mesh.GetTriangles(i);
            }
            //triangles = mesh.triangles;
            meshNormals = mesh.normals;
            meshUV0s = mesh.uv;
            meshUV1s = mesh.uv2;
            meshUV2s = mesh.uv3;
            meshColors = mesh.colors;
            meshTangents = mesh.tangents;
            bounds = mesh.bounds;
            name = mesh.name;
            RecalculateBounds();
        }

        /// <summary>Constructs an empty instance.</summary>
        public MeshData()
        {
        }
    }


}