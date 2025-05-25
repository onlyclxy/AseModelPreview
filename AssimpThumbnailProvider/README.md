# AssimpThumbnailProvider

一个基于 AssimpNet 和 HelixToolkit 的 Windows 缩略图提供程序，用于显示 3D 模型文件的缩略图预览。

## 支持的文件格式

该缩略图提供程序支持以下 3D 模型文件格式：
- .ase (3ds Max ASE)
- .fbx (Autodesk FBX)
- .obj (Wavefront OBJ)
- .gltf (GL Transmission Format)
- .glb (GL Transmission Format Binary)

## 安装说明

1. 下载并安装 [.NET 8.0 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
2. 克隆或下载本仓库
3. 使用 Visual Studio 2022 打开解决方案并构建项目
4. 以管理员身份运行 `install.bat` 脚本
5. 等待安装完成，Explorer 会自动重启

## 卸载说明

1. 以管理员身份运行 `uninstall.bat` 脚本
2. 等待卸载完成，Explorer 会自动重启

## 需求

- Windows 10 或 Windows 11
- .NET 8.0 Runtime
- [SharpShell ServerRegistrationManager (SRM)](https://github.com/dwmkerr/sharpshell)

## 实现细节

该项目使用以下技术：
- AssimpNet：用于加载各种格式的 3D 模型文件
- HelixToolkit.Wpf：用于渲染 3D 模型的缩略图
- SharpShell：用于实现 Windows Shell 扩展

## 注意事项

- 需要以管理员权限运行安装和卸载脚本
- 第一次生成缩略图可能会比较慢，因为需要加载所有必要的库
- 某些大型或复杂的模型可能需要更长的时间生成缩略图

## 故障排除

如果缩略图没有正确显示：
1. 确保已安装 .NET 8.0 Runtime
2. 尝试手动清除 Windows 缩略图缓存（在资源管理器选项中）
3. 确保所有依赖项都已正确安装
4. 检查 Windows 事件查看器中是否有错误 