using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Mana.Graphics;
using Mana.Graphics.Geometry;
using Mana.Graphics.Vertex.Types;

namespace Mana.Utilities
{
    public static class MeshGenerator
    {
        public static VertexPositionNormal[] GenerateIcoSphere(Vector3 position, float radius, int steps = 1)
        {
            return IcoSphere.Create(position, radius, steps)
                            .Select(x => (VertexPositionNormal)x)
                            .ToArray();
        }

        public static VertexPositionNormal[] GenerateCubeData(Vector3 position, float size)
        {
            return GenerateCubeData(position, size, size, size);
        }

        public static Mesh CreateMesh(RenderContext renderContext, VertexPositionNormal[] data, Matrix4x4 transform)
        {
            var vertices = data.Select(x => new VertexPositionNormalTexture(x.Position, x.Normal, Vector2.Zero))
                               .ToArray();
            
            var indices = Enumerable.Range(0, vertices.Length)
                                    .Select(x => (uint)x)
                                    .ToArray();

            return new Mesh(renderContext, vertices, indices, null, transform);
        }

        public static VertexPositionNormal[] GenerateSkyboxCube()
        {
            var positions = new[]{
                -1.0f,  1.0f, -1.0f,
                -1.0f, -1.0f, -1.0f,
                1.0f, -1.0f, -1.0f,
                1.0f, -1.0f, -1.0f,
                1.0f,  1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f,

                -1.0f, -1.0f,  1.0f,
                -1.0f, -1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f,
                -1.0f,  1.0f,  1.0f,
                -1.0f, -1.0f,  1.0f,

                1.0f, -1.0f, -1.0f,
                1.0f, -1.0f,  1.0f,
                1.0f,  1.0f,  1.0f,
                1.0f,  1.0f,  1.0f,
                1.0f,  1.0f, -1.0f,
                1.0f, -1.0f, -1.0f,

                -1.0f, -1.0f,  1.0f,
                -1.0f,  1.0f,  1.0f,
                1.0f,  1.0f,  1.0f,
                1.0f,  1.0f,  1.0f,
                1.0f, -1.0f,  1.0f,
                -1.0f, -1.0f,  1.0f,

                -1.0f,  1.0f, -1.0f,
                1.0f,  1.0f, -1.0f,
                1.0f,  1.0f,  1.0f,
                1.0f,  1.0f,  1.0f,
                -1.0f,  1.0f,  1.0f,
                -1.0f,  1.0f, -1.0f,

                -1.0f, -1.0f, -1.0f,
                -1.0f, -1.0f,  1.0f,
                1.0f, -1.0f, -1.0f,
                1.0f, -1.0f, -1.0f,
                -1.0f, -1.0f,  1.0f,
                1.0f, -1.0f,  1.0f
            };

            VertexPositionNormal[] vertices = new VertexPositionNormal[36];

            for (int i = 0; i < 108; i += 3)
            {
                vertices[i / 3] = new VertexPositionNormal(new Vector3(positions[i], positions[i + 1], positions[i + 2]), Vector3.Zero);
            }

            return vertices;
        }

