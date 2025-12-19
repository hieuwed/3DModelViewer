using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using System.Windows.Data;

namespace _3DModelViewer.Services
{
    /// <summary>
    /// Generator cho chuỗi DNA xoắn kép với animation
    /// </summary>
    public class DNAGenerator
    {
        private Model3DGroup dnaGroup;
        private Storyboard animationStoryboard;
        private RotateTransform3D rotationTransform;

        public Model3DGroup DNAGroup => dnaGroup;
        public bool IsAnimating { get; private set; }

        public DNAGenerator()
        {
            dnaGroup = new Model3DGroup();
            animationStoryboard = new Storyboard();
            rotationTransform = new RotateTransform3D { Rotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0) };
            IsAnimating = true;
        }

        /// <summary>
        /// Tạo chuỗi DNA xoắn kép
        /// </summary>
        public Model3DGroup Generate(int basePairs = 20, double height = 10.0)
        {
            dnaGroup.Children.Clear();

            // Thông số chuỗi DNA
            double radius = 0.8;           // Bán kính xoắn
            double baseRadius = 0.15;      // Bán kính các nucleotide
            double bondRadius = 0.08;      // Bán kính liên kết
            double turnHeight = height / 2; // Chiều cao cho 1 vòng xoắn

            // Màu sắc cho các nucleotide (A-T, G-C)
            System.Windows.Media.Color adenineColor = System.Windows.Media.Colors.Red;      // A
            System.Windows.Media.Color thymineColor = System.Windows.Media.Colors.Blue;     // T
            System.Windows.Media.Color guanineColor = System.Windows.Media.Colors.Yellow;   // G
            System.Windows.Media.Color cytosineColor = System.Windows.Media.Colors.Green;   // C
            System.Windows.Media.Color backboneColor = System.Windows.Media.Colors.Gray;    // Xương sống
            System.Windows.Media.Color bondColor = System.Windows.Media.Colors.White;       // Liên kết

            // Tạo mỗi cặp base
            for (int i = 0; i < basePairs; i++)
            {
                double y = (i / (double)basePairs) * height - height / 2;
                double angle = (i / (double)basePairs) * Math.PI * 4; // 2 vòng xoắn

                // Tính vị trí 2 chuỗi
                double x1 = Math.Cos(angle) * radius;
                double z1 = Math.Sin(angle) * radius;
                double x2 = Math.Cos(angle + Math.PI) * radius;
                double z2 = Math.Sin(angle + Math.PI) * radius;

                // Chọn màu ngẫu nhiên cho cặp base (mô phỏng A-T hoặc G-C)
                bool isAT = (i % 2 == 0);
                System.Windows.Media.Color color1 = isAT ? adenineColor : guanineColor;
                System.Windows.Media.Color color2 = isAT ? thymineColor : cytosineColor;

                // Tạo nucleotide thứ nhất
                CreateSphere(new Point3D(x1, y, z1), baseRadius, color1);

                // Tạo nucleotide thứ hai
                CreateSphere(new Point3D(x2, y, z2), baseRadius, color2);

                // Tạo liên kết giữa 2 cặp base
                CreateBond(new Point3D(x1, y, z1), new Point3D(x2, y, z2), bondRadius, bondColor);

                // Tạo xương sống (backbone) nối các nucleotide
                if (i > 0)
                {
                    double prevY = ((i - 1) / (double)basePairs) * height - height / 2;
                    double prevAngle = ((i - 1) / (double)basePairs) * Math.PI * 4;

                    double prevX1 = Math.Cos(prevAngle) * radius;
                    double prevZ1 = Math.Sin(prevAngle) * radius;
                    double prevX2 = Math.Cos(prevAngle + Math.PI) * radius;
                    double prevZ2 = Math.Sin(prevAngle + Math.PI) * radius;

                    CreateBond(new Point3D(prevX1, prevY, prevZ1), 
                              new Point3D(x1, y, z1), bondRadius * 1.5, backboneColor);
                    CreateBond(new Point3D(prevX2, prevY, prevZ2), 
                              new Point3D(x2, y, z2), bondRadius * 1.5, backboneColor);
                }
            }

            // Setup animation
            SetupRotationAnimation();

            return dnaGroup;
        }

        /// <summary>
        /// Tạo hình cầu cho nucleotide
        /// </summary>
        private void CreateSphere(Point3D center, double radius, System.Windows.Media.Color color)
        {
            var sphere = new SphereVisual3D
            {
                Radius = radius,
                Center = center,
                ThetaDiv = 16,
                PhiDiv = 16
            };

            var material = new DiffuseMaterial(new SolidColorBrush(color));
            var specular = new SpecularMaterial(new SolidColorBrush(System.Windows.Media.Colors.White), 50);
            var materialGroup = new MaterialGroup();
            materialGroup.Children.Add(material);
            materialGroup.Children.Add(specular);

            var geometry = new GeometryModel3D
            {
                Geometry = sphere.Geometry,
                Material = materialGroup,
                BackMaterial = materialGroup
            };

            dnaGroup.Children.Add(geometry);
        }

