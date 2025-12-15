using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using HelixToolkit.Wpf;
using _3DModelViewer.Models;
using _3DModelViewer.Services;
using WinForms = System.Windows.Forms;

namespace _3DModelViewer
{
    /// <summary>
    /// 3D Model Viewer - Main Window
    /// Compatible với .NET 8 và AssimpNet 5.0.0-beta1
    /// </summary>
    public partial class MainWindow : Window
    {
        private FileLoaderService _fileLoader;
        private List<Model3DFile> _loadedFiles;
        private List<Model3DFile> _allFiles;
        private Model3DFile? _currentModel;
        private ModelVisual3D? _currentModelVisual;
        private ViewerSettings _settings;

        // Transform groups
        private Transform3DGroup _modelTransform;
        private RotateTransform3D _rotateX;
        private RotateTransform3D _rotateY;
        private RotateTransform3D _rotateZ;
        private ScaleTransform3D _scale;

        // Solar System Animation
        private DispatcherTimer? _animationTimer;
        private bool _isAnimationRunning = false;
        private Dictionary<string, TextBlock> _planetLabels = new();

        public MainWindow()
        {
            InitializeComponent();
            Initialize();
        }

        #region Initialization

        private void Initialize()
        {
            _fileLoader = new FileLoaderService();
            _loadedFiles = new List<Model3DFile>();
            _allFiles = new List<Model3DFile>();
            _settings = new ViewerSettings();

            // Initialize transforms
            InitializeTransforms();

            // Set initial status
            UpdateStatus("Sẵn sàng - Vui lòng mở file 3D");
        }