        public static VertexPositionNormal[] GenerateCubeData(Vector3 position, float length, float width, float height)
        {
            #region Vertices

            Vector3 p0 = new Vector3(-length * .5f, -width * .5f, height * .5f);
            Vector3 p1 = new Vector3(length * .5f, -width * .5f, height * .5f);
            Vector3 p2 = new Vector3(length * .5f, -width * .5f, -height * .5f);
            Vector3 p3 = new Vector3(-length * .5f, -width * .5f, -height * .5f);

            Vector3 p4 = new Vector3(-length * .5f, width * .5f, height * .5f);
            Vector3 p5 = new Vector3(length * .5f, width * .5f, height * .5f);
            Vector3 p6 = new Vector3(length * .5f, width * .5f, -height * .5f);
            Vector3 p7 = new Vector3(-length * .5f, width * .5f, -height * .5f);

            Vector3[] vertices = new Vector3[]
            {
	            // Bottom
	            p0, p1, p2, p3,

	            // Left
	            p7, p4, p0, p3,

	            // Front
	            p4, p5, p1, p0,

	            // Back
	            p6, p7, p3, p2,

	            // Right
	            p5, p6, p2, p1,

	            // Top
	            p7, p6, p5, p4
            };

            #endregion

            #region Normals

            Vector3 up = Vector3Helper.Up;
            Vector3 down = Vector3Helper.Down;
            Vector3 front = Vector3Helper.Forward;
            Vector3 back = Vector3Helper.Backward;
            Vector3 left = Vector3Helper.Left;
            Vector3 right = Vector3Helper.Right;

            Vector3[] normals = new Vector3[]
            {
	            // Bottom
	            down, down, down, down,

	            // Left
	            left, left, left, left,

	            // Front
	            front, front, front, front,

	            // Back
	            back, back, back, back,

	            // Right
	            right, right, right, right,

	            // Top
	            up, up, up, up
            };
            #endregion

            #region UVs
            Vector2 _00 = new Vector2(0f, 0f);
            Vector2 _10 = new Vector2(1f, 0f);
            Vector2 _01 = new Vector2(0f, 1f);
            Vector2 _11 = new Vector2(1f, 1f);

            Vector2[] uvs = new Vector2[]
            {
	            // Bottom
	            _11, _01, _00, _10,

	            // Left
	            _11, _01, _00, _10,

	            // Front
	            _11, _01, _00, _10,

	            // Back
	            _11, _01, _00, _10,

	            // Right
	            _11, _01, _00, _10,

	            // Top
	            _11, _01, _00, _10,
            };
            #endregion

            #region Indices
            int[] triangles = new int[]
            {
	            // Bottom
	            3, 1, 0,
                3, 2, 1,

	            // Left
	            3 + 4 * 1, 1 + 4 * 1, 0 + 4 * 1,
                3 + 4 * 1, 2 + 4 * 1, 1 + 4 * 1,

	            // Front
	            3 + 4 * 2, 1 + 4 * 2, 0 + 4 * 2,
                3 + 4 * 2, 2 + 4 * 2, 1 + 4 * 2,

	            // Back
	            3 + 4 * 3, 1 + 4 * 3, 0 + 4 * 3,
                3 + 4 * 3, 2 + 4 * 3, 1 + 4 * 3,

	            // Right
	            3 + 4 * 4, 1 + 4 * 4, 0 + 4 * 4,
                3 + 4 * 4, 2 + 4 * 4, 1 + 4 * 4,

	            // Top
	            3 + 4 * 5, 1 + 4 * 5, 0 + 4 * 5,
                3 + 4 * 5, 2 + 4 * 5, 1 + 4 * 5,

            };
            #endregion

            VertexPositionNormalTexture[] output = new VertexPositionNormalTexture[triangles.Length];

            for (int i = 0; i < triangles.Length; i++)
            {
                output[i] = new VertexPositionNormalTexture(vertices[triangles[i]],
                                                            normals[triangles[i]],
                                                            uvs[triangles[i]]);
            }

            return output.Select(x => (VertexPositionNormal)x)
                         .ToArray();
        }