        /// <summary>
        /// Tạo liên kết (cylinder) giữa 2 điểm
        /// </summary>
        private void CreateBond(Point3D start, Point3D end, double radius, System.Windows.Media.Color color)
        {
            var direction = end - start;
            var length = direction.Length;
            direction.Normalize();

            var cylinder = CreateCylinder(radius, length);
            
            var material = new DiffuseMaterial(new SolidColorBrush(color));
            
            var geometry = new GeometryModel3D
            {
                Geometry = cylinder,
                Material = material,
                BackMaterial = material
            };

            // Transform để định vị và xoay cylinder
            var transformGroup = new Transform3DGroup();
            
            // Xoay cylinder theo hướng
            var axis = Vector3D.CrossProduct(new Vector3D(0, 1, 0), direction);
            if (axis.Length > 0.001)
            {
                var angle = Math.Acos(Vector3D.DotProduct(new Vector3D(0, 1, 0), direction));
                transformGroup.Children.Add(new RotateTransform3D(
                    new AxisAngleRotation3D(axis, angle * 180 / Math.PI)));
            }
            
            // Dịch chuyển đến vị trí giữa start và end
            var center = new Point3D(
                (start.X + end.X) / 2,
                (start.Y + end.Y) / 2,
                (start.Z + end.Z) / 2
            );
            transformGroup.Children.Add(new TranslateTransform3D(
                center.X, center.Y, center.Z));

            geometry.Transform = transformGroup;
            dnaGroup.Children.Add(geometry);
        }

        /// <summary>
        /// Tạo hình trụ
        /// </summary>
        private MeshGeometry3D CreateCylinder(double radius, double height)
        {
            var mesh = new MeshGeometry3D();
            int segments = 12;

            // Bottom and top circles
            for (int i = 0; i <= segments; i++)
            {
                double angle = (i / (double)segments) * 2 * Math.PI;
                double x = radius * Math.Cos(angle);
                double z = radius * Math.Sin(angle);

                mesh.Positions.Add(new Point3D(x, -height / 2, z));
                mesh.Positions.Add(new Point3D(x, height / 2, z));
            }

            // Create triangles
            for (int i = 0; i < segments; i++)
            {
                int bottom1 = i * 2;
                int top1 = bottom1 + 1;
                int bottom2 = (i + 1) * 2;
                int top2 = bottom2 + 1;

                mesh.TriangleIndices.Add(bottom1);
                mesh.TriangleIndices.Add(top1);
                mesh.TriangleIndices.Add(bottom2);

                mesh.TriangleIndices.Add(top1);
                mesh.TriangleIndices.Add(top2);
                mesh.TriangleIndices.Add(bottom2);
            }

            return mesh;
        }

        /// <summary>
        /// Setup animation xoay
        /// </summary>
        private void SetupRotationAnimation()
        {
            rotationTransform = new RotateTransform3D
            {
                Rotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0)
            };

            dnaGroup.Transform = rotationTransform;

            var animation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(10),
                RepeatBehavior = RepeatBehavior.Forever
            };

            animationStoryboard = new Storyboard();
            Storyboard.SetTarget(animation, rotationTransform.Rotation);
            Storyboard.SetTargetProperty(animation, 
                new PropertyPath(AxisAngleRotation3D.AngleProperty));
            animationStoryboard.Children.Add(animation);
            animationStoryboard.Begin();
        }

        /// <summary>
        /// Điều chỉnh tốc độ animation
        /// </summary>
        public void SetAnimationSpeed(double speed)
        {
            if (animationStoryboard != null)
            {
                animationStoryboard.SetSpeedRatio(speed);
            }
        }

        /// <summary>
        /// Toggle animation
        /// </summary>
        public void ToggleAnimation()
        {
            if (animationStoryboard != null)
            {
                if (IsAnimating)
                {
                    animationStoryboard.Pause();
                }
                else
                {
                    animationStoryboard.Resume();
                }
                IsAnimating = !IsAnimating;
            }
        }

        /// <summary>
        /// Tạo DNA với sequence cụ thể (A, T, G, C)
        /// </summary>
        public Model3DGroup GenerateWithSequence(string sequence, double height = 10.0)
        {
            // TODO: Implement custom sequence generation
            // Hiện tại dùng Generate() mặc định
            return Generate(sequence.Length, height);
        }
    }

    /// <summary>
    /// Helper class cho SphereVisual3D
    /// </summary>
    internal class SphereVisual3D
    {
        public double Radius { get; set; }
        public Point3D Center { get; set; }
        public int ThetaDiv { get; set; }
        public int PhiDiv { get; set; }

        public MeshGeometry3D Geometry
        {
            get
            {
                var mesh = new MeshGeometry3D();
                
                for (int phi = 0; phi <= PhiDiv; phi++)
                {
                    double phiAngle = phi * Math.PI / PhiDiv;
                    double y = Radius * Math.Cos(phiAngle);
                    double r = Radius * Math.Sin(phiAngle);

                    for (int theta = 0; theta <= ThetaDiv; theta++)
                    {
                        double thetaAngle = theta * 2 * Math.PI / ThetaDiv;
                        double x = r * Math.Cos(thetaAngle);
                        double z = r * Math.Sin(thetaAngle);

                        mesh.Positions.Add(new Point3D(
                            Center.X + x, 
                            Center.Y + y, 
                            Center.Z + z));
                    }
                }

                for (int phi = 0; phi < PhiDiv; phi++)
                {
                    for (int theta = 0; theta < ThetaDiv; theta++)
                    {
                        int first = phi * (ThetaDiv + 1) + theta;
                        int second = first + ThetaDiv + 1;

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
        }
    }
}