        private void InitializeTransforms()
        {
            _modelTransform = new Transform3DGroup();

            _rotateX = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 0));
            _rotateY = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0));
            _rotateZ = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), 0));
            _scale = new ScaleTransform3D(1, 1, 1);

            _modelTransform.Children.Add(_rotateX);
            _modelTransform.Children.Add(_rotateY);
            _modelTransform.Children.Add(_rotateZ);
            _modelTransform.Children.Add(_scale);
        }

        #endregion

        #region File Operations

        private async void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Chọn file 3D",
                Filter = FileLoaderService.GetFileDialogFilter(),
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
            {
                await LoadFilesAsync(dialog.FileNames);
            }
        }

        private async void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new WinForms.FolderBrowserDialog
            {
                Description = "Chọn thư mục chứa file 3D"
            };

            if (dialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                try
                {
                    UpdateStatus("Đang quét thư mục...");

                    var files = await _fileLoader.GetFilesFromDirectoryAsync(dialog.SelectedPath);

                    _allFiles.Clear();
                    _allFiles.AddRange(files);
                    _loadedFiles = new List<Model3DFile>(_allFiles);

                    FileListBox.ItemsSource = null;
                    FileListBox.ItemsSource = _loadedFiles;

                    UpdateStatus($"Đã tìm thấy {files.Count} file 3D");
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Lỗi khi quét thư mục: {ex.Message}",
                                  "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    UpdateStatus("Lỗi khi quét thư mục");
                }
            }
        }

        private async System.Threading.Tasks.Task LoadFilesAsync(string[] filePaths)
        {
            try
            {
                UpdateStatus($"Đang tải {filePaths.Length} file...");

                foreach (var filePath in filePaths)
                {
                    if (FileLoaderService.IsSupported(filePath))
                    {
                        var model = new Model3DFile(filePath);
                        _allFiles.Add(model);
                    }
                }

                _loadedFiles = new List<Model3DFile>(_allFiles);
                FileListBox.ItemsSource = null;
                FileListBox.ItemsSource = _loadedFiles;

                // Load first file
                if (_loadedFiles.Count > 0)
                {
                    FileListBox.SelectedIndex = 0;
                }

                UpdateStatus($"Đã tải {_loadedFiles.Count} file");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi tải file: {ex.Message}",
                              "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateStatus("Lỗi khi tải file");
            }
        }

        private async void FileListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FileListBox.SelectedItem is Model3DFile selectedFile)
            {
                await LoadAndDisplayModelAsync(selectedFile);
            }
        }

        private async System.Threading.Tasks.Task LoadAndDisplayModelAsync(Model3DFile modelFile)
        {
            try
            {
                UpdateStatus($"Đang tải model: {modelFile.FileName}...");

                // Load model data if not loaded
                if (!modelFile.IsLoaded)
                {
                    modelFile = await _fileLoader.LoadModelAsync(modelFile);
                }

                _currentModel = modelFile;

                // Clear previous model
                ClearCurrentModel();

                // Convert Assimp scene to WPF 3D
                _currentModelVisual = ConvertAssimpSceneToWpf(modelFile.AssimpScene!);
                _currentModelVisual.Transform = _modelTransform;

                // Add to viewport
                Viewport3D.Children.Add(_currentModelVisual);

                // Zoom to fit
                Viewport3D.ZoomExtents();

                // Update UI
                UpdateModelInfo(modelFile);
                UpdateStatus($"Đã tải: {modelFile.FileName}");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi hiển thị model: {ex.Message}",
                              "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateStatus($"Lỗi: {ex.Message}");
            }
        }

        private void ClearCurrentModel()
        {
            if (_currentModelVisual != null)
            {
                Viewport3D.Children.Remove(_currentModelVisual);
                _currentModelVisual = null;
            }
        }

        #endregion

        #region Model Conversion

        private ModelVisual3D ConvertAssimpSceneToWpf(Assimp.Scene scene)
        {
            var modelGroup = new Model3DGroup();

            try
            {
                // Process each mesh
                for (int i = 0; i < scene.MeshCount; i++)
                {
                    var mesh = scene.Meshes[i];
                    var geometry = ConvertMeshToGeometry(mesh);

                    // Get material
                    Material material = new DiffuseMaterial(System.Windows.Media.Brushes.Gray);
                    if (mesh.MaterialIndex >= 0 && mesh.MaterialIndex < scene.MaterialCount)
                    {
                        material = ConvertMaterialToWpf(scene.Materials[mesh.MaterialIndex]);
                    }

                    var geometryModel = new GeometryModel3D
                    {
                        Geometry = geometry,
                        Material = material,
                        BackMaterial = material
                    };

                    modelGroup.Children.Add(geometryModel);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting scene to WPF: {ex.Message}", ex);
            }

            return new ModelVisual3D { Content = modelGroup };
        }

        private MeshGeometry3D ConvertMeshToGeometry(Assimp.Mesh mesh)
        {
            var geometry = new MeshGeometry3D();

            try
            {
                // Vertices
                foreach (var vertex in mesh.Vertices)
                {
                    geometry.Positions.Add(new Point3D(vertex.X, vertex.Y, vertex.Z));
                }

                // Normals
                if (mesh.HasNormals)
                {
                    foreach (var normal in mesh.Normals)
                    {
                        geometry.Normals.Add(new Vector3D(normal.X, normal.Y, normal.Z));
                    }
                }

                // Texture coordinates
                if (mesh.HasTextureCoords(0))
                {
                    foreach (var texCoord in mesh.TextureCoordinateChannels[0])
                    {
                        geometry.TextureCoordinates.Add(new System.Windows.Point(texCoord.X, texCoord.Y));
                    }
                }

                // Indices
                foreach (var face in mesh.Faces)
                {
                    if (face.IndexCount == 3)
                    {
                        geometry.TriangleIndices.Add(face.Indices[0]);
                        geometry.TriangleIndices.Add(face.Indices[1]);
                        geometry.TriangleIndices.Add(face.Indices[2]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting mesh to geometry: {ex.Message}", ex);
            }

            return geometry;
        }

        private Material ConvertMaterialToWpf(Assimp.Material assimpMaterial)
        {
            var materialGroup = new MaterialGroup();

            try
            {
                // Diffuse color
                if (assimpMaterial.HasColorDiffuse)
                {
                    var color = assimpMaterial.ColorDiffuse;
                    var brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                        (byte)(color.A * 255),
                        (byte)(color.R * 255),
                        (byte)(color.G * 255),
                        (byte)(color.B * 255)
                    ));

                    materialGroup.Children.Add(new DiffuseMaterial(brush));
                }
                else
                {
                    // Default gray material
                    materialGroup.Children.Add(new DiffuseMaterial(System.Windows.Media.Brushes.LightGray));
                }

                // Specular
                if (assimpMaterial.HasColorSpecular)
                {
                    var color = assimpMaterial.ColorSpecular;
                    var brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                        (byte)(color.A * 255),
                        (byte)(color.R * 255),
                        (byte)(color.G * 255),
                        (byte)(color.B * 255)
                    ));

                    materialGroup.Children.Add(new SpecularMaterial(brush, assimpMaterial.Shininess));
                }
            }
            catch
            {
                // Return default material on error
                return new DiffuseMaterial(System.Windows.Media.Brushes.Gray);
            }

            return materialGroup;
        }

        #endregion

        #region UI Updates

        private void UpdateModelInfo(Model3DFile model)
        {
            FileNameText.Text = model.FileName;
            VertexCountText.Text = model.VertexCount.ToString("N0");
            FaceCountText.Text = model.FaceCount.ToString("N0");
            MeshCountText.Text = model.MeshCount.ToString("N0");
            FileSizeText.Text = model.GetFileSizeString();
        }

        private void UpdateStatus(string message)
        {
            if (StatusText != null)
                StatusText.Text = message;
        }

        private void UpdateCameraPosition()
        {
            var pos = Camera.Position;
            CameraPositionText.Text = $"Camera: ({pos.X:F1}, {pos.Y:F1}, {pos.Z:F1})";
        }

        #endregion

        #region Camera Controls

        private void ResetCamera_Click(object sender, RoutedEventArgs e)
        {
            Viewport3D.ZoomExtents();
            UpdateStatus("Đã reset camera");
            UpdateCameraPosition();
        }

        private void ViewFront_Click(object sender, RoutedEventArgs e)
        {
            var distance = 10.0;
            Camera.Position = new Point3D(0, 0, distance);
            Camera.LookDirection = new Vector3D(0, 0, -distance);
            Camera.UpDirection = new Vector3D(0, 1, 0);
            UpdateCameraPosition();
        }

        private void ViewTop_Click(object sender, RoutedEventArgs e)
        {
            var distance = 10.0;
            Camera.Position = new Point3D(0, distance, 0);
            Camera.LookDirection = new Vector3D(0, -distance, 0);
            Camera.UpDirection = new Vector3D(0, 0, -1);
            UpdateCameraPosition();
        }

        private void ViewSide_Click(object sender, RoutedEventArgs e)
        {
            var distance = 10.0;
            Camera.Position = new Point3D(distance, 0, 0);
            Camera.LookDirection = new Vector3D(-distance, 0, 0);
            Camera.UpDirection = new Vector3D(0, 1, 0);
            UpdateCameraPosition();
        }

        private void FovSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Camera != null && FovValueText != null)
            {
                Camera.FieldOfView = e.NewValue;
                FovValueText.Text = $"{e.NewValue:F0}°";
            }
        }

        #endregion

        #region Transform Controls

        private void RotateX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_rotateX != null)
            {
                ((AxisAngleRotation3D)_rotateX.Rotation).Angle = e.NewValue;
            }
        }

        private void RotateY_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_rotateY != null)
            {
                ((AxisAngleRotation3D)_rotateY.Rotation).Angle = e.NewValue;
            }
        }

        private void RotateZ_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_rotateZ != null)
            {
                ((AxisAngleRotation3D)_rotateZ.Rotation).Angle = e.NewValue;
            }
        }

        private void Scale_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_scale != null)
            {
                _scale.ScaleX = e.NewValue;
                _scale.ScaleY = e.NewValue;
                _scale.ScaleZ = e.NewValue;
            }
        }

        private void ResetTransform_Click(object sender, RoutedEventArgs e)
        {
            RotateXSlider.Value = 0;
            RotateYSlider.Value = 0;
            RotateZSlider.Value = 0;
            ScaleSlider.Value = 1;
            UpdateStatus("Đã reset transform");
        }

        #endregion

        #region Render Settings

        private void ShowWireframe_Changed(object sender, RoutedEventArgs e)
        {
            if (ShowWireframeCheck != null)
                UpdateStatus($"Wireframe: {(ShowWireframeCheck.IsChecked == true ? "Bật" : "Tắt")}");
        }

        private void ShowGrid_Changed(object sender, RoutedEventArgs e)
        {
            if (GridLines != null)
            {
                GridLines.Visible = ShowGridCheck.IsChecked == true;
                UpdateStatus($"Grid: {(ShowGridCheck.IsChecked == true ? "Bật" : "Tắt")}");
            }
        }

        private void ShowAxes_Changed(object sender, RoutedEventArgs e)
        {
            if (Viewport3D != null)
            {
                Viewport3D.ShowCoordinateSystem = ShowAxesCheck.IsChecked == true;
                UpdateStatus($"Coordinate System: {(ShowAxesCheck.IsChecked == true ? "Bật" : "Tắt")}");
            }
        }

        private void EnableLighting_Changed(object sender, RoutedEventArgs e)
        {
            if (EnableLightingCheck != null)
                UpdateStatus($"Lighting: {(EnableLightingCheck.IsChecked == true ? "Bật" : "Tắt")}");
        }

        private void BackgroundColor_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (BackgroundColorCombo.SelectedItem is ComboBoxItem item && Viewport3D != null)
            {
                string? colorHex = item.Tag.ToString();
                if (!string.IsNullOrEmpty(colorHex))
                {
                    try
                    {
                        var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(colorHex);
                        Viewport3D.Background = new SolidColorBrush(color);
                        UpdateStatus("Đã đổi màu nền");
                    }
                    catch
                    {
                        // Ignore color conversion errors
                    }
                }
            }
        }

        #endregion

        #region Toolbar Actions

        private void ToggleLighting_Click(object sender, RoutedEventArgs e)
        {
            EnableLightingCheck.IsChecked = !EnableLightingCheck.IsChecked;
        }

        private void ToggleGrid_Click(object sender, RoutedEventArgs e)
        {
            ShowGridCheck.IsChecked = !ShowGridCheck.IsChecked;
        }

        private void TakeScreenshot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PNG Image|*.png|JPEG Image|*.jpg|BMP Image|*.bmp",
                    FileName = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}"
                };

                if (dialog.ShowDialog() == true)
                {
                    Viewport3D.Export(dialog.FileName);
                    UpdateStatus($"Đã lưu screenshot: {Path.GetFileName(dialog.FileName)}");

                    var result = System.Windows.MessageBox.Show("Đã lưu screenshot. Bạn có muốn mở thư mục chứa file?",
                                                "Thành công",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{dialog.FileName}\"");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi lưu screenshot: {ex.Message}",
                              "Lỗi",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        #endregion

        #region Search Functionality

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Tìm kiếm...")
            {
                SearchBox.Text = "";
                SearchBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = "Tìm kiếm...";
                SearchBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Skip if not initialized yet
            if (_allFiles == null || FileListBox == null)
                return;

            if (string.IsNullOrWhiteSpace(SearchBox.Text) || SearchBox.Text == "Tìm kiếm...")
            {
                _loadedFiles = new List<Model3DFile>(_allFiles);
            }
            else
            {
                var searchText = SearchBox.Text.ToLower();
                _loadedFiles = _allFiles.Where(f =>
                    f.FileName.ToLower().Contains(searchText) ||
                    f.FileExtension.ToLower().Contains(searchText)
                ).ToList();
            }

            FileListBox.ItemsSource = null;
            FileListBox.ItemsSource = _loadedFiles;

            UpdateStatus($"Tìm thấy {_loadedFiles.Count}/{_allFiles.Count} file");
        }

        #endregion

        #region Window Closing

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Cleanup
            ClearCurrentModel();
            Viewport3D.Children.Clear();
            _fileLoader?.Dispose();

            // Force GC
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        #endregion
        #region Sample Models
        private void LoadSampleModel_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is string modelType)
            {
                try
                {
                    if (modelType == "Hệ mặt trời")
                    {
                        LoadSolarSystem();
                        return;
                    }

                    UpdateStatus($"Đang tạo {modelType}...");
                    // Generate the mesh based on model type
                    MeshGeometry3D mesh = modelType switch
                    {
                        "Hình lập phương" => ProceduralModelGenerator.GenerateCube(2.0),
                        "Hình cầu" => ProceduralModelGenerator.GenerateSphere(1.5, 32),
                        "Hình trụ" => ProceduralModelGenerator.GenerateCylinder(1.0, 2.0, 32),
                        "Hình nón" => ProceduralModelGenerator.GenerateCone(1.0, 2.0, 32),
                        "Hình chóp" => ProceduralModelGenerator.GeneratePyramid(2.0),
                        _ => ProceduralModelGenerator.GenerateCube(2.0)
                    };
                    // Create material
                    var material = new DiffuseMaterial(System.Windows.Media.Brushes.LightBlue);
                    var specular = new SpecularMaterial(System.Windows.Media.Brushes.White, 30);
                    var materialGroup = new MaterialGroup();
                    materialGroup.Children.Add(material);
                    materialGroup.Children.Add(specular);
                    // Create geometry model
                    var geometryModel = new GeometryModel3D
                    {
                        Geometry = mesh,
                        Material = materialGroup,
                        BackMaterial = materialGroup
                    };
                    // Clear previous model
                    ClearCurrentModel();
                    // Create model visual
                    var modelGroup = new Model3DGroup();
                    modelGroup.Children.Add(geometryModel);
                    _currentModelVisual = new ModelVisual3D { Content = modelGroup };
                    _currentModelVisual.Transform = _modelTransform;
                    // Add to viewport
                    Viewport3D.Children.Add(_currentModelVisual);
                    // Zoom to fit
                    Viewport3D.ZoomExtents();
                    // Create sample model info
                    var sampleModel = new Model3DFile(
                        modelType,
                        mesh.Positions.Count,
                        mesh.TriangleIndices.Count / 3
                    );
                    _currentModel = sampleModel;
                    // Update UI
                    UpdateModelInfo(sampleModel);
                    UpdateStatus($"Đã tạo {modelType}");
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Lỗi khi tạo mô hình: {ex.Message}",
                                  "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    UpdateStatus($"Lỗi: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Load and display solar system
        /// </summary>
        private void LoadSolarSystem()
        {
            try
            {
                UpdateStatus("Đang tạo Hệ mặt trời...");

                // Stop previous animation
                StopAnimation();

                // Clear previous model
                ClearCurrentModel();

                // Generate solar system
                _currentModelVisual = SolarSystemGenerator.GenerateSolarSystem();

                // Add to viewport
                Viewport3D.Children.Add(_currentModelVisual);

                // Create planet name labels
                _planetLabels.Clear();
                CreatePlanetLabels();

                // Zoom to fit
                Viewport3D.ZoomExtents();

                // Show animation controls
                AnimationControlsGroup.Visibility = Visibility.Visible;

                // Create sample model info
                var sampleModel = new Model3DFile("Hệ mặt trời", 50000, 1000);
                _currentModel = sampleModel;

                // Update UI
                UpdateModelInfo(sampleModel);
                UpdateStatus("Đã tạo Hệ mặt trời - Nhấn ▶️ để phát");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi tạo hệ mặt trời: {ex.Message}",
                              "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateStatus($"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Create TextBlock labels for each planet
        /// </summary>
        private void CreatePlanetLabels()
        {
            // Get planet positions to create labels for each
            var positions = SolarSystemGenerator.GetPlanetPositions();
            
            foreach (var (name, position) in positions)
            {
                var label = new TextBlock
                {
                    Text = name,
                    Foreground = System.Windows.Media.Brushes.White,
                    FontSize = 12,
                    FontWeight = System.Windows.FontWeights.Bold,
                    Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 0, 0, 0)),
                    Padding = new Thickness(4, 2, 4, 2),
                    TextAlignment = System.Windows.TextAlignment.Center
                };
                
                // Add to viewport canvas overlay
                Canvas.SetLeft(label, 0);
                Canvas.SetTop(label, 0);
                
                _planetLabels[name] = label;
            }
        }

        /// <summary>
        /// Update planet label positions based on 3D to 2D projection
        /// </summary>
        private void UpdatePlanetLabels()
        {
            if (_planetLabels.Count == 0 || PlanetLabelsCanvas == null || Viewport3D.Camera == null)
                return;

            var positions = SolarSystemGenerator.GetPlanetPositions();
            var perspectiveCamera = Viewport3D.Camera as PerspectiveCamera;
            
            if (perspectiveCamera == null)
                return;

            foreach (var (name, position3D) in positions)
            {
                if (_planetLabels.TryGetValue(name, out var label))
                {
                    try
                    {
                        // Project 3D point to 2D screen coordinates using camera and viewport dimensions
                        var screenPoint = ProjectPoint3D(position3D, perspectiveCamera, Viewport3D.ActualWidth, Viewport3D.ActualHeight);
                        
                        if (!double.IsNaN(screenPoint.X) && !double.IsNaN(screenPoint.Y) && 
                            screenPoint.X > -100 && screenPoint.X < Viewport3D.ActualWidth + 100 &&
                            screenPoint.Y > -100 && screenPoint.Y < Viewport3D.ActualHeight + 100)
                        {
                            // Add label to canvas if not already added
                            if (!PlanetLabelsCanvas.Children.Contains(label))
                            {
                                PlanetLabelsCanvas.Children.Add(label);
                            }
                            
                            // Update label position with small offset to avoid blocking the planet
                            Canvas.SetLeft(label, screenPoint.X - 30);
                            Canvas.SetTop(label, screenPoint.Y - 10);
                            
                            // Show label
                            label.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            // Hide label if it's outside viewport
                            label.Visibility = Visibility.Hidden;
                        }
                    }
                    catch
                    {
                        // If projection fails, hide the label
                        label.Visibility = Visibility.Hidden;
                    }
                }
            }
        }

        /// <summary>
        /// Project a 3D point to 2D screen coordinates
        /// </summary>
        private System.Windows.Point ProjectPoint3D(Point3D point3D, PerspectiveCamera camera, double viewportWidth, double viewportHeight)
        {
            // Create a projection matrix from camera parameters
            var aspectRatio = viewportWidth / viewportHeight;
            var fov = camera.FieldOfView * Math.PI / 180.0; // Convert to radians
            
            // Vector from camera to point
            var direction = point3D - camera.Position;
            var distance = direction.Length;
            
            if (distance == 0)
                return new System.Windows.Point(double.NaN, double.NaN);
            
            // Normalize direction
            direction.Normalize();
            
            // Look direction (forward)
            var look = camera.LookDirection;
            look.Normalize();
            
            // Calculate right vector
            var right = Vector3D.CrossProduct(look, camera.UpDirection);
            right.Normalize();
            
            // Recalculate up to ensure orthogonality
            var up = Vector3D.CrossProduct(right, look);
            up.Normalize();
            
            // Project point onto camera's view plane
            var lookComponent = Vector3D.DotProduct(direction, look);
            
            // Point is behind camera
            if (lookComponent <= 0)
                return new System.Windows.Point(double.NaN, double.NaN);
            
            var rightComponent = Vector3D.DotProduct(direction, right);
            var upComponent = Vector3D.DotProduct(direction, up);
            
            // Calculate normalized device coordinates
            var halfFovTan = Math.Tan(fov / 2.0);
            var x = (rightComponent / lookComponent) / (halfFovTan * aspectRatio);
            var y = -(upComponent / lookComponent) / halfFovTan;
            
            // Clamp to -1 to 1 range for view frustum
            x = Math.Max(-1, Math.Min(1, x));
            y = Math.Max(-1, Math.Min(1, y));
            
            // Convert to screen coordinates
            var screenX = (x + 1.0) * viewportWidth / 2.0;
            var screenY = (y + 1.0) * viewportHeight / 2.0;
            
            return new System.Windows.Point(screenX, screenY);
        }

        /// <summary>
        /// Play/Pause animation
        /// </summary>
        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (_currentModelVisual == null || _currentModel?.FileName != "Hệ mặt trời")
            {
                System.Windows.MessageBox.Show("Vui lòng tạo Hệ mặt trời trước",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (_isAnimationRunning)
            {
                StopAnimation();
            }
            else
            {
                StartAnimation();
            }
        }

        /// <summary>
        /// Start animation
        /// </summary>
        private void StartAnimation()
        {
            if (_animationTimer != null && _animationTimer.IsEnabled)
                return;

            _isAnimationRunning = true;
            PlayPauseButton.Content = "⏸️ Dừng";
            UpdateStatus("Animation đang chạy...");

            // Create animation timer
            _animationTimer = new DispatcherTimer();
            _animationTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
            _animationTimer.Tick += AnimationTimer_Tick;
            _animationTimer.Start();
        }

        /// <summary>
        /// Stop animation
        /// </summary>
        private void StopAnimation()
        {
            _isAnimationRunning = false;
            PlayPauseButton.Content = "▶️ Phát";
            UpdateStatus("Animation dừng");

            if (_animationTimer != null)
            {
                _animationTimer.Stop();
                _animationTimer = null;
            }
        }

        /// <summary>
        /// Animation timer tick
        /// </summary>
        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            if (_currentModelVisual == null)
                return;

            double animationSpeed = AnimationSpeedSlider.Value;
            const double DELTA_TIME = 0.016; // 16ms per frame (~60 FPS)

            // Update solar system animation with fixed delta time per frame
            SolarSystemGenerator.UpdateAnimation(DELTA_TIME, animationSpeed);
            
            // Update planet name labels
            UpdatePlanetLabels();
        }

        #endregion

        /// <summary>
        /// Handle animation speed slider change
        /// </summary>
        private void AnimationSpeedSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (AnimationSpeedText != null)
            {
                AnimationSpeedText.Text = $"{e.NewValue:F1}x";
            }
        }
    }
    
}