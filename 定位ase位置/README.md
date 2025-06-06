# ASE 文件定位器

这是一个用于帮助定位和打开 ASE 文件的工具程序。

## 功能

1. **文件定位**: 根据 `.dml` 或 `.mat` 文件自动查找同目录下同名的 `.ase` 文件
2. **文件打开**: 使用 `ModelPreview.exe` 自动打开找到的 ASE 文件
3. **资源管理器定位**: 在 Windows 资源管理器中高亮显示找到的 ASE 文件
4. **右键菜单集成**: 可以注册到系统右键菜单，方便使用

## 编译

### 方法1: 使用批处理文件（推荐）
双击运行 `build.bat` 文件，程序会自动尝试编译。

### 方法2: 使用Visual Studio
1. 在 Visual Studio 中打开 `定位ase位置.csproj`
2. 选择 Release 配置
3. 生成解决方案

### 方法3: 使用命令行
```bash
msbuild "定位ase位置.csproj" /p:Configuration=Release
```

## 使用方法

### 首次使用 - 注册右键菜单
1. 以**管理员身份**运行编译后的程序（`ASELocator.exe` 或 `定位ase位置.exe`）
2. 程序会显示注册对话框，点击"是"来注册右键菜单
3. 注册成功后，就可以在 `.dml` 和 `.mat` 文件上使用右键菜单了

### 日常使用
1. 右键点击任意 `.dml` 或 `.mat` 文件
2. 选择 "定位 ASE 文件" 菜单项
3. 程序会自动：
   - 查找同目录下同名的 `.ase` 文件
   - 使用 `ModelPreview.exe` 打开该文件
   - 在资源管理器中定位该文件

## 系统要求

- Windows 操作系统
- .NET Framework 4.8 或更高版本
- 需要有 `ModelPreview.exe` 程序在同一目录下

## 注意事项

1. **管理员权限**: 首次注册右键菜单时需要管理员权限
2. **文件位置**: 确保 `ModelPreview.exe` 与本程序在同一目录下
3. **文件命名**: ASE 文件必须与 DML/MAT 文件同名（扩展名不同）且在同一目录下

## 错误处理

- 如果找不到对应的 ASE 文件，程序会显示提示信息
- 如果找不到 `ModelPreview.exe`，程序会显示警告
- 如果没有管理员权限注册菜单，程序会提示以管理员身份运行

## 卸载

如需移除右键菜单，可以手动删除注册表项：
- `HKEY_CLASSES_ROOT\.dml`
- `HKEY_CLASSES_ROOT\.mat`
- `HKEY_CLASSES_ROOT\dmlfile\shell\LocateASE`
- `HKEY_CLASSES_ROOT\matfile\shell\LocateASE` 