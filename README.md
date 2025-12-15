# 📦 3D Model Viewer - .NET 8 WPF

<div align="center">

![version](https://img.shields.io/badge/version-1.0.0-blue)
![framework](https://img.shields.io/badge/.NET-8.0-blueviolet)
![platform](https://img.shields.io/badge/platform-Windows-success)

Ứng dụng xem và quản lý mô hình 3D được xây dựng bằng .NET 8 và WPF

[Tính Năng](#-tính-năng-chính) • [Cài Đặt](#-cài-đặt--chạy) • [Hướng Dẫn](#-hướng-dẫn-sử-dụng) • [Phát Triển](#-phát-triển)

</div>

---

## 📋 Mô Tả

**3D Model Viewer** là một ứng dụng Windows hiện đại cho phép bạn:
- 📂 Mở và xem các file mô hình 3D (FBX, OBJ, GLTF, DAE, STL, PLY, v.v.)
- 📁 Quét thư mục chứa nhiều file 3D cùng lúc
- 🎨 Tạo các mô hình hình học mẫu sẵn
- 🎮 Điều khiển camera linh hoạt (rotate, zoom, pan)
- 🔄 Biến đổi mô hình (rotation, scale)
- 💡 Kiểm soát lighting, wireframe, grid
- 📸 Chụp và lưu screenshot
- 🔍 Tìm kiếm file nhanh chóng
- 📊 Xem thông tin chi tiết mô hình (đỉnh, mặt, mesh)

---

## 🛠️ Công Nghệ & Stack

| Thành Phần | Chi Tiết |
|-----------|---------|
| **Framework** | .NET 8.0 |
| **UI** | WPF (Windows Presentation Foundation) |
| **3D Rendering** | HelixToolkit.Wpf |
| **Model Import** | AssimpNet 5.0.0-beta1 |
| **Language** | C# 12 |

---

## 📦 Yêu Cầu Hệ Thống

### Để Chạy Ứng Dụng
- Windows 7, 8, 10, 11 hoặc Server 2016+
- .NET 8 Runtime
- 100 MB RAM tối thiểu
- Card đồ họa hỗ trợ DirectX 9+

### Để Phát Triển
- Visual Studio 2022 (Community, Professional hoặc Enterprise)
- .NET 8 SDK
- NuGet Package Manager

---

## 🚀 Cài Đặt & Chạy

### Cách 1: Từ Visual Studio

**Bước 1:** Clone hoặc tải dự án
```bash
git clone [URL_REPO]
cd 3DModelViewer
```

**Bước 2:** Mở Solution
```
Nhấp đôi vào file: 3DModelViewer.sln
```

**Bước 3:** Restore NuGet Packages
```
Visual Studio sẽ tự động restore, hoặc:
Tools → NuGet Package Manager → Package Manager Console
Gõ: Update-Package
```

**Bước 4:** Build Project
```
Ctrl + Shift + B (Build Solution)
```

**Bước 5:** Chạy Ứng Dụng
```
F5 (Start Debugging)
hoặc
Ctrl + F5 (Start Without Debugging)
```

### Cách 2: Từ Command Line

```bash
# Clone repo
git clone [URL_REPO]
cd 3DModelViewer

# Restore dependencies
dotnet restore

# Build
dotnet build -c Release

# Run
dotnet run
```

### Cách 3: Publish & Distribute

```bash
# Tạo executable release
dotnet publish -c Release -o ./publish

# File sẽ nằm trong thư mục: ./publish
```

---

## 📁 Cấu Trúc Dự Án

```
3DModelViewer/
│
├── Models/                          # Lớp mô hình dữ liệu
│   ├── Model3DFile.cs              # Đại diện file 3D
│   └── ViewerSettings.cs            # Cài đặt ứng dụng
│
├── Services/                        # Logic nghiệp vụ
│   ├── FileLoaderService.cs         # Load file 3D
│   ├── ProceduralModelGenerator.cs  # Tạo mô hình mẫu
│   └── SolarSystemGenerator.cs      # Tạo hệ mặt trời
│
├── Converters/                      # XAML Value Converters
│   └── FileSizeConverter.cs         # Convert kích thước file
│
├── MainWindow.xaml                  # Giao diện chính (UI)
├── MainWindow.xaml.cs               # Logic giao diện
├── App.xaml                         # Cài đặt app
├── App.xaml.cs                      # Code-behind app
│
├── Properties/
│   └── AssemblyInfo.cs              # Thông tin assembly
│
├── 3DModelViewer.csproj             # Cấu hình project
├── 3DModelViewer.sln                # Solution file
├── .gitignore                       # Git ignore rules
├── README.md                        # File này
├── CHANGELOG.md                     # Lịch sử thay đổi
├── DEVELOPMENT.md                   # Hướng dẫn phát triển
└── BUILD_INSTRUCTIONS.md            # Hướng dẫn build
```

---

## ✨ Tính Năng Chính

### 1️⃣ Quản Lý File
- ✅ Mở file 3D đơn lẻ (Ctrl+O)
- ✅ Mở thư mục chứa nhiều file 3D
- ✅ Danh sách file với scroll và chi tiết
- ✅ Tìm kiếm nhanh theo tên hoặc extension

**Định Dạng Hỗ Trợ:**
```
.obj, .fbx, .dae, .3ds, .blend, .stl,
.ply, .gltf, .glb, .x3d, .collada
```

### 2️⃣ Mô Hình Mẫu
7 mô hình hình học có sẵn:
- 🟦 Hình lập phương (Cube)
- 🔵 Hình cầu (Sphere)
- 🥫 Hình trụ (Cylinder)
- 🔻 Hình nón (Cone)
- 🔺 Hình chóp (Pyramid)
- 📦 Khối chữ nhật (Cuboid)
- 🪐 Hệ mặt trời (Solar System)

### 3️⃣ Điều Khiển Camera
| Hành Động | Nút/Tổ Hợp |
|----------|-----------|
| Xoay mô hình | Chuột phải + kéo |
| Zoom in/out | Con lăn chuột |
| Pan (dịch chuyển) | Chuột giữa + kéo |
| Reset camera | Nút 🔄 hoặc Home |
| View Front | Nút 👁️ Front |
| View Top | Nút 👁️ Top |
| View Side | Nút 👁️ Side |

### 4️⃣ Transformation (Biến Đổi)
- **Rotate X/Y/Z**: Thanh trượt -180° đến +180°
- **Scale**: Thanh trượt 0.1x đến 3.0x
- **Reset**: Một nút để reset tất cả

### 5️⃣ Render Settings (Cài Đặt Render)
```
☑️ Lighting          (Bật/tắt ánh sáng)
☑️ Grid              (Lưới tham chiếu)
☑️ Coordinate System (Trục X, Y, Z)
☑️ Wireframe         (Hiển thị khung dây)
```

### 6️⃣ Thông Tin Mô Hình
Hiển thị:
- 📊 Số lượng đỉnh (Vertices)
- 📊 Số lượng mặt (Faces)
- 📊 Số lượng Mesh
- 📊 Kích thước file

### 7️⃣ Screenshot
- 📸 Chụp viewport hiện tại
- 💾 Lưu dưới dạng PNG/JPG/BMP
- 📂 Mở thư mục chứa ảnh sau khi chụp

### 8️⃣ Animation (Hệ Mặt Trời)
- ▶️ Phát/Dừng animation
- 🎚️ Điều chỉnh tốc độ animation
- 🏷️ Nhãn tên hành tinh tự động

---

## 🎮 Hướng Dẫn Sử Dụng

### Bước 1: Mở Ứng Dụng
```
Nhấp đôi vào 3DModelViewer.exe (hoặc chạy từ Visual Studio)
```

### Bước 2: Mở File 3D
**Cách A - Mở file đơn:**
```
1. Nhấn nút 📁 (Open File)
2. Chọn file 3D
3. Nhấn "Open"
```

**Cách B - Mở thư mục:**
```
1. Nhấn nút 📂 (Open Folder)
2. Chọn thư mục
3. Ứng dụng sẽ liệt kê tất cả file 3D
```

### Bước 3: Xem Model
```
- Chuột phải + kéo: Xoay xung quanh
- Con lăn: Zoom in/out
- Chuột giữa + kéo: Pan (dịch chuyển)
```

### Bước 4: Biến Đổi Model
```
1. Sử dụng các Slider ở panel phải
2. Rotate X/Y/Z: Xoay theo từng trục
3. Scale: Phóng to/thu nhỏ
4. Nhấn "Reset Transform" để quay lại ban đầu
```

### Bước 5: Tạo Mô Hình Mẫu
```
1. Ở panel trái, chọn mô hình
2. Ví dụ: "Hình cầu", "Hình lập phương"
3. Model sẽ được tạo ngay lập tức
```

### Bước 6: Chụp Screenshot
```
1. Xóa model như mong muốn
2. Nhấn nút 📷 (Screenshot)
3. Chọn vị trí lưu
4. Chọn định dạng (PNG/JPG/BMP)
5. Nhấn "Save"
```

### Bước 7: Tìm Kiếm
```
1. Nhập tên file hoặc extension vào ô "Tìm kiếm..."
2. Danh sách sẽ lọc tự động
3. Nhấn vào file để xem
```

---

## 🌐 Định Dạng File Hỗ Trợ

| Format | Extension | Mô Tả |
|--------|-----------|-------|
| Wavefront OBJ | `.obj` | Định dạng 3D phổ biến |
| FBX | `.fbx` | Dành cho animation & rigging |
| Collada | `.dae` | Định dạng trao đổi 3D |
| 3DS Max | `.3ds` | Định dạng cũ nhưng vẫn phổ biến |
| Blender | `.blend` | Định dạng Blender |
| STL | `.stl` | Dành cho 3D printing |
| PLY | `.ply` | Point cloud format |
| glTF 2.0 | `.gltf, .glb` | Web 3D standard |
| X3D | `.x3d` | ISO standard cho 3D |

---

## ⚙️ Cài Đặt Nâng Cao

### Thay Đổi Màu Nền
```
Panel phải → Background Color → Chọn màu
```

### Điều Chỉnh FOV (Field of View)
```
Panel phải → FOV Slider → Kéo để thay đổi
```

### Tắt Lighting Để Tốc Độ Cao Hơn
```
Panel phải → ☑️ Lighting → Bỏ dấu tích
```

---

## 🐛 Troubleshooting

### ❌ Lỗi: "Cannot load DLL 'assimp.dll'"
**Giải pháp:**
- Cài Microsoft C++ Runtime: https://support.microsoft.com/en-us/help/2977003
- Hoặc build lại project: `dotnet clean && dotnet build`

### ❌ Lỗi: ".NET 8 Runtime not found"
**Giải pháp:**
- Cài .NET 8 Runtime: https://dotnet.microsoft.com/download/dotnet/8.0
- Chọn "Desktop Runtime"

### ❌ File 3D không hiển thị
**Giải pháp:**
1. Kiểm tra định dạng có hỗ trợ không
2. Thử mở file khác
3. Kiểm tra console output có lỗi không
4. Thử reset camera: Nút 🔄

### ❌ Ứng dụng bị lag/chậm
**Giải pháp:**
- Tắt Lighting: Panel phải → Bỏ ☑️ Lighting
- Giảm chất lượng model (open model nhỏ hơn)
- Tắt Grid/Axes nếu không cần
- Đóng các ứng dụng khác

### ❌ Screenshot không thành công
**Giải pháp:**
- Chắc chắn có quyền ghi file
- Kiểm tra đường dẫn tồn tại
- Thử save vào Desktop
- Kiểm tra dung lượng ổ cứng

---

## 📊 Hiệu Suất

### Tối Ưu Hóa Mục Tiêu
- ✅ Hỗ trợ model lên đến 1-2 triệu đỉnh
- ✅ 60 FPS (nếu tắt lighting)
- ✅ Khởi động < 2 giây
- ✅ Sử dụng RAM < 500 MB

### Tips Cải Thiện Hiệu Suất
1. Tắt Lighting nếu không cần
2. Tắt Grid/Coordinate System
3. Giới hạn số lượng model được load
4. Sử dụng các file model tối ưu

---

## 🔐 Bảo Mật

- ✅ Không yêu cầu internet
- ✅ Không thu thập dữ liệu cá nhân
- ✅ Không có quảng cáo
- ✅ Code mở nguồn (có thể kiểm tra)

---

## 📞 Hỗ Trợ & Liên Hệ

Nếu có vấn đề:
1. Xem file [DEVELOPMENT.md](DEVELOPMENT.md) - Hướng dẫn phát triển
2. Xem file [BUILD_INSTRUCTIONS.md](BUILD_INSTRUCTIONS.md) - Hướng dẫn build
3. Kiểm tra [CHANGELOG.md](CHANGELOG.md) - Lịch sử thay đổi
4. Liên hệ với tác giả

---

## 📜 Giấy Phép

MIT License - Xem [LICENSE](LICENSE) để biết chi tiết

---

## 👨‍💻 Tác Giả

**QA Team**
- Phát triển và bảo trì dự án

---

## 🙏 Cảm Ơn

Cảm ơn các thư viện mã nguồn mở:
- **HelixToolkit** - 3D visualization
- **AssimpNet** - Model importing
- **.NET Team** - Amazing framework

---

<div align="center">

**Chúc bạn sử dụng vui vẻ! 🎉**

[⬆ Về Đầu](#-3d-model-viewer---net-8-wpf)

</div>