        public static VertexPositionNormal[] GenerateCone(float height, float bottomRadius, float topRadius, int numSides)
        {
            int nbHeightSeg = 1; // Not implemented yet
            int nbVerticesCap = numSides + 1;

            #region Vertices

            // bottom + top + sides
            Vector3[] vertices = new Vector3[nbVerticesCap + nbVerticesCap + numSides * nbHeightSeg * 2 + 2];
            int vert = 0;
            float _2pi = (float)Math.PI * 2f;

            // Bottom cap
            vertices[vert++] = new Vector3(0f, 0f, 0f);
            while (vert <= numSides)
            {
                float rad = (float)vert / numSides * _2pi;
                vertices[vert] = new Vector3((float)Math.Cos(rad) * bottomRadius, 0f, (float)Math.Sin(rad) * bottomRadius);
                vert++;
            }

            // Top cap
            vertices[vert++] = new Vector3(0f, height, 0f);
            while (vert <= numSides * 2 + 1)
            {
                float rad = (float)(vert - numSides - 1) / numSides * _2pi;
                vertices[vert] = new Vector3((float)Math.Cos(rad) * topRadius, height, (float)Math.Sin(rad) * topRadius);
                vert++;
            }

            // Sides
            int v = 0;
            while (vert <= vertices.Length - 4)
            {
                float rad = (float)v / numSides * _2pi;
                vertices[vert] = new Vector3((float)Math.Cos(rad) * topRadius, height, (float)Math.Sin(rad) * topRadius);
                vertices[vert + 1] = new Vector3((float)Math.Cos(rad) * bottomRadius, 0, (float)Math.Sin(rad) * bottomRadius);
                vert += 2;
                v++;
            }
            vertices[vert] = vertices[numSides * 2 + 2];
            vertices[vert + 1] = vertices[numSides * 2 + 3];
            #endregion

            #region Normals

            // bottom + top + sides
            Vector3[] normals = new Vector3[vertices.Length];
            vert = 0;

            // Bottom cap
            while (vert <= numSides)
            {
                normals[vert++] = Vector3Helper.Down;
            }

            // Top cap
            while (vert <= numSides * 2 + 1)
            {
                normals[vert++] = Vector3Helper.Up;
            }

            // Sides
            v = 0;
            while (vert <= vertices.Length - 4)
            {
                float rad = (float)v / numSides * _2pi;
                float cos = (float)Math.Cos(rad);
                float sin = (float)Math.Sin(rad);

                normals[vert] = new Vector3(cos, 0f, sin);
                normals[vert + 1] = normals[vert];

                vert += 2;
                v++;
            }
            normals[vert] = normals[numSides * 2 + 2];
            normals[vert + 1] = normals[numSides * 2 + 3];
            #endregion

            #region UVs
            Vector2[] uvs = new Vector2[vertices.Length];

            // Bottom cap
            int u = 0;
            uvs[u++] = new Vector2(0.5f, 0.5f);
            while (u <= numSides)
            {
                float rad = (float)u / numSides * _2pi;
                uvs[u] = new Vector2((float)Math.Cos(rad) * .5f + .5f, (float)Math.Sin(rad) * .5f + .5f);
                u++;
            }

            // Top cap
            uvs[u++] = new Vector2(0.5f, 0.5f);
            while (u <= numSides * 2 + 1)
            {
                float rad = (float)u / numSides * _2pi;
                uvs[u] = new Vector2((float)Math.Cos(rad) * .5f + .5f, (float)Math.Sin(rad) * .5f + .5f);
                u++;
            }

            // Sides
            int u_sides = 0;
            while (u <= uvs.Length - 4)
            {
                float t = (float)u_sides / numSides;
                uvs[u] = new Vector2(t, 1f);
                uvs[u + 1] = new Vector2(t, 0f);
                u += 2;
                u_sides++;
            }
            uvs[u] = new Vector2(1f, 1f);
            uvs[u + 1] = new Vector2(1f, 0f);
            #endregion

            #region Indices
            int nbTriangles = numSides + numSides + numSides * 2;
            int[] indices = new int[nbTriangles * 3 + 3];

            // Bottom cap
            int tri = 0;
            int i = 0;
            while (tri < numSides - 1)
            {
                indices[i] = 0;
                indices[i + 1] = tri + 1;
                indices[i + 2] = tri + 2;
                tri++;
                i += 3;
            }
            indices[i] = 0;
            indices[i + 1] = tri + 1;
            indices[i + 2] = 1;
            tri++;
            i += 3;

            // Top cap
            //tri++;
            while (tri < numSides * 2)
            {
                indices[i] = tri + 2;
                indices[i + 1] = tri + 1;
                indices[i + 2] = nbVerticesCap;
                tri++;
                i += 3;
            }

            indices[i] = nbVerticesCap + 1;
            indices[i + 1] = tri + 1;
            indices[i + 2] = nbVerticesCap;
            tri++;
            i += 3;
            tri++;

            // Sides
            while (tri <= nbTriangles)
            {
                indices[i] = tri + 2;
                indices[i + 1] = tri + 1;
                indices[i + 2] = tri + 0;
                tri++;
                i += 3;

                indices[i] = tri + 1;
                indices[i + 1] = tri + 2;
                indices[i + 2] = tri + 0;
                tri++;
                i += 3;
            }
            #endregion

            var verticesOut = new VertexPositionNormal[indices.Length];

            for (int j = 0; j < indices.Length; j++)
            {
                var index = indices[j];
                verticesOut[j] = new VertexPositionNormal
                {
                    Position = vertices[index],
                    Normal = normals[index],
                    //TexCoord = uvs[index]
                };
            }

            return verticesOut;
        }
    }

    internal static class IcoSphere
    {
        private struct TriangleIndices
        {
            public int v1;
            public int v2;
            public int v3;

            public TriangleIndices(int v1, int v2, int v3)
            {
                this.v1 = v1;
                this.v2 = v2;
                this.v3 = v3;
            }
        }

        // return index of point in the middle of p1 and p2
        private static int GetMiddlePoint(int p1, int p2, ref List<Vector3> vertices, ref Dictionary<long, int> cache, float radius)
        {
            // first check if we have it already
            bool firstIsSmaller = p1 < p2;
            long smallerIndex = firstIsSmaller ? p1 : p2;
            long greaterIndex = firstIsSmaller ? p2 : p1;
            long key = (smallerIndex << 32) + greaterIndex;

            if (cache.TryGetValue(key, out int ret))
            {
                return ret;
            }

            // not in cache, calculate it
            Vector3 point1 = vertices[p1];
            Vector3 point2 = vertices[p2];
            Vector3 middle = new Vector3
            (
                (point1.X + point2.X) / 2f,
                (point1.Y + point2.Y) / 2f,
                (point1.Z + point2.Z) / 2f
            );

            // add vertex makes sure point is on unit sphere
            int i = vertices.Count;
            vertices.Add(middle.Normalized() * radius);

            // store it, return index
            cache.Add(key, i);

            return i;
        }

