# 🔨 Hướng Dẫn Build Dự Án

> Tài liệu này hướng dẫn cách biên dịch (build) dự án 3D Model Viewer từ source code

---

## 📋 Yêu Cầu Tiên Quyết

### Cài Đặt Bắt Buộc

1. **Visual Studio 2022**
   - Download: https://visualstudio.microsoft.com/vs/
   - Chọn "Desktop development with C++" workload
   - Hoặc ".NET desktop development"

2. **.NET 8 SDK**
   - Download: https://dotnet.microsoft.com/download/dotnet/8.0
   - Chọn "SDK" (không phải Runtime)
   - Cài đặt cho toàn hệ thống

3. **Git** (Optional - để clone repo)
   - Download: https://git-scm.com/download/win

### Kiểm Tra Cài Đặt

Mở **PowerShell** hoặc **Command Prompt** và chạy:

```bash
# Kiểm tra .NET
dotnet --version
# Kết quả mong muốn: 8.0.x

# Kiểm tra Git (nếu cài)
git --version
# Kết quả mong muốn: git version 2.x.x
```

---

## 🚀 Quy Trình Build

### **Phương Pháp 1: Từ Visual Studio (Dễ Nhất)**

#### Bước 1: Mở Solution
```
1. Khởi động Visual Studio 2022
2. File → Open → Project/Solution
3. Chọn file: 3DModelViewer.sln
4. Nhấp "Open"
```

#### Bước 2: Chờ Solution Load
```
Visual Studio sẽ:
- Load các project
- Kiểm tra dependencies
- Đôi khi yêu cầu restore packages
```

#### Bước 3: Restore NuGet Packages
**Cách A - Tự động:**
```
Visual Studio sẽ nhỏ thông báo ở trên:
"Some NuGet packages are missing"
→ Nhấn "Restore"
```

**Cách B - Thủ công:**
```
1. Vào menu: Tools → NuGet Package Manager → Manage NuGet Packages for Solution
2. Nhấn nút "Restore"
3. Hoặc: Tools → NuGet Package Manager → Package Manager Console
   Gõ: Update-Package
   Nhấn Enter
```

#### Bước 4: Build Solution
**Cách A - Bằng Menu:**
```
1. Vào: Build → Build Solution
2. Hoặc: Build → Rebuild Solution (xóa old files trước)
```

**Cách B - Bằng Keyboard Shortcut:**
```
Ctrl + Shift + B  (Build)
```

**Cách C - Bằng Solution Explorer:**
```
1. Right-click vào solution name
2. Chọn "Build Solution"
```

#### Bước 5: Kiểm Tra Kết Quả
Ở dưới cùng của VS, xem phần "Output":

**✅ Build Thành Công:**
```
========== Build: 1 succeeded, 0 failed, 0 skipped ==========
```

**❌ Build Thất Bại:**
```
========== Build: 0 succeeded, 1 failed, 0 skipped ==========
→ Xem Error List (View → Error List)
→ Fix các lỗi
→ Build lại
```

#### Bước 6: Run Application
**Cách A - Debug Mode (Có thể debug):**
```
F5  (Start Debugging)
Hoặc: Debug → Start Debugging
```

**Cách B - Release Mode (Nhanh hơn):**
```
Ctrl + F5  (Start Without Debugging)
Hoặc: Debug → Start Without Debugging
```

---

### **Phương Pháp 2: Từ Command Line**

Mở **PowerShell** hoặc **Command Prompt** (Admin) và chạy:

#### Bước 1: Di Chuyển Tới Thư Mục Dự Án
```bash
cd d:\QA\3DModelViewer
```

#### Bước 2: Restore NuGet Packages
```bash
dotnet restore
```
**Output mong muốn:**
```
Determining projects to restore...
Restored d:\QA\3DModelViewer\3DModelViewer.csproj
```

#### Bước 3: Build Debug
```bash
dotnet build -c Debug
```

**Hoặc Build Release (Nhanh hơn):**
```bash
dotnet build -c Release
```

#### Bước 4: Kiểm Tra Kết Quả
```
Build succeeded. 0 Warning(s)
```

#### Bước 5: Run Application
```bash
# Chạy từ mã Debug mới xây dựng
dotnet run --no-build -c Debug

# Hoặc chạy Release
dotnet run --no-build -c Release
```

---

## 📂 Output Locations (Vị Trí File Build)

Sau khi build, file .exe nằm tại:

### Debug Mode
```
d:\QA\3DModelViewer\bin\Debug\net8.0-windows\3DModelViewer.exe
```

### Release Mode
```
d:\QA\3DModelViewer\bin\Release\net8.0-windows\3DModelViewer.exe
```

### Chạy Trực Tiếp
```bash
# Debug
.\bin\Debug\net8.0-windows\3DModelViewer.exe

# Release
.\bin\Release\net8.0-windows\3DModelViewer.exe
```

---

## 🧹 Clean & Rebuild

### Xóa Build Files (Clean)

**Từ Visual Studio:**
```
Build → Clean Solution
```

**Từ Command Line:**
```bash
dotnet clean
```

Cách này xóa:
- ✓ bin/ folder
- ✓ obj/ folder
- ✓ Build cache

### Rebuild (Clean + Build)

**Từ Visual Studio:**
```
Build → Rebuild Solution
```

