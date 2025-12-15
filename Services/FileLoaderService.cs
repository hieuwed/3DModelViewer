using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Assimp;
using _3DModelViewer.Models;

namespace _3DModelViewer.Services
{
    /// <summary>
    /// Service để load và quản lý file 3D
    /// Compatible với AssimpNet 5.0.0-beta1 và .NET 8
    /// </summary>
    public class FileLoaderService : IDisposable
    {
        // Các format 3D được hỗ trợ
        private static readonly string[] SupportedExtensions =
        {
            ".obj", ".fbx", ".dae", ".3ds", ".blend", ".stl",
            ".ply", ".gltf", ".glb", ".x3d", ".collada"
        };

        private readonly AssimpContext _assimpContext;
        private bool _disposed;

        public FileLoaderService()
        {
            _assimpContext = new AssimpContext();
        }

        /// <summary>
        /// Quét thư mục và lấy danh sách file 3D (Async)
        /// </summary>
        public async Task<List<Model3DFile>> GetFilesFromDirectoryAsync(string directoryPath)
        {
            return await Task.Run(() => GetFilesFromDirectory(directoryPath));
        }

        /// <summary>
        /// Quét thư mục và lấy danh sách file 3D (Sync)
        /// </summary>
        public List<Model3DFile> GetFilesFromDirectory(string directoryPath)
        {
            var files = new List<Model3DFile>();

            try
            {
                if (!Directory.Exists(directoryPath))
                    throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

                var allFiles = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories)
                    .Where(f => IsSupported(f))
                    .OrderBy(f => Path.GetFileName(f));

                foreach (var filePath in allFiles)
                {
                    try
                    {
                        var model3DFile = new Model3DFile(filePath);
                        files.Add(model3DFile);
                    }
                    catch (Exception ex)
                    {
                        // Skip files that cannot be added and continue with others
                        System.Diagnostics.Debug.WriteLine($"Could not add file {filePath}: {ex.Message}");
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedAccessException($"Access denied to directory: {directoryPath}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error scanning directory: {ex.Message}", ex);
            }

            return files;
        }

        /// <summary>
        /// Load file 3D model (Async)
        /// </summary>
        public async Task<Model3DFile> LoadModelAsync(Model3DFile modelFile)
        {
            return await Task.Run(() => LoadModel(modelFile));
        }

        /// <summary>
        /// Load file 3D model (Sync)
        /// Compatible với AssimpNet 5.0.0-beta1
        /// </summary>
        public Model3DFile LoadModel(Model3DFile modelFile)
        {
            try
            {
                if (!File.Exists(modelFile.FilePath))
                    throw new FileNotFoundException("File not found", modelFile.FilePath);

                // Import settings cho AssimpNet 5.0.0-beta1
                // Một số flags có thể không available trong beta version
                var importSettings = PostProcessSteps.Triangulate |
                                    PostProcessSteps.GenerateSmoothNormals |
                                    PostProcessSteps.FlipUVs |
                                    PostProcessSteps.JoinIdenticalVertices;

                // Load scene với error handling
                Scene? scene = null;
                try
                {
                    scene = _assimpContext.ImportFile(modelFile.FilePath, importSettings);
                }
                catch (AssimpException ex)
                {
                    throw new Exception($"Assimp error loading file: {ex.Message}", ex);
                }

                if (scene == null || !scene.HasMeshes)
                    throw new Exception("File does not contain valid 3D mesh data");

                modelFile.AssimpScene = scene;
                modelFile.MeshCount = scene.MeshCount;

                // Calculate statistics
                CalculateModelStatistics(modelFile, scene);

                modelFile.IsLoaded = true;
                modelFile.LoadError = null;

                return modelFile;
            }
            catch (Exception ex)
            {
                modelFile.IsLoaded = false;
                modelFile.LoadError = ex.Message;
                throw new Exception($"Error loading 3D model '{modelFile.FileName}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Tính toán thống kê model (vertices, faces, bounding box)
        /// </summary>
        private void CalculateModelStatistics(Model3DFile modelFile, Scene scene)
        {
            int totalVertices = 0;
            int totalFaces = 0;

            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (var mesh in scene.Meshes)
            {
                totalVertices += mesh.VertexCount;
                totalFaces += mesh.FaceCount;

                // Calculate bounding box
                foreach (var vertex in mesh.Vertices)
                {
                    min.X = Math.Min(min.X, vertex.X);
                    min.Y = Math.Min(min.Y, vertex.Y);
                    min.Z = Math.Min(min.Z, vertex.Z);

                    max.X = Math.Max(max.X, vertex.X);
                    max.Y = Math.Max(max.Y, vertex.Y);
                    max.Z = Math.Max(max.Z, vertex.Z);
                }
            }

            modelFile.VertexCount = totalVertices;
            modelFile.FaceCount = totalFaces;
            modelFile.MinBounds = min;
            modelFile.MaxBounds = max;
        }

        /// <summary>
        /// Kiểm tra xem file có được hỗ trợ không
        /// </summary>
        public static bool IsSupported(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            string ext = Path.GetExtension(filePath).ToLower();
            return SupportedExtensions.Contains(ext);
        }

        /// <summary>
        /// Lấy filter string cho OpenFileDialog
        /// </summary>
        public static string GetFileDialogFilter()
        {
            return "3D Models|" + string.Join(";", SupportedExtensions.Select(e => "*" + e)) +
                   "|All Files|*.*";
        }

        /// <summary>
        /// Lấy danh sách các extension được hỗ trợ
        /// </summary>
        public static string[] GetSupportedExtensions()
        {
            return SupportedExtensions.ToArray();
        }

        /// <summary>
        /// Dispose pattern implementation
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _assimpContext?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}