        public static VertexPositionNormalTexture[] Create(Vector3 position, float radius, int recursionLevel = 1)
        {
            List<Vector3> vertList = new List<Vector3>();
            Dictionary<long, int> middlePointIndexCache = new Dictionary<long, int>();

            // create 12 vertices of a icosahedron
            float t = (1f + MathF.Sqrt(5f)) / 2f;

            vertList.Add(new Vector3(-1f, t, 0f).Normalized() * radius);
            vertList.Add(new Vector3(1f, t, 0f).Normalized() * radius);
            vertList.Add(new Vector3(-1f, -t, 0f).Normalized() * radius);
            vertList.Add(new Vector3(1f, -t, 0f).Normalized() * radius);

            vertList.Add(new Vector3(0f, -1f, t).Normalized() * radius);
            vertList.Add(new Vector3(0f, 1f, t).Normalized() * radius);
            vertList.Add(new Vector3(0f, -1f, -t).Normalized() * radius);
            vertList.Add(new Vector3(0f, 1f, -t).Normalized() * radius);

            vertList.Add(new Vector3(t, 0f, -1f).Normalized() * radius);
            vertList.Add(new Vector3(t, 0f, 1f).Normalized() * radius);
            vertList.Add(new Vector3(-t, 0f, -1f).Normalized() * radius);
            vertList.Add(new Vector3(-t, 0f, 1f).Normalized() * radius);


            // create 20 triangles of the icosahedron
            List<TriangleIndices> faces = new List<TriangleIndices>();

            // 5 faces around point 0
            faces.Add(new TriangleIndices(0, 11, 5));
            faces.Add(new TriangleIndices(0, 5, 1));
            faces.Add(new TriangleIndices(0, 1, 7));
            faces.Add(new TriangleIndices(0, 7, 10));
            faces.Add(new TriangleIndices(0, 10, 11));

            // 5 adjacent faces
            faces.Add(new TriangleIndices(1, 5, 9));
            faces.Add(new TriangleIndices(5, 11, 4));
            faces.Add(new TriangleIndices(11, 10, 2));
            faces.Add(new TriangleIndices(10, 7, 6));
            faces.Add(new TriangleIndices(7, 1, 8));

            // 5 faces around point 3
            faces.Add(new TriangleIndices(3, 9, 4));
            faces.Add(new TriangleIndices(3, 4, 2));
            faces.Add(new TriangleIndices(3, 2, 6));
            faces.Add(new TriangleIndices(3, 6, 8));
            faces.Add(new TriangleIndices(3, 8, 9));

            // 5 adjacent faces
            faces.Add(new TriangleIndices(4, 9, 5));
            faces.Add(new TriangleIndices(2, 4, 11));
            faces.Add(new TriangleIndices(6, 2, 10));
            faces.Add(new TriangleIndices(8, 6, 7));
            faces.Add(new TriangleIndices(9, 8, 1));


            // refine triangles
            for (int i = 0; i < recursionLevel; i++)
            {
                List<TriangleIndices> faces2 = new List<TriangleIndices>();
                foreach (var tri in faces)
                {
                    // replace triangle by 4 triangles
                    int a = GetMiddlePoint(tri.v1, tri.v2, ref vertList, ref middlePointIndexCache, radius);
                    int b = GetMiddlePoint(tri.v2, tri.v3, ref vertList, ref middlePointIndexCache, radius);
                    int c = GetMiddlePoint(tri.v3, tri.v1, ref vertList, ref middlePointIndexCache, radius);

                    faces2.Add(new TriangleIndices(tri.v1, a, c));
                    faces2.Add(new TriangleIndices(tri.v2, b, a));
                    faces2.Add(new TriangleIndices(tri.v3, c, b));
                    faces2.Add(new TriangleIndices(a, b, c));
                }
                faces = faces2;
            }

            var vertices = vertList.ToArray();

            List<int> triList = new List<int>();
            for (int i = 0; i < faces.Count; i++)
            {
                triList.Add(faces[i].v1);
                triList.Add(faces[i].v2);
                triList.Add(faces[i].v3);
            }
            var triangles = triList.ToArray();
            var uv = new Vector2[vertices.Length];

            Vector3[] normales = new Vector3[vertList.Count];
            for (int i = 0; i < normales.Length; i++)
                normales[i] = vertList[i].Normalized();


            var normals = normales;

            VertexPositionNormalTexture[] output = new VertexPositionNormalTexture[triangles.Length];

            for (int i = 0; i < triangles.Length; i++)
            {
                output[i] = new VertexPositionNormalTexture(vertices[triangles[i]] + position, normals[triangles[i]], uv[triangles[i]]);
            }

            return output;
        }
    }
}
