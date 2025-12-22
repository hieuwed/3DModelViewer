using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace _3DModelViewer.Services
{
    /// <summary>
    /// Generator cho các mô hình giáo dục chuyên nghiệp
    /// Phương án C: 7 mô hình (3 Dễ + 4 Trung bình)
    /// ĐÃ ĐƯỢC KIỂM TRA KỸ - VERSION 2.0
    /// </summary>
    public class EducationalModelGenerator
    {
        #region 1. H2O Molecule (Phân tử nước) ✅
        
        /// <summary>
        /// Tạo phân tử nước H2O với góc liên kết 104.5°
        /// </summary>
        public static Model3DGroup GenerateH2OMolecule()
        {
            var group = new Model3DGroup();
            
            // Oxygen atom (màu đỏ, lớn hơn)
            var oxygenMesh = CreateSphere(new Point3D(0, 0, 0), 0.4, System.Windows.Media.Colors.Red);
            group.Children.Add(oxygenMesh);
            
            // Hydrogen atoms (màu trắng, nhỏ hơn)
            double bondLength = 0.96; // Å (angstrom)
            double bondAngle = 104.5 * Math.PI / 180.0; // Góc H-O-H chuẩn
            
            // H1 position
            double h1X = bondLength * Math.Sin(bondAngle / 2);
            double h1Y = bondLength * Math.Cos(bondAngle / 2);
            var h1Mesh = CreateSphere(new Point3D(h1X, h1Y, 0), 0.2, System.Windows.Media.Colors.White);
            group.Children.Add(h1Mesh);
            
            // H2 position
            var h2Mesh = CreateSphere(new Point3D(h1X, -h1Y, 0), 0.2, System.Windows.Media.Colors.White);
            group.Children.Add(h2Mesh);
            
            // Bonds (liên kết cộng hóa trị)
            var bond1 = CreateBond(new Point3D(0, 0, 0), new Point3D(h1X, h1Y, 0), 0.05, System.Windows.Media.Colors.LightGray);
            group.Children.Add(bond1);
            
            var bond2 = CreateBond(new Point3D(0, 0, 0), new Point3D(h1X, -h1Y, 0), 0.05, System.Windows.Media.Colors.LightGray);
            group.Children.Add(bond2);
            
            return group;
        }
        
        #endregion

        #region 2. Triangular Prism (Lăng trụ tam giác) ✅
        
        /// <summary>
        /// Tạo lăng trụ tam giác đều
        /// </summary>
        public static MeshGeometry3D GenerateTriangularPrism(double baseSize = 1.5, double height = 2.5)
        {
            var mesh = new MeshGeometry3D();
            
            // Tính toán đỉnh tam giác đều
            double h = baseSize * Math.Sqrt(3) / 2; // Chiều cao tam giác
            double halfHeight = height / 2;
            
            // 6 đỉnh của lăng trụ
            Point3D[] vertices = new Point3D[]
            {
                // Tam giác dưới (y = -halfHeight)
                new Point3D(-baseSize/2, -halfHeight, -h/3),
                new Point3D(baseSize/2, -halfHeight, -h/3),
                new Point3D(0, -halfHeight, 2*h/3),
                // Tam giác trên (y = halfHeight)
                new Point3D(-baseSize/2, halfHeight, -h/3),
                new Point3D(baseSize/2, halfHeight, -h/3),
                new Point3D(0, halfHeight, 2*h/3)
            };
            
            // Add all vertices
            foreach (var v in vertices)
            {
                mesh.Positions.Add(v);
            }
            
            // Bottom face (tam giác dưới) - ngược chiều kim đồng hồ nhìn từ dưới lên
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(1);
            
            // Top face (tam giác trên) - ngược chiều kim đồng hồ nhìn từ trên xuống
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(5);
            
            // Side faces (3 mặt chữ nhật)
            // Face 1: vertices 0-1-4-3
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(3);
            
            // Face 2: vertices 1-2-5-4
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(4);
            
            // Face 3: vertices 2-0-3-5
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(5);
            
            CalculateNormals(mesh);
            return mesh;
        }
        
        #endregion

        #region 3. Sine Wave 3D (Sóng sin) ✅ FIXED
        
        /// <summary>
        /// Tạo sóng sin 3D (mô phỏng sóng cơ/điện từ)
        /// ĐÃ FIX - Bảo đảm hiển thị đúng
        /// </summary>
        public static MeshGeometry3D GenerateSineWave3D(double amplitude = 0.5, double wavelength = 2.0, int cycles = 3, int resolution = 64)
        {
            var mesh = new MeshGeometry3D();
            
            double totalLength = wavelength * cycles;
            double width = 1.0;
            int lengthSegments = resolution;
            int widthSegments = 20;
            
            // Generate vertices với sóng sin
            for (int i = 0; i <= lengthSegments; i++)
            {
                double x = (i / (double)lengthSegments) * totalLength - totalLength / 2;
                double y = amplitude * Math.Sin((2 * Math.PI / wavelength) * (x + totalLength / 2));
                
                for (int j = 0; j <= widthSegments; j++)
                {
                    double z = (j / (double)widthSegments) * width - width / 2;
                    mesh.Positions.Add(new Point3D(x, y, z));
                }
            }
            
            // Generate triangle indices
            for (int i = 0; i < lengthSegments; i++)
            {
                for (int j = 0; j < widthSegments; j++)
                {
                    int current = i * (widthSegments + 1) + j;
                    int next = current + widthSegments + 1;
                    
                    // Triangle 1
                    mesh.TriangleIndices.Add(current);
                    mesh.TriangleIndices.Add(next);
                    mesh.TriangleIndices.Add(current + 1);
                    
                    // Triangle 2
                    mesh.TriangleIndices.Add(next);
                    mesh.TriangleIndices.Add(next + 1);
                    mesh.TriangleIndices.Add(current + 1);
                }
            }
            
            CalculateNormals(mesh);
            return mesh;
        }
        
        #endregion

        #region 4-6. Platonic Solids (Đa diện đều Plato) ✅
        
        /// <summary>
        /// Tạo Tetrahedron (4 mặt) - Đa diện đều đơn giản nhất
        /// </summary>
        public static MeshGeometry3D GenerateTetrahedron(double size = 1.5)
        {
            var mesh = new MeshGeometry3D();
            
            double a = size / Math.Sqrt(2);
            
            // 4 đỉnh của tetrahedron đều
            Point3D[] vertices = new Point3D[]
            {
                new Point3D(a, a, a),
                new Point3D(a, -a, -a),
                new Point3D(-a, a, -a),
                new Point3D(-a, -a, a)
            };
            
            foreach (var v in vertices)
            {
                mesh.Positions.Add(v);
            }
            
            // 4 mặt tam giác
            int[][] faces = new int[][]
            {
                new int[] { 0, 2, 1 },
                new int[] { 0, 3, 2 },
                new int[] { 0, 1, 3 },
                new int[] { 1, 2, 3 }
            };
            
            foreach (var face in faces)
            {
                mesh.TriangleIndices.Add(face[0]);
                mesh.TriangleIndices.Add(face[1]);
                mesh.TriangleIndices.Add(face[2]);
            }
            
            CalculateNormals(mesh);
            return mesh;
        }
        
        /// <summary>
        /// Tạo Octahedron (8 mặt)
        /// </summary>
        public static MeshGeometry3D GenerateOctahedron(double size = 1.5)
        {
            var mesh = new MeshGeometry3D();
            
            // 6 đỉnh (ở 6 hướng chính)
            Point3D[] vertices = new Point3D[]
            {
                new Point3D(size, 0, 0),    // right
                new Point3D(-size, 0, 0),   // left
                new Point3D(0, size, 0),    // top
                new Point3D(0, -size, 0),   // bottom
                new Point3D(0, 0, size),    // front
                new Point3D(0, 0, -size)    // back
            };
            
            foreach (var v in vertices)
            {
                mesh.Positions.Add(v);
            }
            
            // 8 mặt tam giác
            int[][] faces = new int[][]
            {
                new int[] { 0, 2, 4 }, new int[] { 0, 4, 3 },
                new int[] { 0, 3, 5 }, new int[] { 0, 5, 2 },
                new int[] { 1, 4, 2 }, new int[] { 1, 3, 4 },
                new int[] { 1, 5, 3 }, new int[] { 1, 2, 5 }
            };
            
            foreach (var face in faces)
            {
                mesh.TriangleIndices.Add(face[0]);
                mesh.TriangleIndices.Add(face[1]);
                mesh.TriangleIndices.Add(face[2]);
            }
            
            CalculateNormals(mesh);
            return mesh;
        }
        
        /// <summary>
        /// Tạo Icosahedron (20 mặt)
        /// </summary>
        public static MeshGeometry3D GenerateIcosahedron(double size = 1.5)
        {
            var mesh = new MeshGeometry3D();
            
            double t = (1.0 + Math.Sqrt(5.0)) / 2.0; // Golden ratio φ
            double s = size / Math.Sqrt(1 + t * t);
            
            // 12 đỉnh (tạo từ 3 golden rectangles vuông góc nhau)
            Point3D[] vertices = new Point3D[]
            {
                new Point3D(-s, t*s, 0), new Point3D(s, t*s, 0), 
                new Point3D(-s, -t*s, 0), new Point3D(s, -t*s, 0),
                new Point3D(0, -s, t*s), new Point3D(0, s, t*s), 
                new Point3D(0, -s, -t*s), new Point3D(0, s, -t*s),
                new Point3D(t*s, 0, -s), new Point3D(t*s, 0, s), 
                new Point3D(-t*s, 0, -s), new Point3D(-t*s, 0, s)
            };
            
            foreach (var v in vertices)
            {
                mesh.Positions.Add(v);
            }
            
            // 20 mặt tam giác
            int[][] faces = new int[][]
            {
                new int[] { 0, 11, 5 }, new int[] { 0, 5, 1 }, 
                new int[] { 0, 1, 7 }, new int[] { 0, 7, 10 }, 
                new int[] { 0, 10, 11 },
                new int[] { 1, 5, 9 }, new int[] { 5, 11, 4 }, 
                new int[] { 11, 10, 2 }, new int[] { 10, 7, 6 }, 
                new int[] { 7, 1, 8 },
                new int[] { 3, 9, 4 }, new int[] { 3, 4, 2 }, 
                new int[] { 3, 2, 6 }, new int[] { 3, 6, 8 }, 
                new int[] { 3, 8, 9 },
                new int[] { 4, 9, 5 }, new int[] { 2, 4, 11 }, 
                new int[] { 6, 2, 10 }, new int[] { 8, 6, 7 }, 
                new int[] { 9, 8, 1 }
            };
            
            foreach (var face in faces)
            {
                mesh.TriangleIndices.Add(face[0]);
                mesh.TriangleIndices.Add(face[1]);
                mesh.TriangleIndices.Add(face[2]);
            }
            
            CalculateNormals(mesh);
            return mesh;
        }
        
        #endregion

        #region 7. Fibonacci Spiral 3D ✅
        
        /// <summary>
        /// Sử dụng thuật toán Fibonacci lattice trên sphere
        /// </summary>
        public static MeshGeometry3D GenerateFibonacciSpiral(int points = 500, double scale = 2.0)
        {
            var mesh = new MeshGeometry3D();
            
            double goldenAngle = Math.PI * (3.0 - Math.Sqrt(5.0)); // ≈ 137.5 degrees
            double sphereRadius = 0.04; // Bán kính mỗi sphere nhỏ
            int sphereSegments = 8;
            
            // Tạo các điểm theo Fibonacci lattice
            for (int i = 0; i < points; i++)
            {
                // Tính toán vị trí trên sphere
                double y = 1 - (i / (double)(points - 1)) * 2; // y từ 1 đến -1
                double radiusAtY = Math.Sqrt(1 - y * y);
                double theta = goldenAngle * i;
                
                double x = Math.Cos(theta) * radiusAtY * scale;
                double z = Math.Sin(theta) * radiusAtY * scale;
                
                Point3D center = new Point3D(x, y * scale, z);
                
                // Tạo sphere nhỏ tại mỗi điểm
                AddSmallSphere(mesh, center, sphereRadius, sphereSegments);
            }
            
            CalculateNormals(mesh);
            return mesh;
        }
        
        /// <summary>
        /// Thêm một sphere nhỏ vào mesh
        /// </summary>
        private static void AddSmallSphere(MeshGeometry3D mesh, Point3D center, double radius, int segments)
        {
            int baseIndex = mesh.Positions.Count;
            int rings = segments / 2;
            
            // Generate sphere vertices
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
                    
                    double x = radius * cosPhi * sinTheta;
                    double y = radius * cosTheta;
                    double z = radius * sinPhi * sinTheta;
                    
                    mesh.Positions.Add(new Point3D(
                        center.X + x,
                        center.Y + y,
                        center.Z + z
                    ));
                }
            }
            
            // Generate sphere indices
            for (int lat = 0; lat < rings; lat++)
            {
                for (int lon = 0; lon < segments; lon++)
                {
                    int first = baseIndex + lat * (segments + 1) + lon;
                    int second = first + segments + 1;
                    
                    mesh.TriangleIndices.Add(first);
                    mesh.TriangleIndices.Add(second);
                    mesh.TriangleIndices.Add(first + 1);
                    
                    mesh.TriangleIndices.Add(second);
                    mesh.TriangleIndices.Add(second + 1);
                    mesh.TriangleIndices.Add(first + 1);
                }
            }
        }
        
        #endregion

        #region 8. Diamond Lattice (Cấu trúc kim cương) ✅
        
        /// <summary>
        /// Tạo cấu trúc tinh thể kim cương (Diamond cubic)
        /// </summary>
        public static Model3DGroup GenerateDiamondLattice(double size = 2.0)
        {
            var group = new Model3DGroup();
            
            // Carbon atoms positions trong unit cell kim cương
            Point3D[] carbonPositions = new Point3D[]
            {
                // 8 góc của cube
                new Point3D(0, 0, 0),
                new Point3D(size, 0, 0),
                new Point3D(0, size, 0),
                new Point3D(size, size, 0),
                new Point3D(0, 0, size),
                new Point3D(size, 0, size),
                new Point3D(0, size, size),
                new Point3D(size, size, size),
                // 6 face centers
                new Point3D(size/2, size/2, 0),
                new Point3D(size/2, 0, size/2),
                new Point3D(0, size/2, size/2),
                new Point3D(size, size/2, size/2),
                new Point3D(size/2, size, size/2),
                new Point3D(size/2, size/2, size),
                // 4 internal tetrahedral positions
                new Point3D(size/4, size/4, size/4),
                new Point3D(3*size/4, 3*size/4, size/4),
                new Point3D(3*size/4, size/4, 3*size/4),
                new Point3D(size/4, 3*size/4, 3*size/4)
            };
            
            // Center the structure
            double offset = size / 2;
            for (int i = 0; i < carbonPositions.Length; i++)
            {
                carbonPositions[i] = new Point3D(
                    carbonPositions[i].X - offset,
                    carbonPositions[i].Y - offset,
                    carbonPositions[i].Z - offset
                );
            }
            
            // Add carbon atoms
            foreach (var pos in carbonPositions)
            {
                var atom = CreateSphere(pos, 0.15, System.Windows.Media.Colors.DarkGray);
                group.Children.Add(atom);
            }
            
            // Add bonds (chỉ các nearest neighbors)
            var bondColor = System.Windows.Media.Colors.LightGray;
            double bondThreshold = size * 0.6;
            
            for (int i = 0; i < carbonPositions.Length; i++)
            {
                for (int j = i + 1; j < carbonPositions.Length; j++)
                {
                    double distance = Distance(carbonPositions[i], carbonPositions[j]);
                    if (distance < bondThreshold)
                    {
                        var bond = CreateBond(carbonPositions[i], carbonPositions[j], 0.04, bondColor);
                        group.Children.Add(bond);
                    }
                }
            }
            
            return group;
        }
        
        #endregion

        #region Helper Methods
        
        /// <summary>
        /// Tạo sphere (quả cầu) cho atoms
        /// </summary>
        private static GeometryModel3D CreateSphere(Point3D center, double radius, System.Windows.Media.Color color)
        {
            var sphere = new MeshGeometry3D();
            int segments = 20;
            int rings = 20;
            
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
                    
                    sphere.Positions.Add(new Point3D(
                        center.X + radius * x,
                        center.Y + radius * y,
                        center.Z + radius * z
                    ));
                }
            }
            
            for (int lat = 0; lat < rings; lat++)
            {
                for (int lon = 0; lon < segments; lon++)
                {
                    int first = lat * (segments + 1) + lon;
                    int second = first + segments + 1;
                    
                    sphere.TriangleIndices.Add(first);
                    sphere.TriangleIndices.Add(second);
                    sphere.TriangleIndices.Add(first + 1);
                    
                    sphere.TriangleIndices.Add(second);
                    sphere.TriangleIndices.Add(second + 1);
                    sphere.TriangleIndices.Add(first + 1);
                }
            }
            
            var material = new DiffuseMaterial(new SolidColorBrush(color));
            var specular = new SpecularMaterial(new SolidColorBrush(System.Windows.Media.Colors.White), 50);
            var materialGroup = new MaterialGroup();
            materialGroup.Children.Add(material);
            materialGroup.Children.Add(specular);
            
            return new GeometryModel3D { Geometry = sphere, Material = materialGroup, BackMaterial = materialGroup };
        }
        
        /// <summary>
        /// Tạo bond (liên kết) giữa 2 atoms
        /// </summary>
        private static GeometryModel3D CreateBond(Point3D start, Point3D end, double radius, System.Windows.Media.Color color)
        {
            var direction = new Vector3D(
                end.X - start.X,
                end.Y - start.Y,
                end.Z - start.Z
            );
            double length = direction.Length;
            direction.Normalize();
            
            var cylinder = new MeshGeometry3D();
            int segments = 8;
            
            for (int i = 0; i <= segments; i++)
            {
                double angle = i * 2 * Math.PI / segments;
                double x = radius * Math.Cos(angle);
                double z = radius * Math.Sin(angle);
                
                cylinder.Positions.Add(new Point3D(x, 0, z));
                cylinder.Positions.Add(new Point3D(x, length, z));
            }
            
            for (int i = 0; i < segments; i++)
            {
                int bottom1 = i * 2;
                int top1 = bottom1 + 1;
                int bottom2 = (i + 1) * 2;
                int top2 = bottom2 + 1;
                
                cylinder.TriangleIndices.Add(bottom1);
                cylinder.TriangleIndices.Add(top1);
                cylinder.TriangleIndices.Add(bottom2);
                
                cylinder.TriangleIndices.Add(top1);
                cylinder.TriangleIndices.Add(top2);
                cylinder.TriangleIndices.Add(bottom2);
            }
            
            var model = new GeometryModel3D
            {
                Geometry = cylinder,
                Material = new DiffuseMaterial(new SolidColorBrush(color)),
                BackMaterial = new DiffuseMaterial(new SolidColorBrush(color))
            };
            
            // Transform để kết nối start và end
            var transformGroup = new Transform3DGroup();
            
            // Rotate để align với direction
            var yAxis = new Vector3D(0, 1, 0);
            var axis = Vector3D.CrossProduct(yAxis, direction);
            if (axis.Length > 0.001)
            {
                double angle = Math.Acos(Vector3D.DotProduct(yAxis, direction));
                transformGroup.Children.Add(new RotateTransform3D(
                    new AxisAngleRotation3D(axis, angle * 180 / Math.PI)
                ));
            }
            
            // Translate to start position
            transformGroup.Children.Add(new TranslateTransform3D(start.X, start.Y, start.Z));
            
            model.Transform = transformGroup;
            return model;
        }
        
        /// <summary>
        /// Tính khoảng cách giữa 2 điểm
        /// </summary>
        private static double Distance(Point3D p1, Point3D p2)
        {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double dz = p2.Z - p1.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
        
        /// <summary>
        /// Tính normals cho mesh
        /// </summary>
        private static void CalculateNormals(MeshGeometry3D mesh)
        {
            mesh.Normals.Clear();
            
            for (int i = 0; i < mesh.Positions.Count; i++)
            {
                mesh.Normals.Add(new Vector3D(0, 0, 0));
            }
            
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