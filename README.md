# 3D Model Preview Tools | 3D模型预览工具集

A comprehensive set of tools for 3D model viewing and thumbnail generation on Windows.

一套完整的Windows 3D模型查看和缩略图生成工具集。

## Projects | 项目介绍

### 🎯 AseModelPreview
**3D Model Material Previewer | 3D模型材质预览器**

A powerful WPF-based 3D model viewer with advanced material preview capabilities.

基于WPF的强大3D模型查看器，具有高级材质预览功能。

#### Features | 功能特点
- **Multi-format Support | 多格式支持**: .obj, .fbx, .ase, .gltf, .glb, .dae
- **Material Preview | 材质预览**: 
  - Isolated preview (individual mesh preview) | 孤立预览（单个网格预览）
  - Transparent preview (highlight specific materials) | 透明预览（突出显示特定材质）
- **UV Mapping Viewer | UV映射查看器**: Visual UV coordinate display with multi-channel support | 可视化UV坐标显示，支持多通道
- **Vertex Data Visualization | 顶点数据可视化**:
  - Vertex colors display | 顶点颜色显示
  - Vertex alpha/transparency visualization | 顶点透明度可视化
- **Plugin System | 插件系统**: Extensible architecture for custom functionality | 可扩展架构支持自定义功能
- **Customizable Toolbar | 可自定义工具栏**: Configure which tools are visible | 配置可见工具
- **Drag & Drop Support | 拖放支持**: Easy file loading | 便捷的文件加载
- **Keyboard Shortcuts | 键盘快捷键**: Ctrl+O to load models | Ctrl+O加载模型

#### Technical Stack | 技术栈
- **Framework**: .NET Framework with WPF | .NET Framework + WPF
- **3D Engine**: HelixToolkit.Wpf | HelixToolkit.Wpf
- **Model Loading**: Assimp.Net | Assimp.Net模型加载
- **Language**: C# | C#

### 🖼️ AssimpThumbnailProvider
**Windows Shell Thumbnail Provider | Windows资源管理器缩略图提供程序**

A Windows Shell extension that generates thumbnails for 3D model files directly in Windows Explorer.

Windows Shell扩展，直接在Windows资源管理器中为3D模型文件生成缩略图。

#### Features | 功能特点
- **Shell Integration | Shell集成**: Seamless Windows Explorer integration | 无缝Windows资源管理器集成
- **Multi-format Support | 多格式支持**: .ase, .fbx, .obj, .gltf, .glb, .dae, .dml, .chr
- **Special File Handling | 特殊文件处理**: 
  - DML/CHR file support with automatic model discovery | 支持DML/CHR文件并自动发现模型
  - Uses DmlPathGet component for intelligent file parsing | 使用DmlPathGet组件进行智能文件解析
- **High-Quality Rendering | 高质量渲染**: 
  - Multi-light setup for better visualization | 多光源设置以获得更好的可视化效果
  - Automatic model normalization and positioning | 自动模型标准化和定位
- **Performance Optimized | 性能优化**: Efficient off-screen rendering | 高效的离屏渲染
- **Error Resilience | 错误恢复**: Fallback mechanisms for corrupted files | 损坏文件的回退机制

#### Technical Stack | 技术栈
- **Framework**: .NET Framework | .NET Framework
- **Shell Extension**: SharpShell | SharpShell
- **3D Rendering**: WPF + HelixToolkit.Wpf (off-screen) | WPF + HelixToolkit.Wpf（离屏）
- **Model Loading**: Assimp.Net | Assimp.Net模型加载
- **COM Registration**: Required for Windows integration | Windows集成所需的COM注册

### 🔍 DmlPathGet
**DML File Parser | DML文件解析器**

A utility component for parsing DML files and extracting model file paths.

用于解析DML文件并提取模型文件路径的实用程序组件。

#### Features | 功能特点
- **Smart Path Resolution | 智能路径解析**: Resolves relative and absolute paths | 解析相对和绝对路径
- **Everything Search Integration | Everything搜索集成**: Uses Everything API for file discovery | 使用Everything API进行文件发现
- **Configuration Management | 配置管理**: Project directory configuration | 项目目录配置
- **Error Handling | 错误处理**: Robust error handling and logging | 强大的错误处理和日志记录

## Installation | 安装说明

### Prerequisites | 先决条件
- Windows 10 or later | Windows 10或更高版本
- .NET Framework 4.7.2 or later | .NET Framework 4.7.2或更高版本
- Visual C++ Redistributable | Visual C++可再发行组件包

### AseModelPreview Installation | AseModelPreview安装
1. Download the latest release | 下载最新版本
2. Extract to desired location | 解压到所需位置
3. Run `AseModelPreview.exe` | 运行 `AseModelPreview.exe`

### AssimpThumbnailProvider Installation | AssimpThumbnailProvider安装
1. Download the compiled DLL | 下载编译的DLL
2. Register the COM component: | 注册COM组件：
   ```cmd
   regasm AssimpThumbnailProvider.dll /codebase
   ```
3. Restart Windows Explorer | 重启Windows资源管理器

## Usage | 使用方法

### AseModelPreview
1. **Load Model | 加载模型**: 
   - Use File → Load Model or Ctrl+O | 使用 文件→加载模型 或 Ctrl+O
   - Drag and drop supported files | 拖放支持的文件
2. **Preview Materials | 预览材质**:
   - Click "Isolated Preview" for individual mesh view | 点击"孤立预览"查看单个网格
   - Click "Transparent Preview" to highlight materials | 点击"透明预览"突出显示材质
3. **View UV Mapping | 查看UV映射**: Click "View UV Mapping" | 点击"查看UV映射"
4. **Vertex Visualization | 顶点可视化**: Use checkboxes for vertex colors/alpha | 使用复选框显示顶点颜色/透明度

### AssimpThumbnailProvider
Once installed, thumbnails will automatically appear in Windows Explorer for supported file types.

安装后，支持的文件类型将在Windows资源管理器中自动显示缩略图。

## Building from Source | 从源码构建

### Requirements | 要求
- Visual Studio 2019 or later | Visual Studio 2019或更高版本
- .NET Framework 4.7.2 SDK | .NET Framework 4.7.2 SDK

### Build Steps | 构建步骤
1. Clone the repository | 克隆仓库
2. Restore NuGet packages | 恢复NuGet包
3. Build solution in Release mode | 在Release模式下构建解决方案

## Dependencies | 依赖项

### Core Dependencies | 核心依赖
- **Assimp.Net**: 3D model loading | 3D模型加载
- **HelixToolkit.Wpf**: 3D rendering | 3D渲染
- **SharpShell**: Windows Shell extensions | Windows Shell扩展

### Runtime Dependencies | 运行时依赖
- **Assimp Native Library**: Platform-specific native binaries | 特定平台的本机二进制文件

## Contributing | 贡献

1. Fork the repository | Fork仓库
2. Create a feature branch | 创建功能分支
3. Commit your changes | 提交更改
4. Push to the branch | 推送到分支
5. Create a Pull Request | 创建Pull Request

## License | 许可证

This project is licensed under the MIT License - see the LICENSE file for details.

本项目基于MIT许可证 - 详情请参见LICENSE文件。

## Support | 支持

For issues and feature requests, please use the GitHub issue tracker.

如有问题和功能请求，请使用GitHub问题跟踪器。

## Changelog | 更新日志

### v1.0.0
- Initial release | 初始版本
- Complete 3D model viewer with material preview | 完整的3D模型查看器，具有材质预览功能
- Windows Shell thumbnail provider | Windows Shell缩略图提供程序
- DML file parsing support | DML文件解析支持