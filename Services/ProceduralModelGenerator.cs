using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace _3DModelViewer.Services
{
    /// <summary>
    /// Service for generating procedural 3D models
    /// </summary>
    public class ProceduralModelGenerator
    {
        private const int DefaultSphereSegments = 32;
        private const double CubeNormalizeValue = 1.0;
        
        /// <summary>
        /// Generates a cube mesh
        /// </summary>
        public static MeshGeometry3D GenerateCube(double size = 1.0)
        {
            var mesh = new MeshGeometry3D();
            double half = size / 2.0;

            // Define 8 vertices of the cube
            Point3D[] vertices = new Point3D[]
            {
                new Point3D(-half, -half, -half), // 0
                new Point3D( half, -half, -half), // 1
                new Point3D( half,  half, -half), // 2
                new Point3D(-half,  half, -half), // 3
                new Point3D(-half, -half,  half), // 4
                new Point3D( half, -half,  half), // 5
                new Point3D( half,  half,  half), // 6
                new Point3D(-half,  half,  half)  // 7
            };

            // Define 6 faces (each face has 2 triangles = 6 vertices)
            int[] indices = new int[]
            {
                // Front face
                0, 1, 2,  0, 2, 3,
                // Back face
                5, 4, 7,  5, 7, 6,
                // Left face
                4, 0, 3,  4, 3, 7,
                // Right face
                1, 5, 6,  1, 6, 2,
                // Top face
                3, 2, 6,  3, 6, 7,
                // Bottom face
                4, 5, 1,  4, 1, 0
            };

            // Add all vertices (need to duplicate for proper normals per face)
            foreach (var index in indices)
            {
                mesh.Positions.Add(vertices[index]);
            }

            // Add indices
            for (int i = 0; i < indices.Length; i++)
            {
                mesh.TriangleIndices.Add(i);
            }

            // Calculate normals
            CalculateNormals(mesh);

            return mesh;
        }

        /// <summary>
        /// Generates a sphere mesh using UV sphere algorithm
        /// </summary>
        public static MeshGeometry3D GenerateSphere(double radius = 1.0, int segments = 32)
        {
            var mesh = new MeshGeometry3D();
            int rings = segments / 2;

            // Generate vertices
            for (int lat = 0; lat <= rings; lat++)
            {
                double theta = lat * Math.PI / rings;
                double sinTheta = Math.Sin(theta);
                double cosTheta = Math.Cos(theta);

                for (int lon = 0; lon <= segments; lon++)
                {
                    double phi = lon * 2 * Math.PI / segments;
                    double sinPhi = Math.Sin(phi);
                    double cosPhi = Math.Cos(phi);

                    double x = cosPhi * sinTheta;
                    double y = cosTheta;
                    double z = sinPhi * sinTheta;

                    mesh.Positions.Add(new Point3D(radius * x, radius * y, radius * z));
                    mesh.Normals.Add(new Vector3D(x, y, z));
                }
            }

            // Generate indices
            for (int lat = 0; lat < rings; lat++)
            {
                for (int lon = 0; lon < segments; lon++)
                {
                    int first = (lat * (segments + 1)) + lon;
                    int second = first + segments + 1;

                    mesh.TriangleIndices.Add(first);
                    mesh.TriangleIndices.Add(second);
                    mesh.TriangleIndices.Add(first + 1);

                    mesh.TriangleIndices.Add(second);
                    mesh.TriangleIndices.Add(second + 1);
                    mesh.TriangleIndices.Add(first + 1);
                }
            }

            return mesh;
        }

        /// <summary>
        /// Generates a cylinder mesh
        /// </summary>
        public static MeshGeometry3D GenerateCylinder(double radius = 1.0, double height = 2.0, int segments = 32)
        {
            var mesh = new MeshGeometry3D();
            double halfHeight = height / 2.0;

            // Generate side vertices
            for (int i = 0; i <= segments; i++)
            {
                double angle = 2 * Math.PI * i / segments;
                double x = radius * Math.Cos(angle);
                double z = radius * Math.Sin(angle);

                // Bottom vertex
                mesh.Positions.Add(new Point3D(x, -halfHeight, z));
                mesh.Normals.Add(new Vector3D(x, 0, z).Normalized());

                // Top vertex
                mesh.Positions.Add(new Point3D(x, halfHeight, z));
                mesh.Normals.Add(new Vector3D(x, 0, z).Normalized());
            }

            // Generate side faces
            for (int i = 0; i < segments; i++)
            {
                int bottomLeft = i * 2;
                int bottomRight = (i + 1) * 2;
                int topLeft = bottomLeft + 1;
                int topRight = bottomRight + 1;

                // First triangle
                mesh.TriangleIndices.Add(bottomLeft);
                mesh.TriangleIndices.Add(topLeft);
                mesh.TriangleIndices.Add(bottomRight);

                // Second triangle
                mesh.TriangleIndices.Add(bottomRight);
                mesh.TriangleIndices.Add(topLeft);
                mesh.TriangleIndices.Add(topRight);
            }

            // Add caps
            int baseIndex = mesh.Positions.Count;

            // Bottom cap
            mesh.Positions.Add(new Point3D(0, -halfHeight, 0));
            mesh.Normals.Add(new Vector3D(0, -1, 0));

            for (int i = 0; i <= segments; i++)
            {
                double angle = 2 * Math.PI * i / segments;
                double x = radius * Math.Cos(angle);
                double z = radius * Math.Sin(angle);
                mesh.Positions.Add(new Point3D(x, -halfHeight, z));
                mesh.Normals.Add(new Vector3D(0, -1, 0));
            }

            for (int i = 0; i < segments; i++)
            {
                mesh.TriangleIndices.Add(baseIndex);
                mesh.TriangleIndices.Add(baseIndex + i + 2);
                mesh.TriangleIndices.Add(baseIndex + i + 1);
            }

            // Top cap
            baseIndex = mesh.Positions.Count;
            mesh.Positions.Add(new Point3D(0, halfHeight, 0));
            mesh.Normals.Add(new Vector3D(0, 1, 0));

            for (int i = 0; i <= segments; i++)
            {
                double angle = 2 * Math.PI * i / segments;
                double x = radius * Math.Cos(angle);
                double z = radius * Math.Sin(angle);
                mesh.Positions.Add(new Point3D(x, halfHeight, z));
                mesh.Normals.Add(new Vector3D(0, 1, 0));
            }

            for (int i = 0; i < segments; i++)
            {
                mesh.TriangleIndices.Add(baseIndex);
                mesh.TriangleIndices.Add(baseIndex + i + 1);
                mesh.TriangleIndices.Add(baseIndex + i + 2);
            }

            return mesh;
        }

        /// <summary>
        /// Generates a cone mesh
        /// </summary>
        public static MeshGeometry3D GenerateCone(double radius = 1.0, double height = 2.0, int segments = 32)
        {
            var mesh = new MeshGeometry3D();
            double halfHeight = height / 2.0;

            // Apex of the cone
            Point3D apex = new Point3D(0, halfHeight, 0);

            // Generate base circle and side faces
            for (int i = 0; i <= segments; i++)
            {
                double angle = 2 * Math.PI * i / segments;
                double x = radius * Math.Cos(angle);
                double z = radius * Math.Sin(angle);

                Point3D basePoint = new Point3D(x, -halfHeight, z);

                // Add vertices for side face
                mesh.Positions.Add(apex);
                mesh.Positions.Add(basePoint);

                if (i < segments)
                {
                    double nextAngle = 2 * Math.PI * (i + 1) / segments;
                    double nextX = radius * Math.Cos(nextAngle);
                    double nextZ = radius * Math.Sin(nextAngle);
                    Point3D nextBasePoint = new Point3D(nextX, -halfHeight, nextZ);

                    mesh.Positions.Add(nextBasePoint);

                    // Add triangle indices
                    int baseIdx = i * 3;
                    mesh.TriangleIndices.Add(baseIdx);
                    mesh.TriangleIndices.Add(baseIdx + 1);
                    mesh.TriangleIndices.Add(baseIdx + 2);
                }
            }

            // Add base cap
            int baseIndex = mesh.Positions.Count;
            mesh.Positions.Add(new Point3D(0, -halfHeight, 0));

            for (int i = 0; i <= segments; i++)
            {
                double angle = 2 * Math.PI * i / segments;
                double x = radius * Math.Cos(angle);
                double z = radius * Math.Sin(angle);
                mesh.Positions.Add(new Point3D(x, -halfHeight, z));
            }

            for (int i = 0; i < segments; i++)
            {
                mesh.TriangleIndices.Add(baseIndex);
                mesh.TriangleIndices.Add(baseIndex + i + 2);
                mesh.TriangleIndices.Add(baseIndex + i + 1);
            }

            CalculateNormals(mesh);

            return mesh;
        }

        /// <summary>
        /// Generates a pyramid mesh (square base)
        /// </summary>
        public static MeshGeometry3D GeneratePyramid(double size = 1.0)
        {
            var mesh = new MeshGeometry3D();
            double half = size / 2.0;
            double height = size;

            // Define vertices
            Point3D apex = new Point3D(0, height / 2, 0);
            Point3D[] baseVertices = new Point3D[]
            {
                new Point3D(-half, -height/2, -half),
                new Point3D( half, -height/2, -half),
                new Point3D( half, -height/2,  half),
                new Point3D(-half, -height/2,  half)
            };

            // Side faces (4 triangles)
            for (int i = 0; i < 4; i++)
            {
                int next = (i + 1) % 4;
                mesh.Positions.Add(apex);
                mesh.Positions.Add(baseVertices[i]);
                mesh.Positions.Add(baseVertices[next]);

                mesh.TriangleIndices.Add(i * 3);
                mesh.TriangleIndices.Add(i * 3 + 1);
                mesh.TriangleIndices.Add(i * 3 + 2);
            }

            // Base (2 triangles)
            int baseIdx = mesh.Positions.Count;
            mesh.Positions.Add(baseVertices[0]);
            mesh.Positions.Add(baseVertices[1]);
            mesh.Positions.Add(baseVertices[2]);
            mesh.Positions.Add(baseVertices[3]);

            mesh.TriangleIndices.Add(baseIdx);
            mesh.TriangleIndices.Add(baseIdx + 2);
            mesh.TriangleIndices.Add(baseIdx + 1);

            mesh.TriangleIndices.Add(baseIdx);
            mesh.TriangleIndices.Add(baseIdx + 3);
            mesh.TriangleIndices.Add(baseIdx + 2);

            CalculateNormals(mesh);

            return mesh;
        }

        /// <summary>
        /// Calculates normals for a mesh
        /// </summary>
        private static void CalculateNormals(MeshGeometry3D mesh)
        {
            mesh.Normals.Clear();

            // Initialize normals to zero
            for (int i = 0; i < mesh.Positions.Count; i++)
            {
                mesh.Normals.Add(new Vector3D(0, 0, 0));
            }

            // Calculate face normals and accumulate
            for (int i = 0; i < mesh.TriangleIndices.Count; i += 3)
            {
                int idx0 = mesh.TriangleIndices[i];
                int idx1 = mesh.TriangleIndices[i + 1];
                int idx2 = mesh.TriangleIndices[i + 2];

                Point3D p0 = mesh.Positions[idx0];
                Point3D p1 = mesh.Positions[idx1];
                Point3D p2 = mesh.Positions[idx2];

                Vector3D v1 = p1 - p0;
                Vector3D v2 = p2 - p0;
                Vector3D normal = Vector3D.CrossProduct(v1, v2);
                normal.Normalize();

                mesh.Normals[idx0] += normal;
                mesh.Normals[idx1] += normal;
                mesh.Normals[idx2] += normal;
            }

            // Normalize all normals
            for (int i = 0; i < mesh.Normals.Count; i++)
            {
                Vector3D normal = mesh.Normals[i];
                normal.Normalize();
                mesh.Normals[i] = normal;
            }
        }

        /// <summary>
        /// Generates a cuboid (rectangular box) mesh with custom width, height, and depth
        /// </summary>
        public static MeshGeometry3D GenerateCuboid(double width = 2.0, double height = 1.5, double depth = 1.0)
        {
            var mesh = new MeshGeometry3D();
            
            double w = width / 2.0;
            double h = height / 2.0;
            double d = depth / 2.0;

            // Define 8 vertices of the cuboid
            Point3D[] vertices = new Point3D[]
            {
                new Point3D(-w, -h, -d), // 0 - front bottom left
                new Point3D( w, -h, -d), // 1 - front bottom right
                new Point3D( w,  h, -d), // 2 - front top right
                new Point3D(-w,  h, -d), // 3 - front top left
                new Point3D(-w, -h,  d), // 4 - back bottom left
                new Point3D( w, -h,  d), // 5 - back bottom right
                new Point3D( w,  h,  d), // 6 - back top right
                new Point3D(-w,  h,  d)  // 7 - back top left
            };

            // Add all vertices
            foreach (var vertex in vertices)
            {
                mesh.Positions.Add(vertex);
            }

            // Define 6 faces (each face has 2 triangles = 6 indices)
            int[] indices = new int[]
            {
                // Front face (z = -d)
                0, 1, 2,  0, 2, 3,
                // Back face (z = d)
                5, 4, 7,  5, 7, 6,
                // Left face (x = -w)
                4, 0, 3,  4, 3, 7,
                // Right face (x = w)
                1, 5, 6,  1, 6, 2,
                // Top face (y = h)
                3, 2, 6,  3, 6, 7,
                // Bottom face (y = -h)
                4, 5, 1,  4, 1, 0
            };

            // Add all triangles
            foreach (var index in indices)
            {
                mesh.TriangleIndices.Add(index);
            }

            // Calculate and add normals
            CalculateNormals(mesh);

            return mesh;
        }
    }

    /// <summary>
    /// Extension method for Vector3D normalization
    /// </summary>
    public static class Vector3DExtensions
    {
        public static Vector3D Normalized(this Vector3D vector)
        {
            vector.Normalize();
            return vector;
        }
    }
}
