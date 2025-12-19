using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace _3DModelViewer.Services
{
    /// <summary>
    /// Generator cho các mô hình 3D nâng cao
    /// Bao gồm: Torus, Ellipsoid, Gear, Spring, Mobius Strip
    /// </summary>
    public class AdvancedModelGenerator
    {
        #region Torus (Hình xuyến)
        
        /// <summary>
        /// Tạo hình xuyến (Torus/Donut)
        /// </summary>
        /// <param name="majorRadius">Bán kính lớn (từ tâm đến trung tâm ống)</param>
        /// <param name="minorRadius">Bán kính nhỏ (bán kính ống)</param>
        /// <param name="majorSegments">Số segments vòng lớn</param>
        /// <param name="minorSegments">Số segments vòng nhỏ</param>
        public static MeshGeometry3D GenerateTorus(double majorRadius = 1.0, double minorRadius = 0.3, int majorSegments = 32, int minorSegments = 16)
        {
            var mesh = new MeshGeometry3D();

            // Generate vertices
            for (int i = 0; i <= majorSegments; i++)
            {
                double u = 2 * Math.PI * i / majorSegments;
                double cosU = Math.Cos(u);
                double sinU = Math.Sin(u);

                for (int j = 0; j <= minorSegments; j++)
                {
                    double v = 2 * Math.PI * j / minorSegments;
                    double cosV = Math.Cos(v);
                    double sinV = Math.Sin(v);

                    double x = (majorRadius + minorRadius * cosV) * cosU;
                    double y = minorRadius * sinV;
                    double z = (majorRadius + minorRadius * cosV) * sinU;

                    mesh.Positions.Add(new Point3D(x, y, z));

                    // Calculate normal
                    double nx = cosV * cosU;
                    double ny = sinV;
                    double nz = cosV * sinU;
                    mesh.Normals.Add(new Vector3D(nx, ny, nz));
                }
            }

            // Generate indices
            for (int i = 0; i < majorSegments; i++)
            {
                for (int j = 0; j < minorSegments; j++)
                {
                    int first = i * (minorSegments + 1) + j;
                    int second = first + minorSegments + 1;

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
        
        #endregion

        #region Ellipsoid (Hình bầu dục)
        
        /// <summary>
        /// Tạo hình bầu dục (Ellipsoid)
        /// </summary>
        /// <param name="radiusX">Bán kính trục X</param>
        /// <param name="radiusY">Bán kính trục Y</param>
        /// <param name="radiusZ">Bán kính trục Z</param>
        /// <param name="segments">Độ chi tiết</param>
        public static MeshGeometry3D GenerateEllipsoid(double radiusX = 1.5, double radiusY = 1.0, double radiusZ = 0.8, int segments = 32)
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

                    double x = radiusX * cosPhi * sinTheta;
                    double y = radiusY * cosTheta;
                    double z = radiusZ * sinPhi * sinTheta;

                    mesh.Positions.Add(new Point3D(x, y, z));

                    // Calculate normal (for ellipsoid, need to account for different radii)
                    Vector3D normal = new Vector3D(
                        x / (radiusX * radiusX),
                        y / (radiusY * radiusY),
                        z / (radiusZ * radiusZ)
                    );
                    normal.Normalize();
                    mesh.Normals.Add(normal);
                }
            }

            // Generate indices
            for (int lat = 0; lat < rings; lat++)
            {
                for (int lon = 0; lon < segments; lon++)
                {
                    int first = lat * (segments + 1) + lon;
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
        
        #endregion

        #region Gear (Bánh răng)
        
        /// <summary>
        /// Tạo bánh răng (Gear)
        /// </summary>
        /// <param name="innerRadius">Bán kính trong</param>
        /// <param name="outerRadius">Bán kính ngoài (đỉnh răng)</param>
        /// <param name="thickness">Độ dày</param>
        /// <param name="toothCount">Số răng</param>
        public static MeshGeometry3D GenerateGear(double innerRadius = 0.5, double outerRadius = 1.0, double thickness = 0.3, int toothCount = 12)
        {
            var mesh = new MeshGeometry3D();
            double halfThickness = thickness / 2.0;

            // Calculate tooth dimensions
            double toothAngle = 2 * Math.PI / toothCount;

            // Generate front and back faces vertices
            for (int side = 0; side < 2; side++)
            {
                double z = side == 0 ? -halfThickness : halfThickness;
                
                // Generate gear profile (4 points per tooth: inner-outer-outer-inner)
                for (int i = 0; i <= toothCount * 4; i++)
                {
                    double angle = (i / 4.0) * toothAngle;
                    int step = i % 4;
                    
                    double radius;
                    if (step == 0 || step == 3)
                        radius = innerRadius;
                    else
                        radius = outerRadius;

                    double x = radius * Math.Cos(angle);
                    double y = radius * Math.Sin(angle);

                    mesh.Positions.Add(new Point3D(x, y, z));
                }
            }

            int pointsPerSide = toothCount * 4 + 1;
            
            // Generate side faces
            for (int i = 0; i < toothCount * 4; i++)
            {
                int frontCurrent = i;
                int frontNext = i + 1;
                int backCurrent = pointsPerSide + i;
                int backNext = pointsPerSide + i + 1;

                // Side face (two triangles)
                mesh.TriangleIndices.Add(frontCurrent);
                mesh.TriangleIndices.Add(backCurrent);
                mesh.TriangleIndices.Add(frontNext);

                mesh.TriangleIndices.Add(backCurrent);
                mesh.TriangleIndices.Add(backNext);
                mesh.TriangleIndices.Add(frontNext);
            }

            // Generate front and back face triangles (triangle fan from center)
            // Front face
            mesh.Positions.Add(new Point3D(0, 0, -halfThickness));
            int centerFront = mesh.Positions.Count - 1;
            
            for (int i = 0; i < toothCount * 4; i++)
            {
                mesh.TriangleIndices.Add(centerFront);
                mesh.TriangleIndices.Add(i);
                mesh.TriangleIndices.Add(i + 1);
            }

            // Back face
            mesh.Positions.Add(new Point3D(0, 0, halfThickness));
            int centerBack = mesh.Positions.Count - 1;
            
            for (int i = 0; i < toothCount * 4; i++)
            {
                mesh.TriangleIndices.Add(centerBack);
                mesh.TriangleIndices.Add(pointsPerSide + i + 1);
                mesh.TriangleIndices.Add(pointsPerSide + i);
            }

            CalculateNormals(mesh);
            return mesh;
        }
        
        #endregion

        #region Spring (Lò xo)
        
        /// <summary>
        /// Tạo lò xo (Spring)
        /// </summary>
        /// <param name="radius">Bán kính xoắn</param>
        /// <param name="coilRadius">Bán kính dây</param>
        /// <param name="height">Chiều cao tổng</param>
        /// <param name="coils">Số vòng xoắn</param>
        /// <param name="segments">Độ chi tiết</param>
        public static MeshGeometry3D GenerateSpring(double radius = 0.5, double coilRadius = 0.1, double height = 3.0, int coils = 10, int segments = 16)
        {
            var mesh = new MeshGeometry3D();
            int totalSteps = coils * segments;

            // Generate vertices
            for (int i = 0; i <= totalSteps; i++)
            {
                double t = (double)i / totalSteps;
                double angle = t * coils * 2 * Math.PI;
                double y = t * height - height / 2;

                // Center line of spring
                double centerX = radius * Math.Cos(angle);
                double centerZ = radius * Math.Sin(angle);

                // Tangent direction for coil orientation
                double tangentX = -Math.Sin(angle);
                double tangentZ = Math.Cos(angle);

                // Create circle around spring path
                for (int j = 0; j <= segments; j++)
                {
                    double circleAngle = j * 2 * Math.PI / segments;
                    double dx = coilRadius * Math.Cos(circleAngle);
                    double dy = coilRadius * Math.Sin(circleAngle);

                    // Position on coil surface
                    double x = centerX + dx * tangentZ;
                    double yPos = y + dy;
                    double z = centerZ - dx * tangentX;

                    mesh.Positions.Add(new Point3D(x, yPos, z));
                }
            }

            // Generate indices
            for (int i = 0; i < totalSteps; i++)
            {
                for (int j = 0; j < segments; j++)
                {
                    int first = i * (segments + 1) + j;
                    int second = first + segments + 1;

                    mesh.TriangleIndices.Add(first);
                    mesh.TriangleIndices.Add(second);
                    mesh.TriangleIndices.Add(first + 1);

                    mesh.TriangleIndices.Add(second);
                    mesh.TriangleIndices.Add(second + 1);
                    mesh.TriangleIndices.Add(first + 1);
                }
            }

            CalculateNormals(mesh);
            return mesh;
        }
        
        #endregion

        #region Mobius Strip (Mặt Möbius)
        
        /// <summary>
        /// Tạo mặt Möbius (Mobius Strip)
        /// Bề mặt toán học đặc biệt chỉ có 1 mặt và 1 cạnh
        /// </summary>
        /// <param name="radius">Bán kính vòng</param>
        /// <param name="width">Độ rộng dải</param>
        /// <param name="segments">Độ chi tiết</param>
        public static MeshGeometry3D GenerateMobiusStrip(double radius = 1.0, double width = 0.4, int segments = 64)
        {
            var mesh = new MeshGeometry3D();

            // Generate vertices
            for (int i = 0; i <= segments; i++)
            {
                double u = 2 * Math.PI * i / segments;
                
                // Generate two points across the width
                for (int j = -1; j <= 1; j += 2)
                {
                    double v = j * width / 2;

                    // Möbius transformation
                    // The key is the u/2 in the twist - this creates the single surface
                    double x = (radius + v * Math.Cos(u / 2)) * Math.Cos(u);
                    double y = v * Math.Sin(u / 2);
                    double z = (radius + v * Math.Cos(u / 2)) * Math.Sin(u);

                    mesh.Positions.Add(new Point3D(x, y, z));
                }
            }

            // Generate indices
            for (int i = 0; i < segments; i++)
            {
                int idx0 = i * 2;
                int idx1 = idx0 + 1;
                int idx2 = idx0 + 2;
                int idx3 = idx0 + 3;

                // Two triangles per segment
                mesh.TriangleIndices.Add(idx0);
                mesh.TriangleIndices.Add(idx2);
                mesh.TriangleIndices.Add(idx1);

                mesh.TriangleIndices.Add(idx1);
                mesh.TriangleIndices.Add(idx2);
                mesh.TriangleIndices.Add(idx3);
            }

            CalculateNormals(mesh);
            return mesh;
        }
        
        #endregion

        #region Helper Methods
        
        /// <summary>
        /// Tính toán normals cho mesh
        /// Sử dụng thuật toán tính normal dựa trên các mặt (face normals)
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
        
        #endregion
    }
}