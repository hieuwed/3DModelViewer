using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace _3DModelViewer.Services
{
    /// <summary>
    /// Service for generating 3D Solar System model with 9 planets and animation
    /// </summary>
    public class SolarSystemGenerator
    {
        /// <summary>
        /// Planet data structure
        /// </summary>
        public struct PlanetData
        {
            public string Name { get; set; }
            public double Radius { get; set; }
            public double OrbitalRadius { get; set; }
            public double OrbitalPeriod { get; set; }
            public System.Windows.Media.Color Color { get; set; }
            public double RotationSpeed { get; set; }
        }

        // Dictionary to store animation data for each planet visual
        private static Dictionary<ModelVisual3D, PlanetAnimationData> _animationDataMap = new();

        /// <summary>
        /// Generate solar system with 9 planets
        /// Returns a ModelVisual3D containing the entire solar system
        /// </summary>
        public static ModelVisual3D GenerateSolarSystem()
        {
            _animationDataMap.Clear();
            
            var rootVisual = new ModelVisual3D();
            var solarSystem = new Model3DGroup();

            // Create Sun (center)
            var sun = CreateSphere(0.5, 32);
            var sunModel = new GeometryModel3D
            {
                Geometry = sun,
                Material = new DiffuseMaterial(new SolidColorBrush(System.Windows.Media.Colors.Yellow)),
                BackMaterial = new DiffuseMaterial(new SolidColorBrush(System.Windows.Media.Colors.Yellow))
            };
            solarSystem.Children.Add(sunModel);

            // Define planets data
            PlanetData[] planets = new PlanetData[]
            {
                new PlanetData { Name = "Sao Thủy", Radius = 0.08, OrbitalRadius = 1.5, OrbitalPeriod = 0.24, Color = System.Windows.Media.Colors.Gray, RotationSpeed = 0.04 },
                new PlanetData { Name = "Sao Kim", Radius = 0.14, OrbitalRadius = 2.5, OrbitalPeriod = 0.62, Color = System.Windows.Media.Colors.Orange, RotationSpeed = 0.015 },
                new PlanetData { Name = "Trái Đất", Radius = 0.15, OrbitalRadius = 3.5, OrbitalPeriod = 1.0, Color = System.Windows.Media.Colors.CornflowerBlue, RotationSpeed = 1.0 },
                new PlanetData { Name = "Sao Hỏa", Radius = 0.12, OrbitalRadius = 4.5, OrbitalPeriod = 1.88, Color = System.Windows.Media.Colors.Red, RotationSpeed = 0.97 },
                new PlanetData { Name = "Sao Mộc", Radius = 0.35, OrbitalRadius = 6.0, OrbitalPeriod = 11.86, Color = System.Windows.Media.Colors.BurlyWood, RotationSpeed = 2.44 },
                new PlanetData { Name = "Sao Thổ", Radius = 0.30, OrbitalRadius = 7.5, OrbitalPeriod = 29.46, Color = System.Windows.Media.Colors.PaleGoldenrod, RotationSpeed = 2.27 },
                new PlanetData { Name = "Sao Thiên Vương", Radius = 0.20, OrbitalRadius = 8.8, OrbitalPeriod = 84.01, Color = System.Windows.Media.Colors.CornflowerBlue, RotationSpeed = 1.40 },
                new PlanetData { Name = "Sao Hải Vương", Radius = 0.19, OrbitalRadius = 9.8, OrbitalPeriod = 164.79, Color = System.Windows.Media.Colors.RoyalBlue, RotationSpeed = 1.49 },
                new PlanetData { Name = "Sao Diêm Vương", Radius = 0.06, OrbitalRadius = 10.8, OrbitalPeriod = 248.0, Color = System.Windows.Media.Colors.DarkGray, RotationSpeed = 6.39 }
            };

            // Create planets with orbital paths
            for (int i = 0; i < planets.Length; i++)
            {
                var planet = planets[i];

                // Create planet sphere
                var mesh = CreateSphere(planet.Radius, 16);
                var planetMaterial = new DiffuseMaterial(new SolidColorBrush(planet.Color));
                var geometryModel = new GeometryModel3D
                {
                    Geometry = mesh,
                    Material = planetMaterial,
                    BackMaterial = planetMaterial
                };

                // Create orbital path visualization
                var orbitalPath = CreateOrbitalPath(planet.OrbitalRadius);
                var pathMaterial = new DiffuseMaterial(new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 255, 255, 255)));
                var pathModel = new GeometryModel3D
                {
                    Geometry = orbitalPath,
                    Material = pathMaterial,
                    BackMaterial = pathMaterial
                };
                solarSystem.Children.Add(pathModel);

                // Add planet to system as ModelVisual3D directly to rootVisual
                var planetVisual = new ModelVisual3D { Content = geometryModel };
                rootVisual.Children.Add(planetVisual);

                // Store planet data in dictionary for animation
                _animationDataMap[planetVisual] = new PlanetAnimationData
                {
                    PlanetData = planet,
                    Visual = planetVisual,
                    OrbitalAngle = 0,
                    RotationAngle = 0
                };
            }

            // Add sun group (with all orbital paths) to root
            var sunVisual = new ModelVisual3D { Content = solarSystem };
            rootVisual.Children.Add(sunVisual);
            
            return rootVisual;
        }

        /// <summary>
        /// Create a sphere mesh
        /// </summary>
        private static MeshGeometry3D CreateSphere(double radius, int segments = 32)
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

                    mesh.Positions.Add(new Point3D(x * radius, y * radius, z * radius));
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

        /// <summary>
        /// Create orbital path visualization
        /// </summary>
        private static MeshGeometry3D CreateOrbitalPath(double radius, int segments = 100)
        {
            var mesh = new MeshGeometry3D();
            double pathWidth = 0.02;

            for (int i = 0; i <= segments; i++)
            {
                double angle = i * 2 * Math.PI / segments;
                double x = radius * Math.Cos(angle);
                double z = radius * Math.Sin(angle);

                // Create line using thin rectangles
                mesh.Positions.Add(new Point3D(x - pathWidth, 0, z - pathWidth));
                mesh.Positions.Add(new Point3D(x + pathWidth, 0, z + pathWidth));
            }

            // Create indices for line segments
            for (int i = 0; i < segments; i++)
            {
                int a = i * 2;
                int b = a + 1;
                int c = a + 2;
                int d = b + 2;

                mesh.TriangleIndices.Add(a);
                mesh.TriangleIndices.Add(b);
                mesh.TriangleIndices.Add(c);

                mesh.TriangleIndices.Add(b);
                mesh.TriangleIndices.Add(d);
                mesh.TriangleIndices.Add(c);
            }

            return mesh;
        }

        /// <summary>
        /// Update planet positions and rotations for animation
        /// </summary>
        public static void UpdateAnimation(double deltaTime, double animationSpeed)
        {
            // Animation speed factor for normal viewing speed
            const double SPEED_FACTOR = 0.5;
            
            // Update each planet in the animation map
            foreach (var kvp in _animationDataMap)
            {
                var planetVisual = kvp.Key;
                var animData = kvp.Value;

                // Update orbital angle (slower movement around sun)
                animData.OrbitalAngle += (360.0 / animData.PlanetData.OrbitalPeriod) * deltaTime * animationSpeed * SPEED_FACTOR;
                if (animData.OrbitalAngle >= 360)
                    animData.OrbitalAngle -= 360;

                // Update rotation angle (slower rotation on own axis)
                animData.RotationAngle += animData.PlanetData.RotationSpeed * deltaTime * animationSpeed * SPEED_FACTOR;
                if (animData.RotationAngle >= 360)
                    animData.RotationAngle -= 360;

                // Create transform group
                var transformGroup = new Transform3DGroup();

                // Rotation transform (spin on own axis)
                var rotationTransform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), animData.RotationAngle));
                transformGroup.Children.Add(rotationTransform);

                // Orbital position transform
                double radians = animData.OrbitalAngle * Math.PI / 180.0;
                double x = animData.PlanetData.OrbitalRadius * Math.Cos(radians);
                double z = animData.PlanetData.OrbitalRadius * Math.Sin(radians);
                var translationTransform = new TranslateTransform3D(x, 0, z);
                transformGroup.Children.Add(translationTransform);

                // Apply transforms
                planetVisual.Transform = transformGroup;
            }
        }

        /// <summary>
        /// Get current planet positions and names for text labels
        /// </summary>
        public static List<(string Name, Point3D Position)> GetPlanetPositions()
        {
            var positions = new List<(string, Point3D)>();
            
            // Add Sun at center
            positions.Add(("Mặt Trời", new Point3D(0, 0, 0)));
            
            // Add planets
            foreach (var kvp in _animationDataMap)
            {
                var animData = kvp.Value;
                if (animData.Visual != null)
                {
                    // Calculate position based on orbital angle
                    double radians = animData.OrbitalAngle * Math.PI / 180.0;
                    double x = animData.PlanetData.OrbitalRadius * Math.Cos(radians);
                    double z = animData.PlanetData.OrbitalRadius * Math.Sin(radians);
                    positions.Add((animData.PlanetData.Name, new Point3D(x, 0, z)));
                }
            }
            
            return positions;
        }
    }

    /// <summary>
    /// Helper class for storing animation data per planet
    /// </summary>
    public class PlanetAnimationData
    {
        public SolarSystemGenerator.PlanetData PlanetData { get; set; }
        public ModelVisual3D? Visual { get; set; }
        public double OrbitalAngle { get; set; }
        public double RotationAngle { get; set; }
    }
}