**Từ Command Line:**
```bash
dotnet clean
dotnet build -c Release
```

---

## 📦 Publish (Tạo Release Executable)

Để tạo file executable đóng gói sẵn (không cần .NET SDK):

### Cách 1: Command Line (Khuyến Nghị)

```bash
# Chuyển đến thư mục dự án
cd d:\QA\3DModelViewer

# Publish Release version
dotnet publish -c Release -o ./publish

# Hoặc Self-contained (không cần .NET installed)
dotnet publish -c Release -o ./publish -r win-x64 --self-contained
```

### Cách 2: Visual Studio

```
1. Right-click vào project name
2. Chọn "Publish..."
3. Chọn "Folder"
4. Chọn vị trí output
5. Nhấn "Publish"
```

### Output
```
./publish/
├── 3DModelViewer.exe
├── 3DModelViewer.dll
├── config.json
├── runtimes/
└── [Các file dependencies khác]
```

---

## ❌ Troubleshooting

### Lỗi 1: "The project file could not be loaded"
```
❌ Error: The project file could not be loaded. [path]
```

**Giải pháp:**
```bash
# Xóa cache
dotnet nuget locals all --clear

# Restore lại
dotnet restore

# Build lại
dotnet build
```

---

### Lỗi 2: "NuGet Package Restore Failed"
```
❌ Unable to resolve dependency 'HelixToolkit.Wpf'
```

**Giải pháp:**
```bash
# Clear cache
dotnet nuget locals all --clear

# Restore with verbose output
dotnet restore --verbosity detailed

# Check Internet connection
ping nuget.org
```

---

### Lỗi 3: "The name 'InitializeComponent' does not exist"
```
❌ The name 'InitializeComponent' does not exist in the current context
```

**Giải pháp:**
```bash
# Clean xor files XAML generated
dotnet clean

# Build lại
dotnet build

# Nếu vẫn lỗi, xóa file:
# d:\QA\3DModelViewer\bin\
# d:\QA\3DModelViewer\obj\
# Rồi build lại
```

---

### Lỗi 4: ".NET 8 is not installed"
```
❌ It was not possible to find any suitable supported .NET runtime
```

**Giải pháp:**
```bash
# Kiểm tra installed runtimes
dotnet --list-runtimes

# Cài .NET 8
# Download từ: https://dotnet.microsoft.com/download/dotnet/8.0

# Hoặc qua Windows Terminal:
winget install Microsoft.DotNet.SDK.8
```

---

## 🔧 Build Configurations

### Debug vs Release

| Khía Cạnh | Debug | Release |
|----------|-------|---------|
| **Tốc độ Build** | Nhanh | Chậm |
| **Tốc độ Runtime** | Chậm | Nhanh |
| **File Size** | Lớn | Nhỏ |
| **Debugging** | ✅ Có | ❌ Không |
| **Optimizations** | ❌ Không | ✅ Có |
| **Ideal For** | Phát triển | Sản xuất |

**Chọn Configuration:**

Visual Studio:
```
Trên thanh toolbar, có dropdown menu:
Select: "Debug" hoặc "Release"
```

Command Line:
```bash
dotnet build -c Debug    # Debug
dotnet build -c Release  # Release
```

---

## ⚡ Advanced Build Options

### Build Với Specific Configuration
```bash
# Release + Self-contained
dotnet publish -c Release -o ./publish -r win-x64 --self-contained

# Debug + Specific target
dotnet build --framework net8.0-windows
```

### Parallel Build (Nhanh hơn)
```bash
# Build với nhiều cores
dotnet build -m
```

### Verbose Output (Debug)
```bash
dotnet build --verbosity detailed
```

---

## ✅ Checklist Build

- [ ] .NET 8 SDK cài đặt
- [ ] Visual Studio 2022 cài đặt
- [ ] Repository cloned hoặc extracted
- [ ] Tất cả files đang chỗ đúng
- [ ] NuGet packages restored
- [ ] Build không có lỗi
- [ ] Application chạy thành công
- [ ] Tất cả tính năng hoạt động

---

## 💡 Tips & Best Practices

1. **Luôn Restore trước Build**
   ```bash
   dotnet restore && dotnet build
   ```

2. **Xóa Cache Nếu Lỗi Lạ**
   ```bash
   dotnet nuget locals all --clear
   ```

3. **Rebuild Nếu Có Nghi Vấn**
   ```bash
   dotnet clean && dotnet build
   ```

4. **Sử dụng Release cho Shipping**
   ```bash
   dotnet publish -c Release -o ./publish
   ```

5. **Keep SDK Updated**
   ```bash
   dotnet sdk check
   ```

---

## 📚 Tham Khảo Thêm

- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Visual Studio Documentation](https://docs.microsoft.com/en-us/visualstudio/)
- [NuGet Package Manager](https://www.nuget.org/)
- [HelixToolkit Documentation](https://github.com/helix-toolkit/helix-toolkit)

---

## 🆘 Cần Giúp?

Nếu build thất bại:
1. Đọc error message kỹ lưỡng
2. Xem phần Troubleshooting trên
3. Xóa `bin/` và `obj/` rồi build lại
4. Kiểm tra internet connection
5. Update .NET SDK mới nhất

---

<div align="center">

**Build Thành Công = Sẵn Sàng Phát Triển! 🚀**

[← Quay Lại README](README.md)

</div>
