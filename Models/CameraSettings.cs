using System.Numerics;

namespace _3DModelViewer.Models
{
    /// <summary>
    /// Cài đặt camera cho 3D viewport
    /// </summary>
    public class CameraSettings
    {
        // Camera position and orientation
        public Vector3 Position { get; set; } = new Vector3(5, 5, 5);
        public Vector3 Target { get; set; } = new Vector3(0, 0, 0);
        public Vector3 UpVector { get; set; } = new Vector3(0, 1, 0);

        // Projection settings
        public double FieldOfView { get; set; } = 45.0;
        public double NearPlane { get; set; } = 0.1;
        public double FarPlane { get; set; } = 1000.0;

        // Camera movement speeds
        public double RotationSpeed { get; set; } = 1.0;
        public double ZoomSpeed { get; set; } = 0.1;
        public double PanSpeed { get; set; } = 0.01;

        // Animation
        public bool EnableSmoothTransition { get; set; } = true;
        public int TransitionDurationMs { get; set; } = 300;

        /// <summary>
        /// Đặt về vị trí mặc định
        /// </summary>
        public void Reset()
        {
            Position = new Vector3(5, 5, 5);
            Target = new Vector3(0, 0, 0);
            UpVector = new Vector3(0, 1, 0);
            FieldOfView = 45.0;
        }

        /// <summary>
        /// Lấy look direction từ position và target
        /// </summary>
        public Vector3 GetLookDirection()
        {
            return Target - Position;
        }

        /// <summary>
        /// Tính khoảng cách từ camera đến target
        /// </summary>
        public float GetDistance()
        {
            return Vector3.Distance(Position, Target);
        }
    }
}