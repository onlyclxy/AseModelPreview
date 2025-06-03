using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Assimp;
using HelixToolkit.Wpf;
using System.Windows.Controls;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using Media3D = System.Windows.Media.Media3D;
using System.Reflection;
using System.Windows.Input;

namespace AseTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Scene _currentScene;
        private List<GeometryModel3D> _models;
        private Model3DGroup _originalModelGroup;
        private List<DiffuseMaterial> _originalMaterials;  // 存储原始材质
        private string _currentModelPath; // 存储当前模型路径
        private readonly string[] _supportedExtensions = { ".obj", ".fbx", ".ase", ".gltf", ".glb",".dae" };
        private List<dynamic> _loadedPlugins = new List<dynamic>();  // 存储加载的插件
        private List<Button> _pluginButtons = new List<Button>(); // 存储插件按钮
        private ToolbarSettings _toolbarSettings;
        private Dictionary<string, Button> _coreButtons;
        private Dictionary<string, CheckBox> _vertexDisplayControls;
        private StackPanel _alphaEditorGroup;

        public MainWindow()
        {
            InitializeComponent();
            _models = new List<GeometryModel3D>();
            _originalMaterials = new List<DiffuseMaterial>();
            
            // 初始化工具栏设置
            InitializeToolbarSettings();
            InitializeCoreButtons();
            InitializeVertexDisplayControls();
            InitializeAlphaEditorGroup();
            
            // 设置窗口的拖放事件处理
            this.AllowDrop = true;
            this.DragEnter += OnDragEnter;
            this.Drop += OnDrop;

            // 加载插件
            LoadPlugins();

            // 应用工具栏设置
            ApplyToolbarSettings();

            // 添加键盘快捷键
            AddKeyboardShortcuts();
        }

        private void InitializeToolbarSettings()
        {
            _toolbarSettings = LoadToolbarSettings();
        }

        private void InitializeCoreButtons()
        {
            _coreButtons = new Dictionary<string, Button>
            {
                ["LoadModel"] = btnLoadModel,
                ["MaterialPreview"] = btnMaterialPreview,
                ["TransparentPreview"] = btnTransparentPreview,
                ["UVMapping"] = btnUVMapping
            };
        }

        private void InitializeVertexDisplayControls()
        {
            _vertexDisplayControls = new Dictionary<string, CheckBox>
            {
                ["VertexColor"] = chkVertexColor,
                ["VertexAlpha"] = chkVertexAlpha
            };
        }

        private void InitializeAlphaEditorGroup()
        {
            _alphaEditorGroup = alphaEditorGroup;
        }

        private void AddKeyboardShortcuts()
        {
            // Ctrl+O 加载模型
            var loadModelCommand = new RoutedCommand();
            loadModelCommand.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(loadModelCommand, (s, e) => OnLoadModelClick(s, e)));
        }

        private string GetConfigFilePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ToolbarSettings.config");
        }

        private ToolbarSettings LoadToolbarSettings()
        {
            try
            {
                string configPath = GetConfigFilePath();
                var settings = ToolbarSettings.LoadFromFile(configPath);
                
                // 设置默认值（如果配置文件中没有或是首次运行）
                if (!settings.CoreButtons.ContainsKey("LoadModel"))
                    settings.CoreButtons["LoadModel"] = true;
                if (!settings.CoreButtons.ContainsKey("MaterialPreview"))
                    settings.CoreButtons["MaterialPreview"] = true;
                if (!settings.CoreButtons.ContainsKey("TransparentPreview"))
                    settings.CoreButtons["TransparentPreview"] = true;
                if (!settings.CoreButtons.ContainsKey("UVMapping"))
                    settings.CoreButtons["UVMapping"] = true;
                if (!settings.CoreButtons.ContainsKey("AlphaEditor"))
                    settings.CoreButtons["AlphaEditor"] = true;
                if (!settings.VertexDisplayButtons.ContainsKey("VertexColor"))
                    settings.VertexDisplayButtons["VertexColor"] = true;
                if (!settings.VertexDisplayButtons.ContainsKey("VertexAlpha"))
                    settings.VertexDisplayButtons["VertexAlpha"] = true;

                return settings;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载配置文件失败，将使用默认设置: {ex.Message}", "配置", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                
                var defaultSettings = new ToolbarSettings();
                defaultSettings.CoreButtons["LoadModel"] = true;
                defaultSettings.CoreButtons["MaterialPreview"] = true;
                defaultSettings.CoreButtons["TransparentPreview"] = true;
                defaultSettings.CoreButtons["UVMapping"] = true;
                defaultSettings.CoreButtons["AlphaEditor"] = true;
                defaultSettings.VertexDisplayButtons["VertexColor"] = true;
                defaultSettings.VertexDisplayButtons["VertexAlpha"] = true;
                defaultSettings.CompactMode = false;
                defaultSettings.ShowButtonText = true;
                
                return defaultSettings;
            }
        }

        private void SaveToolbarSettings()
        {
            try
            {
                string configPath = GetConfigFilePath();
                _toolbarSettings.SaveToFile(configPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存配置文件失败: {ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ApplyToolbarSettings()
        {
            // 应用核心按钮显示设置
            foreach (var kvp in _coreButtons)
            {
                bool isVisible = _toolbarSettings.CoreButtons.ContainsKey(kvp.Key) 
                    ? _toolbarSettings.CoreButtons[kvp.Key] 
                    : true;
                kvp.Value.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            }

            // 应用透明度控件组显示设置
            bool alphaEditorVisible = _toolbarSettings.CoreButtons.ContainsKey("AlphaEditor") 
                ? _toolbarSettings.CoreButtons["AlphaEditor"] 
                : true;
            _alphaEditorGroup.Visibility = alphaEditorVisible ? Visibility.Visible : Visibility.Collapsed;

            // 应用顶点显示控件显示设置
            foreach (var kvp in _vertexDisplayControls)
            {
                bool isVisible = _toolbarSettings.VertexDisplayButtons.ContainsKey(kvp.Key) 
                    ? _toolbarSettings.VertexDisplayButtons[kvp.Key] 
                    : true;
                kvp.Value.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            }

            // 应用插件按钮显示设置
            for (int i = 0; i < _pluginButtons.Count; i++)
            {
                var button = _pluginButtons[i];
                string pluginName = GetPluginNameFromButton(button, i);
                bool isVisible = _toolbarSettings.PluginButtons.ContainsKey(pluginName) 
                    ? _toolbarSettings.PluginButtons[pluginName] 
                    : true;
                button.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            }

            // 应用紧凑模式
            if (_toolbarSettings.CompactMode)
            {
                foreach (var button in _coreButtons.Values)
                {
                    button.Margin = new Thickness(0, 0, 2, 0);
                    button.Padding = new Thickness(5, 2, 5, 2);
                }
                foreach (var button in _pluginButtons)
                {
                    button.Margin = new Thickness(0, 0, 2, 0);
                    button.Padding = new Thickness(5, 2, 5, 2);
                }
                foreach (var checkBox in _vertexDisplayControls.Values)
                {
                    checkBox.Margin = new Thickness(2, 0, 2, 0);
                }
            }
            else
            {
                foreach (var button in _coreButtons.Values)
                {
                    button.Margin = new Thickness(0, 0, 5, 0);
                    button.Padding = new Thickness(8, 3, 8, 3);
                }
                foreach (var button in _pluginButtons)
                {
                    button.Margin = new Thickness(0, 0, 5, 0);
                    button.Padding = new Thickness(8, 3, 8, 3);
                }
                foreach (var checkBox in _vertexDisplayControls.Values)
                {
                    checkBox.Margin = new Thickness(5, 0, 5, 0);
                }
            }

            // 应用按钮文字显示设置
            if (!_toolbarSettings.ShowButtonText)
            {
                foreach (var button in _coreButtons.Values)
                {
                    button.ToolTip = button.Content;
                    button.Content = "●"; // 使用简单图标替代
                    button.Width = 30;
                }
                foreach (var button in _pluginButtons)
                {
                    button.ToolTip = button.Content;
                    button.Content = "◆";
                    button.Width = 30;
                }
            }
        }

        private void OnShowVertexColorChanged(object sender, RoutedEventArgs e)
        {
            // 同步复选框和菜单项状态
            if (sender == chkVertexColor)
            {
                menuVertexColor.IsChecked = chkVertexColor.IsChecked ?? false;
            }
            else if (sender == menuVertexColor)
            {
                chkVertexColor.IsChecked = menuVertexColor.IsChecked;
            }

            UpdateVertexColorDisplay();
        }

        private void OnShowVertexAlphaChanged(object sender, RoutedEventArgs e)
        {
            // 同步复选框和菜单项状态
            if (sender == chkVertexAlpha)
            {
                menuVertexAlpha.IsChecked = chkVertexAlpha.IsChecked ?? false;
            }
            else if (sender == menuVertexAlpha)
            {
                chkVertexAlpha.IsChecked = menuVertexAlpha.IsChecked;
            }

            UpdateVertexAlphaDisplay();
        }

        private void UpdateVertexColorDisplay()
        {
            if (_currentScene == null || _models.Count == 0)
                return;

            bool showVertexColors = chkVertexColor.IsChecked ?? false;
            bool showVertexAlpha = chkVertexAlpha.IsChecked ?? false;

            try
            {
                if (showVertexColors || showVertexAlpha)
                {
                    // 显示顶点颜色和/或透明度
                    for (int i = 0; i < _models.Count && i < _currentScene.Meshes.Count; i++)
                    {
                        var model = _models[i];
                        var mesh = _currentScene.Meshes[i];

                        if (mesh.HasVertexColors(0))
                        {
                            ApplyVertexColorsAndAlpha(model, mesh, showVertexColors, showVertexAlpha);
                        }
                    }
                }
                else
                {
                    // 恢复原始模型
                    RestoreOriginalModels();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新顶点颜色显示失败: {ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UpdateVertexAlphaDisplay()
        {
            if (_currentScene == null || _models.Count == 0)
                return;

            bool showVertexColors = chkVertexColor.IsChecked ?? false;
            bool showVertexAlpha = chkVertexAlpha.IsChecked ?? false;

            try
            {
                if (showVertexColors || showVertexAlpha)
                {
                    // 显示顶点颜色和/或透明度
                    for (int i = 0; i < _models.Count && i < _currentScene.Meshes.Count; i++)
                    {
                        var model = _models[i];
                        var mesh = _currentScene.Meshes[i];

                        if (mesh.HasVertexColors(0))
                        {
                            ApplyVertexColorsAndAlpha(model, mesh, showVertexColors, showVertexAlpha);
                        }
                    }
                }
                else
                {
                    // 恢复原始模型
                    RestoreOriginalModels();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新顶点透明度显示失败: {ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RestoreOriginalModels()
        {
            if (_originalModelGroup == null || _models.Count == 0)
                return;

            // 清空当前模型组
            _originalModelGroup.Children.Clear();

            // 重新添加原始模型
            foreach (var model in _models)
            {
                // 恢复原始材质
                var originalIndex = _models.IndexOf(model);
                if (originalIndex < _originalMaterials.Count)
                {
                    model.Material = _originalMaterials[originalIndex];
                    model.BackMaterial = _originalMaterials[originalIndex];
                }
                _originalModelGroup.Children.Add(model);
            }

            // 更新状态栏
            if (!string.IsNullOrEmpty(_currentModelPath))
            {
                txtPath.Text = _currentModelPath;
            }
        }

        private void ApplyVertexColorsAndAlpha(GeometryModel3D model, Mesh mesh, bool showColors, bool showAlpha)
        {
            try
            {
                var geometry = model.Geometry as MeshGeometry3D;
                if (geometry == null || !mesh.HasVertexColors(0))
                    return;

                var vertexColors = mesh.VertexColorChannels[0];
                if (vertexColors.Count == 0)
                    return;

                // 根据选择创建相应的网格
                CreateVertexDisplayMesh(model, mesh, vertexColors, showColors, showAlpha);

                // 更新状态栏信息
                string statusText = "";
                if (showColors && showAlpha)
                    statusText = "显示顶点颜色+透明度";
                else if (showColors)
                    statusText = "显示顶点颜色";
                else if (showAlpha)
                    statusText = "显示顶点透明度";

                txtPath.Text = $"{txtPath.Text} | {statusText}: {vertexColors.Count}个顶点";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"应用顶点显示失败: {ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ApplyVertexColors(GeometryModel3D model, Mesh mesh)
        {
            // 这个方法现在由 ApplyVertexColorsAndAlpha 统一处理
            bool showVertexColors = chkVertexColor.IsChecked ?? false;
            bool showVertexAlpha = chkVertexAlpha.IsChecked ?? false;
            ApplyVertexColorsAndAlpha(model, mesh, showVertexColors, showVertexAlpha);
        }

        private void ApplyVertexAlpha(GeometryModel3D model, Mesh mesh)
        {
            // 这个方法现在由 ApplyVertexColorsAndAlpha 统一处理
            bool showVertexColors = chkVertexColor.IsChecked ?? false;
            bool showVertexAlpha = chkVertexAlpha.IsChecked ?? false;
            ApplyVertexColorsAndAlpha(model, mesh, showVertexColors, showVertexAlpha);
        }

        private void CreateVertexDisplayMesh(GeometryModel3D originalModel, Mesh mesh, List<Color4D> vertexColors, bool showColors, bool showAlpha)
        {
            // 移除原始模型
            _originalModelGroup.Children.Remove(originalModel);

            // 为每个面片创建单独的几何体和材质
            foreach (var face in mesh.Faces)
            {
                if (face.IndexCount < 3) continue;

                var faceGeometry = new MeshGeometry3D();
                var faceMaterials = new List<Color>();

                // 添加面片的顶点
                for (int i = 0; i < face.IndexCount; i++)
                {
                    int vertexIndex = face.Indices[i];
                    
                    // 添加顶点位置
                    if (vertexIndex < mesh.Vertices.Count)
                    {
                        var vertex = mesh.Vertices[vertexIndex];
                        faceGeometry.Positions.Add(new Point3D(vertex.X, vertex.Y, vertex.Z));
                    }

                    // 添加法线
                    if (vertexIndex < mesh.Normals.Count)
                    {
                        var normal = mesh.Normals[vertexIndex];
                        faceGeometry.Normals.Add(new Media3D.Vector3D(normal.X, normal.Y, normal.Z));
                    }

                    // 添加UV坐标
                    if (mesh.TextureCoordinateChannels.Length > 0 && 
                        vertexIndex < mesh.TextureCoordinateChannels[0].Count)
                    {
                        var uv = mesh.TextureCoordinateChannels[0][vertexIndex];
                        faceGeometry.TextureCoordinates.Add(new System.Windows.Point(uv.X, uv.Y));
                    }

                    // 收集顶点颜色和/或透明度
                    if (vertexIndex < vertexColors.Count)
                    {
                        var color = vertexColors[vertexIndex];
                        Color finalColor;

                        if (showColors && showAlpha)
                        {
                            // 两个都显示：颜色+透明度
                            finalColor = Color.FromArgb(
                                (byte)(color.A * 255),
                                (byte)(color.R * 255),
                                (byte)(color.G * 255),
                                (byte)(color.B * 255)
                            );
                        }
                        else if (showColors)
                        {
                            // 只显示颜色：不透明的顶点颜色
                            finalColor = Color.FromRgb(
                                (byte)(color.R * 255),
                                (byte)(color.G * 255),
                                (byte)(color.B * 255)
                            );
                        }
                        else if (showAlpha)
                        {
                            // 只显示透明度：灰色+透明度
                            finalColor = Color.FromArgb(
                                (byte)(color.A * 255),
                                128, 128, 128
                            );
                        }
                        else
                        {
                            finalColor = Colors.LightGray;
                        }

                        faceMaterials.Add(finalColor);
                    }
                }

                // 优化面片创建 - 尝试减少三角化的视觉效果
                CreateOptimizedFaceGeometry(faceGeometry, face, faceMaterials);

                // 计算面片的平均颜色
                if (faceMaterials.Count > 0)
                {
                    var avgColor = CalculateAverageColorWithAlpha(faceMaterials);
                    var material = new DiffuseMaterial(new SolidColorBrush(avgColor));

                    var faceModel = new GeometryModel3D
                    {
                        Geometry = faceGeometry,
                        Material = material,
                        BackMaterial = material
                    };

                    _originalModelGroup.Children.Add(faceModel);
                }
            }
        }

        private void CreateOptimizedFaceGeometry(MeshGeometry3D faceGeometry, Face face, List<Color> faceMaterials)
        {
            // 为了减少三角化的视觉效果，我们尝试优化几何体创建
            if (face.IndexCount == 3)
            {
                // 三角形
                faceGeometry.TriangleIndices.Add(0);
                faceGeometry.TriangleIndices.Add(1);
                faceGeometry.TriangleIndices.Add(2);
            }
            else if (face.IndexCount == 4)
            {
                // 四边形 - 使用更好的分割方式，保持面的平整度
                faceGeometry.TriangleIndices.Add(0);
                faceGeometry.TriangleIndices.Add(1);
                faceGeometry.TriangleIndices.Add(2);
                
                faceGeometry.TriangleIndices.Add(0);
                faceGeometry.TriangleIndices.Add(2);
                faceGeometry.TriangleIndices.Add(3);

                // 确保法线一致性，减少分割痕迹
                if (faceGeometry.Normals.Count >= 4)
                {
                    // 计算面的平均法线
                    var avgNormal = new Media3D.Vector3D(
                        (faceGeometry.Normals[0].X + faceGeometry.Normals[1].X + faceGeometry.Normals[2].X + faceGeometry.Normals[3].X) / 4,
                        (faceGeometry.Normals[0].Y + faceGeometry.Normals[1].Y + faceGeometry.Normals[2].Y + faceGeometry.Normals[3].Y) / 4,
                        (faceGeometry.Normals[0].Z + faceGeometry.Normals[1].Z + faceGeometry.Normals[2].Z + faceGeometry.Normals[3].Z) / 4
                    );
                    avgNormal.Normalize();

                    // 使用统一的法线减少视觉分割
                    for (int i = 0; i < faceGeometry.Normals.Count; i++)
                    {
                        faceGeometry.Normals[i] = avgNormal;
                    }
                }
            }
            else if (face.IndexCount > 4)
            {
                // 多边形 - 扇形三角化
                for (int i = 1; i < face.IndexCount - 1; i++)
                {
                    faceGeometry.TriangleIndices.Add(0);
                    faceGeometry.TriangleIndices.Add(i);
                    faceGeometry.TriangleIndices.Add(i + 1);
                }
            }
        }

        private Color CalculateAverageColorWithAlpha(List<Color> colors)
        {
            if (colors.Count == 0)
                return Colors.LightGray;

            double a = colors.Average(c => c.A);
            double r = colors.Average(c => c.R);
            double g = colors.Average(c => c.G);
            double b = colors.Average(c => c.B);

            return Color.FromArgb(
                (byte)Math.Min(255, Math.Max(0, a)),
                (byte)Math.Min(255, Math.Max(0, r)),
                (byte)Math.Min(255, Math.Max(0, g)),
                (byte)Math.Min(255, Math.Max(0, b))
            );
        }

        private Color CalculateAverageColor(List<Color> colors)
        {
            if (colors.Count == 0)
                return Colors.LightGray;

            double r = colors.Average(c => c.R);
            double g = colors.Average(c => c.G);
            double b = colors.Average(c => c.B);

            return Color.FromRgb(
                (byte)Math.Min(255, Math.Max(0, r)),
                (byte)Math.Min(255, Math.Max(0, g)),
                (byte)Math.Min(255, Math.Max(0, b))
            );
        }

        private string GetPluginNameFromButton(Button button, int index)
        {
            if (index < _loadedPlugins.Count)
            {
                return _loadedPlugins[index].Name;
            }
            return $"Plugin_{index}";
        }

        private void LoadPlugins()
        {
            try
            {
                string pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
                if (!Directory.Exists(pluginPath))
                {
                    Directory.CreateDirectory(pluginPath);
                    return;
                }

                foreach (string file in Directory.GetFiles(pluginPath, "*.dll"))
                {
                    try
                    {
                        Assembly assembly = Assembly.LoadFrom(file);
                        foreach (Type type in assembly.GetTypes())
                        {
                            if (type.GetInterfaces().Any(i => i.Name == "IModelPlugin"))
                            {
                                dynamic plugin = Activator.CreateInstance(type);
                                _loadedPlugins.Add(plugin);

                                // 获取插件按钮并添加到工具栏
                                var buttons = plugin.GetButtons();
                                foreach (Button button in buttons)
                                {
                                    _pluginButtons.Add(button);
                                    topPanel.Children.Add(button);

                                    // 添加到插件菜单
                                    var menuItem = new MenuItem
                                    {
                                        Header = button.Content.ToString(),
                                        Tag = button
                                    };
                                    menuItem.Click += (s, e) => button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                                    menuPlugins.Items.Add(menuItem);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"加载插件 {Path.GetFileName(file)} 失败: {ex.Message}", 
                            "插件加载错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                // 如果没有插件，隐藏插件菜单
                if (_loadedPlugins.Count == 0)
                {
                    menuPlugins.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载插件目录失败: {ex.Message}", 
                    "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnToolbarSettingsClick(object sender, RoutedEventArgs e)
        {
            var pluginNames = _loadedPlugins.Select(p => (string)p.Name).ToList();
            var settingsWindow = new ToolbarSettingsWindow(_toolbarSettings, pluginNames)
            {
                Owner = this
            };

            if (settingsWindow.ShowDialog() == true)
            {
                _toolbarSettings = settingsWindow.Settings;
                ApplyToolbarSettings();
                SaveToolbarSettings();
            }
        }

        private void OnAboutClick(object sender, RoutedEventArgs e)
        {
            string configPath = GetConfigFilePath();
            MessageBox.Show($"3D模型材质预览器 v1.0\n\n" +
                           $"支持格式: {string.Join(", ", _supportedExtensions)}\n" +
                           $"已加载插件: {_loadedPlugins.Count} 个\n" +
                           $"配置文件: {configPath}\n\n" +
                           $"Copyright © 2024", 
                           "关于", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            SaveToolbarSettings(); // 退出时保存设置
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            SaveToolbarSettings(); // 窗口关闭时保存设置
            base.OnClosed(e);
        }

        private bool IsSupportedModelFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            return _supportedExtensions.Contains(extension);
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            bool isValid = false;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files?.Length == 1 && IsSupportedModelFile(files[0]))
                {
                    isValid = true;
                }
            }
            e.Effects = isValid ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files?.Length == 1 && IsSupportedModelFile(files[0]))
                {
                    string filePath = files[0];
                    txtPath.Text = filePath;
                    LoadModel(filePath);
                }
            }
        }

        private void OnLoadModelClick(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = $"3D模型文件|{string.Join(";", _supportedExtensions.Select(ext => $"*{ext}"))}|所有文件|*.*"
            };
            if (dlg.ShowDialog() != true)
                return;

            LoadModelFromPath(dlg.FileName);
        }

        public void LoadModelFromPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            txtPath.Text = path;
            LoadModel(path);
        }

        private void LoadModel(string path)
        {
            if (!IsSupportedModelFile(path))
            {
                MessageBox.Show("不支持的文件格式。请选择 .obj, .fbx, .ase, .gltf 或 .glb 文件。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!File.Exists(path))
            {
                MessageBox.Show("文件不存在。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // 保存当前模型路径
                _currentModelPath = path;
                
                // 使用 AssimpNet 加载模型并进行基础处理
                var importer = new AssimpContext();
                _currentScene = importer.ImportFile(path,
                    PostProcessSteps.Triangulate |
                    PostProcessSteps.JoinIdenticalVertices |
                    PostProcessSteps.GenerateNormals |
                    PostProcessSteps.FlipUVs);

                // 构建 WPF 3D 模型组
                _originalModelGroup = new Model3DGroup();
                _models.Clear();
                _originalMaterials.Clear();

                foreach (var mesh in _currentScene.Meshes)
                {
                    var geometry = new MeshGeometry3D();
                    // 顶点
                    foreach (var v in mesh.Vertices)
                        geometry.Positions.Add(new Point3D(v.X, v.Y, v.Z));
                    // 法线
                    foreach (var n in mesh.Normals)
                        geometry.Normals.Add(new Media3D.Vector3D(n.X, n.Y, n.Z));
                    // UV
                    if (mesh.TextureCoordinateChannels.Length > 0)
                    {
                        foreach (var uv in mesh.TextureCoordinateChannels[0])
                            geometry.TextureCoordinates.Add(new System.Windows.Point(uv.X, uv.Y));
                    }
                    // 面索引（三角形）
                    foreach (var face in mesh.Faces)
                        if (face.IndexCount == 3)
                        {
                            geometry.TriangleIndices.Add(face.Indices[0]);
                            geometry.TriangleIndices.Add(face.Indices[1]);
                            geometry.TriangleIndices.Add(face.Indices[2]);
                        }

                    // 材质：默认灰色
                    var material = new DiffuseMaterial(new SolidColorBrush(Colors.LightGray));
                    var model = new GeometryModel3D
                    {
                        Geometry = geometry,
                        Material = material,
                        BackMaterial = material
                    };
                    _models.Add(model);
                    _originalMaterials.Add(material);
                    _originalModelGroup.Children.Add(model);
                }

                // 渲染到 HelixViewport3D
                viewPort.Children.Clear();
                viewPort.Children.Add(new DefaultLights());
                viewPort.Children.Add(new ModelVisual3D { Content = _originalModelGroup });
                
                // 重置相机视角
                viewPort.ZoomExtents();

                // 通知所有插件模型已加载
                foreach (dynamic plugin in _loadedPlugins)
                {
                    plugin.Initialize(_currentModelPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载模型失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void OnGenerateTransparentPreviewClick(object sender, RoutedEventArgs e)
        {
            if (_currentScene == null || _models.Count == 0)
            {
                MessageBox.Show("请先加载模型", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var previewImages = new List<(int index, string materialName, RenderTargetBitmap bitmap)>();

                // 为每个模型生成预览图
                for (int i = 0; i < _models.Count; i++)
                {
                    // 保存当前所有材质
                    var currentMaterials = _models.Select(m => m.Material).ToList();
                    var currentBackMaterials = _models.Select(m => m.BackMaterial).ToList();


                    // 1) 先尝试解析用户输入并限制在 0–255 之间
                    int alphaInt;
                    if (!int.TryParse(txtAlpha.Text, out alphaInt))
                    {
                        alphaInt = 64;   // 解析失败时的默认值
                    }
                    alphaInt = Math.Max(0, Math.Min(255, alphaInt));
                    byte alpha = (byte)alphaInt;


                    // 将其他部分设置为半透明
                    var transparentMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(alpha, 200, 200, 200)));
                    for (int j = 0; j < _models.Count; j++)
                    {
                        if (j != i)
                        {
                            _models[j].Material = transparentMaterial;
                            _models[j].BackMaterial = transparentMaterial;
                        }
                    }

                    // 等待渲染完成
                    await Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => { }));

                    // 创建预览图
                    var renderBitmap = new RenderTargetBitmap(
                        (int)viewPort.ActualWidth,
                        (int)viewPort.ActualHeight,
                        96, 96,
                        PixelFormats.Pbgra32);

                    renderBitmap.Render(viewPort);

                    // 获取材质名称
                    string materialName = "未命名";
                    if (_currentScene.Meshes[i].MaterialIndex >= 0 && 
                        _currentScene.Materials.Count > _currentScene.Meshes[i].MaterialIndex)
                    {
                        materialName = _currentScene.Materials[_currentScene.Meshes[i].MaterialIndex].Name;
                    }

                    previewImages.Add((i + 1, materialName, renderBitmap));

                    // 恢复原始材质
                    for (int j = 0; j < _models.Count; j++)
                    {
                        _models[j].Material = currentMaterials[j];
                        _models[j].BackMaterial = currentBackMaterials[j];
                    }
                }

                // 计算拼接图的布局
                int columns = (int)Math.Ceiling(Math.Sqrt(_models.Count));
                int rows = (int)Math.Ceiling(_models.Count / (double)columns);
                int previewWidth = (int)viewPort.ActualWidth;
                int previewHeight = (int)viewPort.ActualHeight;

                // 创建最终的大图
                var finalBitmap = new WriteableBitmap(
                    previewWidth * columns,
                    previewHeight * rows,
                    96, 96,
                    PixelFormats.Pbgra32,
                    null);

                // 拼接所有预览图
                foreach (var (index, materialName, bitmap) in previewImages)
                {
                    int row = (index - 1) / columns;
                    int col = (index - 1) % columns;
                    var rect = new Int32Rect(col * previewWidth, row * previewHeight, previewWidth, previewHeight);
                    
                    // 将预览图复制到最终图像中
                    int stride = bitmap.PixelWidth * 4;
                    byte[] pixels = new byte[stride * bitmap.PixelHeight];
                    bitmap.CopyPixels(pixels, stride, 0);
                    finalBitmap.WritePixels(rect, pixels, stride, 0);

                    // 添加材质ID和名称文本
                    DrawText(finalBitmap, 
                        $"材质 ID: {index}\n材质名称: {materialName}", 
                        col * previewWidth + 10, 
                        row * previewHeight + 10);
                }

                // 显示预览窗口
                string modelName = Path.GetFileName(_currentModelPath);
                var previewWindow = new PreviewWindow(finalBitmap, modelName);
                previewWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"生成预览图失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void OnGenerateMaterialPreviewClick(object sender, RoutedEventArgs e)
        {
            if (_currentScene == null || _models.Count == 0)
            {
                MessageBox.Show("请先加载模型", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // 创建一个临时的Model3DGroup用于隔离显示
                var isolatedGroup = new Model3DGroup();
                var previewImages = new List<(int index, string materialName, RenderTargetBitmap bitmap)>();

                // 为每个模型生成预览图
                for (int i = 0; i < _models.Count; i++)
                {
                    // 清除当前视图
                    viewPort.Children.Clear();
                    viewPort.Children.Add(new DefaultLights());

                    // 创建隔离视图
                    isolatedGroup.Children.Clear();
                    isolatedGroup.Children.Add(_models[i]);
                    viewPort.Children.Add(new ModelVisual3D { Content = isolatedGroup });

                    // 调整视角以适应当前模型
                    viewPort.ZoomExtents();

                    // 等待渲染完成
                    await Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() => { }));

                    // 创建预览图
                    var renderBitmap = new RenderTargetBitmap(
                        (int)viewPort.ActualWidth,
                        (int)viewPort.ActualHeight,
                        96, 96,
                        PixelFormats.Pbgra32);

                    renderBitmap.Render(viewPort);

                    // 获取材质名称
                    string materialName = "未命名";
                    if (_currentScene.Meshes[i].MaterialIndex >= 0 && 
                        _currentScene.Materials.Count > _currentScene.Meshes[i].MaterialIndex)
                    {
                        materialName = _currentScene.Materials[_currentScene.Meshes[i].MaterialIndex].Name;
                    }

                    previewImages.Add((i + 1, materialName, renderBitmap));
                }

                // 恢复原始视图
                viewPort.Children.Clear();
                viewPort.Children.Add(new DefaultLights());
                viewPort.Children.Add(new ModelVisual3D { Content = _originalModelGroup });
                viewPort.ZoomExtents();

                // 计算拼接图的布局
                int columns = (int)Math.Ceiling(Math.Sqrt(_models.Count));
                int rows = (int)Math.Ceiling(_models.Count / (double)columns);
                int previewWidth = (int)viewPort.ActualWidth;
                int previewHeight = (int)viewPort.ActualHeight;

                // 创建最终的大图
                var finalBitmap = new WriteableBitmap(
                    previewWidth * columns,
                    previewHeight * rows,
                    96, 96,
                    PixelFormats.Pbgra32,
                    null);

                // 拼接所有预览图
                foreach (var (index, materialName, bitmap) in previewImages)
                {
                    int row = (index - 1) / columns;
                    int col = (index - 1) % columns;
                    var rect = new Int32Rect(col * previewWidth, row * previewHeight, previewWidth, previewHeight);
                    
                    // 将预览图复制到最终图像中
                    int stride = bitmap.PixelWidth * 4;
                    byte[] pixels = new byte[stride * bitmap.PixelHeight];
                    bitmap.CopyPixels(pixels, stride, 0);
                    finalBitmap.WritePixels(rect, pixels, stride, 0);

                    // 添加材质ID和名称文本
                    DrawText(finalBitmap, 
                        $"材质 ID: {index}\n材质名称: {materialName}", 
                        col * previewWidth + 10, 
                        row * previewHeight + 10);
                }

                // 显示预览窗口
                string modelName = Path.GetFileName(_currentModelPath);
                var previewWindow = new PreviewWindow(finalBitmap, modelName);
                previewWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"生成预览图失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DrawText(WriteableBitmap bitmap, string text, int x, int y)
        {
            // 创建一个临时的TextBlock来渲染文本
            var textBlock = new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
                Padding = new Thickness(5),
                FontSize = 14
            };

            // 测量文本大小
            textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            textBlock.Arrange(new Rect(textBlock.DesiredSize));

            // 渲染文本到位图
            var textBitmap = new RenderTargetBitmap(
                (int)textBlock.ActualWidth,
                (int)textBlock.ActualHeight,
                96, 96,
                PixelFormats.Pbgra32);
            textBitmap.Render(textBlock);

            // 将文本复制到主图像
            var rect = new Int32Rect(x, y, (int)textBlock.ActualWidth, (int)textBlock.ActualHeight);
            int stride = textBitmap.PixelWidth * 4;
            byte[] pixels = new byte[stride * textBitmap.PixelHeight];
            textBitmap.CopyPixels(pixels, stride, 0);
            bitmap.WritePixels(rect, pixels, stride, 0);
        }

        private void OnShowUVMappingClick(object sender, RoutedEventArgs e)
        {
            if (_currentScene == null)
            {
                MessageBox.Show("请先加载模型", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                string modelName = Path.GetFileName(_currentModelPath);
                var uvViewer = new UVViewerWindow(_currentScene, modelName);
                uvViewer.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开UV查看器失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateVertexColorMesh(GeometryModel3D originalModel, Mesh mesh, List<Color4D> vertexColors)
        {
            // 这个方法现在由 CreateVertexDisplayMesh 替代
            bool showVertexColors = chkVertexColor.IsChecked ?? false;
            bool showVertexAlpha = chkVertexAlpha.IsChecked ?? false;
            CreateVertexDisplayMesh(originalModel, mesh, vertexColors, showVertexColors, showVertexAlpha);
        }

        private void CreateVertexAlphaMesh(GeometryModel3D originalModel, Mesh mesh, List<Color4D> vertexColors)
        {
            // 这个方法现在由 CreateVertexDisplayMesh 替代
            bool showVertexColors = chkVertexColor.IsChecked ?? false;
            bool showVertexAlpha = chkVertexAlpha.IsChecked ?? false;
            CreateVertexDisplayMesh(originalModel, mesh, vertexColors, showVertexColors, showVertexAlpha);
        }
    }
}
