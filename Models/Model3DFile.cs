using System;
using System.IO;
using System.Numerics;
using System.Windows.Media.Imaging;
using Assimp;

namespace _3DModelViewer.Models
{
    /// <summary>
    /// Đại diện cho một file 3D model với metadata và thông tin hiển thị
    /// </summary>
    public class Model3DFile
    {
        // Basic file information
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileExtension { get; set; }
        public long FileSize { get; set; }
        public DateTime LastModified { get; set; }
        public BitmapImage? Thumbnail { get; set; }

        // 3D Model data
        public Scene? AssimpScene { get; set; }
        public int VertexCount { get; set; }
        public int FaceCount { get; set; }
        public int MeshCount { get; set; }

        // Bounding box
        public Vector3 MinBounds { get; set; }
        public Vector3 MaxBounds { get; set; }

        // Loading state
        public bool IsLoaded { get; set; }
        public string? LoadError { get; set; }

        // Sample model flag
        public bool IsSampleModel { get; set; }

        public Model3DFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be empty", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            FileExtension = Path.GetExtension(filePath).ToLower();

            var fileInfo = new FileInfo(filePath);
            FileSize = fileInfo.Length;
            LastModified = fileInfo.LastWriteTime;

            IsLoaded = false;
            IsSampleModel = false;
        }

        /// <summary>
        /// Constructor for procedurally generated sample models
        /// </summary>
        public Model3DFile(string name, int vertexCount, int faceCount)
        {
            FileName = name;
            FilePath = "[Sample Model]";
            FileExtension = ".sample";
            FileSize = 0;
            LastModified = DateTime.Now;
            VertexCount = vertexCount;
            FaceCount = faceCount;
            MeshCount = 1;
            IsLoaded = true;
            IsSampleModel = true;
        }

        /// <summary>
        /// Lấy kích thước file dưới dạng chuỗi dễ đọc
        /// </summary>
        public string GetFileSizeString()
        {
            if (IsSampleModel)
                return "Sample";

            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = FileSize;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// Lấy kích thước model (dimensions)
        /// </summary>
        public Vector3 GetDimensions()
        {
            return MaxBounds - MinBounds;
        }

        /// <summary>
        /// Lấy tâm của model
        /// </summary>
        public Vector3 GetCenter()
        {
            return (MinBounds + MaxBounds) / 2;
        }

        /// <summary>
        /// Lấy bán kính bounding sphere
        /// </summary>
        public float GetBoundingRadius()
        {
            var dimensions = GetDimensions();
            return dimensions.Length() / 2;
        }

        public override string ToString()
        {
            return $"{FileName} ({GetFileSizeString()}) - {VertexCount:N0} vertices";
        }
    }
}