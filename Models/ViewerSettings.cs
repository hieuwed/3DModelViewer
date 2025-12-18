using System.Windows.Media;

namespace _3DModelViewer.Models
{
    /// <summary>
    /// Cài đặt hiển thị cho 3D Viewer
    /// </summary>
    public class ViewerSettings
    {
        // Display settings
        public System.Windows.Media.Color BackgroundColor { get; set; } = System.Windows.Media.Color.FromRgb(149, 165, 166); // #95a5a6
        public bool ShowGrid { get; set; } = true;
        public bool ShowAxes { get; set; } = true;
        public bool ShowBoundingBox { get; set; } = false;
        public bool ShowViewCube { get; set; } = true;

        // Lighting settings
        public bool EnableLighting { get; set; } = true;
        public bool EnableShadows { get; set; } = false;

        // Render mode
        public bool ShowWireframe { get; set; } = false;

        // Performance settings
        public bool EnableAntialiasing { get; set; } = true;
        public int MaxFPS { get; set; } = 60;

        /// <summary>
        /// Đặt về cài đặt mặc định
        /// </summary>
        public void Reset()
        {
            BackgroundColor = System.Windows.Media.Color.FromRgb(149, 165, 166);
            ShowGrid = true;
            ShowAxes = true;
            ShowBoundingBox = false;
            ShowViewCube = true;
            EnableLighting = true;
            EnableShadows = false;
            ShowWireframe = false;
        }
    }
}