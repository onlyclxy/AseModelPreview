# DML编辑器

一个专为DML格式文件设计的文本编辑器，类似于Notepad3，具有语法高亮功能。

## 功能特性

### 基本编辑功能
- ✅ 新建、打开、保存文件
- ✅ 剪切、复制、粘贴
- ✅ 撤销、重做
- ✅ 查找和替换
- ✅ 字体设置

### DML格式支持
- ✅ 语法高亮显示
  - 段标题：`[SHADER]`、`[GEOMETRY MESH]` 等以蓝色粗体显示
  - 文件路径：包含路径分隔符和常见3D文件扩展名的行以绿色显示
  - 注释：以 `//` 或 `#` 开头的行以灰色斜体显示
- ✅ 行号显示
- ✅ 状态栏显示当前行列信息

### 界面特性
- 菜单栏：文件、编辑、视图、帮助
- 工具栏：快速访问常用功能
- 状态栏：显示文件状态和光标位置
- 可切换的语法高亮和行号显示

## DML格式说明

DML格式类似于INI配置文件，具有以下特征：

```dml
// 注释行，以//或#开头
[SECTION_NAME]
配置项或数据
文件路径

示例：
[SHADER]
11111111

[GEOMETRY MESH]
..\resources\art\eff\chair\c25851\model\c25851_md_xuanzhuan02.ase

[NEW_SHADER]
..\resources\art\eff\chair\c25851\texture\c25851_md_xuanzhuan02.mat
```

### 支持的段类型
- `[SHADER]` - 着色器配置
- `[GEOMETRY MESH]` - 几何网格
- `[NEW_SHADER]` - 新着色器
- `[SubEntity]` - 子实体
- `[MATERIAL]` - 材质
- `[ANIMATION]` - 动画
- `[LIGHTING]` - 光照
- `[CONFIG]` - 配置

### 支持的文件类型
编辑器能够识别并高亮显示以下3D文件格式的路径：
- `.ase` - 3ds Max ASCII Scene Export
- `.mat` - 材质文件
- `.fbx` - Filmbox格式
- `.obj` - Wavefront OBJ
- `.dae` - COLLADA
- `.3ds` - 3D Studio
- `.blend` - Blender
- `.max` - 3ds Max

## 快捷键

| 功能 | 快捷键 |
|------|--------|
| 新建 | Ctrl+N |
| 打开 | Ctrl+O |
| 保存 | Ctrl+S |
| 另存为 | Ctrl+Shift+S |
| 剪切 | Ctrl+X |
| 复制 | Ctrl+C |
| 粘贴 | Ctrl+V |
| 撤销 | Ctrl+Z |
| 重做 | Ctrl+Y |
| 查找 | Ctrl+F |
| 替换 | Ctrl+H |

## 编译和运行

### 系统要求
- Windows 10 或更高版本
- .NET Framework 4.8
- Visual Studio 2019 或更高版本

### 编译步骤
1. 打开 `DMLView.sln` 解决方案文件
2. 在Visual Studio中选择 "生成" > "生成解决方案"
3. 编译完成后，可执行文件位于 `bin\Debug\DMLView.exe`

### 直接运行
双击 `bin\Debug\DMLView.exe` 即可启动编辑器

## 使用说明

1. **启动编辑器**：运行 `DMLView.exe`
2. **打开DML文件**：使用 "文件" > "打开" 或 Ctrl+O
3. **编辑文件**：在编辑区域直接输入或修改内容
4. **语法高亮**：编辑器会自动对DML格式进行着色
5. **保存文件**：使用 "文件" > "保存" 或 Ctrl+S

## 开发说明

### 项目结构
```
DMLView/
├── MainWindow.xaml          # 主窗口界面
├── MainWindow.xaml.cs       # 主窗口逻辑
├── App.xaml                 # 应用程序定义
├── App.xaml.cs              # 应用程序逻辑
├── sample.dml               # 示例DML文件
└── Resources/               # 资源文件夹
```

### 主要类和方法
- `MainWindow` - 主窗口类，包含所有编辑器功能
- `ApplySyntaxHighlighting()` - 应用DML语法高亮
- `FindDialog` - 查找对话框
- `ReplaceDialog` - 替换对话框

### 扩展功能
可以通过修改 `ApplySyntaxHighlighting()` 方法来添加更多的语法高亮规则，或者扩展支持的文件格式。

## 许可证

此项目是开源项目，欢迎贡献代码和建议。

## 更新历史

- v1.0 - 初始版本，支持基本的DML格式编辑和语法高亮 