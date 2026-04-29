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
        public MeshData(MeshData mesh)
        {
            if (mesh == null)
                throw new System.ArgumentNullException("mesh");

            indexFormat = mesh.indexFormat;
            System.Array.Copy(mesh.vertices, vertices, mesh.vertices.Length);
            triangles = new int[mesh.triangles.Length][];

            for (int i = 0; i < mesh.triangles.Length; i++)
            {
                triangles[i] = new int[mesh.triangles[i].Length];
                System.Array.Copy(mesh.triangles[i], triangles[i], mesh.triangles[i].Length);
            }

            System.Array.Copy(mesh.meshNormals, meshNormals, mesh.meshNormals.Length);
            System.Array.Copy(mesh.meshUV0s, meshUV0s, mesh.meshUV0s.Length);
            System.Array.Copy(mesh.meshUV1s, meshUV1s, mesh.meshUV1s.Length);
            System.Array.Copy(mesh.meshUV2s, meshUV2s, mesh.meshUV2s.Length);
            System.Array.Copy(mesh.meshColors, meshColors, mesh.meshColors.Length);
            System.Array.Copy(mesh.meshTangents, meshTangents, mesh.meshTangents.Length);

            bounds = mesh.bounds;
            name = mesh.name;
            
        }
        /// <summary>Constructs an empty instance.</summary>
        public MeshData()
        {
        }

        public void AppendMesh(MeshData other)
        {
            if (other == null || other.vertices == null || other.vertices.Length == 0)
            {
                return;
            }

            int vertexOffset = this.vertexCount;

            // --- Vertices ---
            if (this.vertices == null)
            {
                this.vertices = (Vector3[])other.vertices.Clone();
            }
            else
            {
                Vector3[] newVerts = new Vector3[this.vertices.Length + other.vertices.Length];
                System.Array.Copy(this.vertices, 0, newVerts, 0, this.vertices.Length);
                System.Array.Copy(other.vertices, 0, newVerts, this.vertices.Length, other.vertices.Length);
                this.vertices = newVerts;
            }

            // --- Normals ---
            if (other.meshNormals != null)
            {
                if (this.meshNormals == null)
                {
                    this.meshNormals = (Vector3[])other.meshNormals.Clone();
                }
                else
                {
                    Vector3[] arr = new Vector3[this.meshNormals.Length + other.meshNormals.Length];
                    System.Array.Copy(this.meshNormals, 0, arr, 0, this.meshNormals.Length);
                    System.Array.Copy(other.meshNormals, 0, arr, this.meshNormals.Length, other.meshNormals.Length);
                    this.meshNormals = arr;
                }
            }

            // --- UV0 ---
            if (other.meshUV0s != null)
            {
                if (this.meshUV0s == null)
                {
                    this.meshUV0s = (Vector2[])other.meshUV0s.Clone();
                }
                else
                {
                    Vector2[] arr = new Vector2[this.meshUV0s.Length + other.meshUV0s.Length];
                    System.Array.Copy(this.meshUV0s, 0, arr, 0, this.meshUV0s.Length);
                    System.Array.Copy(other.meshUV0s, 0, arr, this.meshUV0s.Length, other.meshUV0s.Length);
                    this.meshUV0s = arr;
                }
            }

            // --- UV1 ---
            if (other.meshUV1s != null)
            {
                if (this.meshUV1s == null)
                {
                    this.meshUV1s = (Vector2[])other.meshUV1s.Clone();
                }
                else
                {
                    Vector2[] arr = new Vector2[this.meshUV1s.Length + other.meshUV1s.Length];
                    System.Array.Copy(this.meshUV1s, 0, arr, 0, this.meshUV1s.Length);
                    System.Array.Copy(other.meshUV1s, 0, arr, this.meshUV1s.Length, other.meshUV1s.Length);
                    this.meshUV1s = arr;
                }
            }

            // --- UV2 ---
            if (other.meshUV2s != null)
            {
                if (this.meshUV2s == null)
                {
                    this.meshUV2s = (Vector2[])other.meshUV2s.Clone();
                }
                else
                {
                    Vector2[] arr = new Vector2[this.meshUV2s.Length + other.meshUV2s.Length];
                    System.Array.Copy(this.meshUV2s, 0, arr, 0, this.meshUV2s.Length);
                    System.Array.Copy(other.meshUV2s, 0, arr, this.meshUV2s.Length, other.meshUV2s.Length);
                    this.meshUV2s = arr;
                }
            }

            // --- Colors ---
            if (other.meshColors != null)
            {
                if (this.meshColors == null)
                {
                    this.meshColors = (Color[])other.meshColors.Clone();
                }
                else
                {
                    Color[] arr = new Color[this.meshColors.Length + other.meshColors.Length];
                    System.Array.Copy(this.meshColors, 0, arr, 0, this.meshColors.Length);
                    System.Array.Copy(other.meshColors, 0, arr, this.meshColors.Length, other.meshColors.Length);
                    this.meshColors = arr;
                }
            }

            // --- Tangents ---
            if (other.meshTangents != null)
            {
                if (this.meshTangents == null)
                {
                    this.meshTangents = (Vector4[])other.meshTangents.Clone();
                }
                else
                {
                    Vector4[] arr = new Vector4[this.meshTangents.Length + other.meshTangents.Length];
                    System.Array.Copy(this.meshTangents, 0, arr, 0, this.meshTangents.Length);
                    System.Array.Copy(other.meshTangents, 0, arr, this.meshTangents.Length, other.meshTangents.Length);
                    this.meshTangents = arr;
                }
            }

            // --- Triangles (per-submesh, vertex offset) ---
            if (other.triangles != null)
            {
                if (this.triangles == null)
                {
                    this.triangles = new int[other.triangles.Length][];
                }
                else
                {
                    int[][] newTris = new int[this.triangles.Length + other.triangles.Length][];
                    System.Array.Copy(this.triangles, newTris, this.triangles.Length);
                    this.triangles = newTris;
                }

                int baseIndex = this.triangles.Length - other.triangles.Length;

                for (int s = 0; s < other.triangles.Length; s++)
                {
                    int[] src = other.triangles[s];

                    if (src == null)
                    {
                        this.triangles[baseIndex + s] = null;
                        continue;
                    }

                    int[] dst = new int[src.Length];

                    for (int i = 0; i < src.Length; i++)
                    {
                        dst[i] = src[i] + vertexOffset;
                    }

                    this.triangles[baseIndex + s] = dst;
                }
            }

            // --- Bounds ---
            this.bounds.Encapsulate(other.bounds);
        }

        public static MeshData MergeMeshes(System.Collections.Generic.List<MeshData> meshes)
        {
            if (meshes == null || meshes.Count == 0)
            {
                return new MeshData();
            }

            MeshData result = new MeshData();

            var vertList = new System.Collections.Generic.List<Vector3>();

            var normalsList = new System.Collections.Generic.List<Vector3>();
            var uv0List = new System.Collections.Generic.List<Vector2>();
            var uv1List = new System.Collections.Generic.List<Vector2>();
            var uv2List = new System.Collections.Generic.List<Vector2>();
            var colorList = new System.Collections.Generic.List<Color>();
            var tangentList = new System.Collections.Generic.List<Vector4>();

            bool hasNormals = true;
            bool hasUV0 = true;
            bool hasUV1 = true;
            bool hasUV2 = true;
            bool hasColors = true;
            bool hasTangents = true;

            // determine max submesh count
            int maxSubMeshCount = 0;
            for (int m = 0; m < meshes.Count; m++)
            {
                MeshData md = meshes[m];
                if (md == null) continue;

                if (md.triangles != null && md.triangles.Length > maxSubMeshCount)
                {
                    maxSubMeshCount = md.triangles.Length;
                }
            }

            var submeshLists = new System.Collections.Generic.List<int>[maxSubMeshCount];
            for (int i = 0; i < maxSubMeshCount; i++)
            {
                submeshLists[i] = new System.Collections.Generic.List<int>();
            }

            int vertexOffset = 0;

            for (int m = 0; m < meshes.Count; m++)
            {
                MeshData md = meshes[m];
                if (md == null || md.vertices == null || md.vertices.Length == 0)
                {
                    continue;
                }

                // --- vertices ---
                vertList.AddRange(md.vertices);

                // --- optional channels presence tracking ---
                if (md.meshNormals == null) hasNormals = false;
                if (md.meshUV0s == null) hasUV0 = false;
                if (md.meshUV1s == null) hasUV1 = false;
                if (md.meshUV2s == null) hasUV2 = false;
                if (md.meshColors == null) hasColors = false;
                if (md.meshTangents == null) hasTangents = false;

                // --- triangles ---
                if (md.triangles != null)
                {
                    for (int s = 0; s < md.triangles.Length; s++)
                    {
                        int[] tris = md.triangles[s];
                        if (tris == null) continue;

                        var dst = submeshLists[s];

                        for (int i = 0; i < tris.Length; i++)
                        {
                            dst.Add(tris[i] + vertexOffset);
                        }
                    }
                }

                // --- attributes (only append now, filter later if needed) ---
                if (md.meshNormals != null) normalsList.AddRange(md.meshNormals);
                if (md.meshUV0s != null) uv0List.AddRange(md.meshUV0s);
                if (md.meshUV1s != null) uv1List.AddRange(md.meshUV1s);
                if (md.meshUV2s != null) uv2List.AddRange(md.meshUV2s);
                if (md.meshColors != null) colorList.AddRange(md.meshColors);
                if (md.meshTangents != null) tangentList.AddRange(md.meshTangents);

                vertexOffset += md.vertices.Length;
            }// end loop meshes

            // --- assign vertices ---
            result.vertices = vertList.ToArray();

            // --- assign triangles ---
            result.triangles = new int[maxSubMeshCount][];
            for (int i = 0; i < maxSubMeshCount; i++)
            {
                result.triangles[i] = submeshLists[i].Count > 0 ? submeshLists[i].ToArray() : null;
            }

            // --- assign attributes ONLY if all meshes had them ---
            if (hasNormals && normalsList.Count == vertList.Count)
                result.meshNormals = normalsList.ToArray();

            if (hasUV0 && uv0List.Count == vertList.Count)
                result.meshUV0s = uv0List.ToArray();

            if (hasUV1 && uv1List.Count == vertList.Count)
                result.meshUV1s = uv1List.ToArray();

            if (hasUV2 && uv2List.Count == vertList.Count)
                result.meshUV2s = uv2List.ToArray();

            if (hasColors && colorList.Count == vertList.Count)
                result.meshColors = colorList.ToArray();

            if (hasTangents && tangentList.Count == vertList.Count)
                result.meshTangents = tangentList.ToArray();

            // --- bounds ---
            bool first = true;
            for (int i = 0; i < meshes.Count; i++)
            {
                MeshData md = meshes[i];
                if (md == null) continue;

                if (first)
                {
                    result.bounds = md.bounds;
                    first = false;
                }
                else
                {
                    result.bounds.Encapsulate(md.bounds);
                }
            }

            return result;
        }

    }


}