using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Assimp;
using Microsoft.Win32;
using System.IO;
using IOPath = System.IO.Path;  // 使用别名避免命名空间冲突

namespace AseTest
{
    /// <summary>
    /// UVViewerWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UVViewerWindow : Window
    {
        private Scene _scene;
        private string _modelName;
        private bool _showWireframe = true;
        private bool _showAllMeshes = false;

        // 缓存相关变量
        private Canvas _cachedUVCanvas = null;
        private string _currentCacheKey = "";

        // 为不同网格定义不同的颜色
        private readonly Color[] _meshColors = new Color[]
        {
            Colors.Red, Colors.Blue, Colors.Green, Colors.Orange, Colors.Purple,
            Colors.Brown, Colors.Pink, Colors.Cyan, Colors.Magenta, Colors.Yellow,
            Colors.DarkBlue, Colors.DarkGreen, Colors.DarkRed, Colors.DarkCyan, Colors.DarkMagenta
        };

        public UVViewerWindow(Scene scene, string modelName)
        {
            InitializeComponent();
            _scene = scene;
            _modelName = modelName;
            
            Title = $"UV映射查看器 - {modelName}";
            
            InitializeControls();
            LoadUVChannels();
            LoadMeshes();
            SetDefaultSelections();
        }

        private void InitializeControls()
        {
            // 设置默认背景为白色
            cmbBackground.SelectedIndex = 0;
            
            // 默认显示线框
            chkShowWireframe.IsChecked = true;
            _showWireframe = true;
        }

        private void LoadUVChannels()
        {
            cmbUVChannels.Items.Clear();
            
            // 检查所有网格中的最大UV通道数
            int maxChannels = 0;
            foreach (var mesh in _scene.Meshes)
            {
                if (mesh.TextureCoordinateChannels != null)
                {
                    for (int i = 0; i < mesh.TextureCoordinateChannels.Length; i++)
                    {
                        if (mesh.TextureCoordinateChannels[i].Count > 0)
                        {
                            maxChannels = Math.Max(maxChannels, i + 1);
                        }
                    }
                }
            }

            // 添加UV通道选项
            for (int i = 0; i < Math.Max(1, maxChannels); i++)
            {
                cmbUVChannels.Items.Add($"UV{i}");
            }
            
            if (cmbUVChannels.Items.Count > 0)
                cmbUVChannels.SelectedIndex = 0;
        }

        private void LoadMeshes()
        {
            cmbMeshes.Items.Clear();
            
            for (int i = 0; i < _scene.Meshes.Count; i++)
            {
                string meshName = string.IsNullOrEmpty(_scene.Meshes[i].Name) 
                    ? $"网格 {i + 1}" 
                    : _scene.Meshes[i].Name;
                cmbMeshes.Items.Add(meshName);
            }
            
            if (cmbMeshes.Items.Count > 0)
                cmbMeshes.SelectedIndex = 0;
        }

        private void SetDefaultSelections()
        {
            if (cmbUVChannels.Items.Count > 0 && cmbMeshes.Items.Count > 0)
            {
                DisplayUV();
            }
        }

        private void OnUVChannelSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearCache(); // 清空缓存，因为UV通道改变了
            DisplayUV();
        }

        private void OnMeshSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_showAllMeshes)
            {
                ClearCache(); // 清空缓存，因为网格选择改变了
                DisplayUV();
            }
        }

        private void OnShowAllMeshesChanged(object sender, RoutedEventArgs e)
        {
            bool newShowAllMeshes = chkShowAllMeshes.IsChecked ?? false;
            
            // 如果用户勾选了"显示所有网格"，显示确认对话框
            if (newShowAllMeshes && !_showAllMeshes)
            {
                var result = MessageBox.Show(
                    "显示所有网格的UV映射可能会很慢，特别是对于包含大量网格和面片的复杂模型。\n\n" +
                    "您确定要继续吗？",
                    "性能警告",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.No)
                {
                    // 用户选择取消，恢复复选框状态
                    chkShowAllMeshes.IsChecked = false;
                    return;
                }
            }
            
            _showAllMeshes = newShowAllMeshes;
            
            // 当选择显示所有网格时，禁用网格选择下拉框
            cmbMeshes.IsEnabled = !_showAllMeshes;
            
            ClearCache(); // 清空缓存，因为显示模式改变了
            DisplayUV();
        }

        private void OnBackgroundChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateBackground();
            ClearCache(); // 清空缓存，因为背景改变了
            DisplayUV();
        }

        private void OnShowWireframeChanged(object sender, RoutedEventArgs e)
        {
            _showWireframe = chkShowWireframe.IsChecked ?? false;
            ClearCache(); // 清空缓存，因为线框显示设置改变了
            DisplayUV();
        }

        private void UpdateBackground()
        {
            if (cmbBackground.SelectedItem is ComboBoxItem selectedItem)
            {
                string tag = selectedItem.Tag?.ToString();
                switch (tag)
                {
                    case "White":
                        uvDisplayArea.Background = Brushes.White;
                        break;
                    case "Black":
                        uvDisplayArea.Background = Brushes.Black;
                        break;
                    case "Gray":
                        uvDisplayArea.Background = Brushes.Gray;
                        break;
                    case "Checkerboard":
                        uvDisplayArea.Background = CreateCheckerboardBrush();
                        break;
                    default:
                        uvDisplayArea.Background = Brushes.White;
                        break;
                }
            }
        }

        private Brush CreateCheckerboardBrush()
        {
            var drawingBrush = new DrawingBrush();
            var geometryDrawing = new GeometryDrawing();
            
            geometryDrawing.Geometry = new RectangleGeometry(new Rect(0, 0, 32, 32));
            geometryDrawing.Brush = Brushes.LightGray;
            
            var drawingGroup = new DrawingGroup();
            drawingGroup.Children.Add(geometryDrawing);
            
            var checkerGeometry = new GeometryDrawing();
            var combinedGeometry = new GeometryGroup();
            combinedGeometry.Children.Add(new RectangleGeometry(new Rect(0, 0, 16, 16)));
            combinedGeometry.Children.Add(new RectangleGeometry(new Rect(16, 16, 16, 16)));
            
            checkerGeometry.Geometry = combinedGeometry;
            checkerGeometry.Brush = Brushes.White;
            drawingGroup.Children.Add(checkerGeometry);
            
            drawingBrush.Drawing = drawingGroup;
            drawingBrush.TileMode = TileMode.Tile;
            drawingBrush.Viewport = new Rect(0, 0, 32, 32);
            drawingBrush.ViewportUnits = BrushMappingMode.Absolute;
            
            return drawingBrush;
        }

        private void DisplayUV()
        {
            if (_scene == null || cmbUVChannels.SelectedIndex < 0)
            {
                uvCanvas.Children.Clear();
                return;
            }

            try
            {
                // 生成当前设置的缓存键
                string newCacheKey = GenerateCacheKey();
                
                // 检查是否可以使用缓存
                if (_cachedUVCanvas != null && _currentCacheKey == newCacheKey)
                {
                    // 使用缓存的画布
                    uvCanvas.Children.Clear();
                    var clonedCanvas = CloneCanvas(_cachedUVCanvas);
                    
                    // 将克隆的元素添加到当前画布
                    foreach (UIElement child in clonedCanvas.Children)
                    {
                        uvCanvas.Children.Add(child);
                    }
                    
                    txtUVInfo.Text += " (已缓存)";
                    return;
                }

                // 清空当前画布
                uvCanvas.Children.Clear();

                int uvChannel = cmbUVChannels.SelectedIndex;

                if (_showAllMeshes)
                {
                    // 显示所有网格的UV
                    DisplayAllMeshesUV(uvChannel);
                }
                else
                {
                    // 显示单个网格的UV
                    DisplaySingleMeshUV(uvChannel);
                }

                // 生成完成后，缓存当前画布（只对显示所有网格的情况进行缓存，因为这是最耗时的）
                if (_showAllMeshes)
                {
                    _cachedUVCanvas = CloneCanvas(uvCanvas);
                    _currentCacheKey = newCacheKey;
                    txtUVInfo.Text += " (已生成缓存)";
                }
            }
            catch (Exception ex)
            {
                txtUVInfo.Text = $"显示UV失败: {ex.Message}";
            }
        }

        private void DisplayAllMeshesUV(int uvChannel)
        {
            int validMeshCount = 0;
            int totalPoints = 0;
            int totalFaces = 0;

            for (int meshIndex = 0; meshIndex < _scene.Meshes.Count; meshIndex++)
            {
                var mesh = _scene.Meshes[meshIndex];
                
                if (mesh.TextureCoordinateChannels == null || 
                    uvChannel >= mesh.TextureCoordinateChannels.Length ||
                    mesh.TextureCoordinateChannels[uvChannel].Count == 0)
                    continue;

                var uvCoords = mesh.TextureCoordinateChannels[uvChannel];
                if (uvCoords.Count == 0)
                    continue;

                validMeshCount++;
                totalPoints += uvCoords.Count;

                // 为当前网格选择颜色
                Color meshColor = _meshColors[meshIndex % _meshColors.Length];
                var brush = new SolidColorBrush(meshColor);
                var strokeBrush = new SolidColorBrush(Color.FromRgb(
                    (byte)Math.Max(0, meshColor.R - 50),
                    (byte)Math.Max(0, meshColor.G - 50),
                    (byte)Math.Max(0, meshColor.B - 50)));

                // 绘制UV点
                foreach (var uv in uvCoords)
                {
                    double x = uv.X * 512;
                    double y = (1 - uv.Y) * 512; // Y轴翻转

                    var point = new Ellipse
                    {
                        Width = 3,
                        Height = 3,
                        Fill = brush,
                        Stroke = strokeBrush,
                        StrokeThickness = 0.5
                    };

                    Canvas.SetLeft(point, x - 1.5);
                    Canvas.SetTop(point, y - 1.5);
                    uvCanvas.Children.Add(point);
                }

                // 绘制UV面片（如果显示线框）
                if (_showWireframe)
                {
                    foreach (var face in mesh.Faces)
                    {
                        if (face.IndexCount >= 3)
                        {
                            totalFaces++;

                            for (int i = 0; i < face.IndexCount; i++)
                            {
                                int currentIndex = face.Indices[i];
                                int nextIndex = face.Indices[(i + 1) % face.IndexCount];

                                if (currentIndex < uvCoords.Count && nextIndex < uvCoords.Count)
                                {
                                    var uv1 = uvCoords[currentIndex];
                                    var uv2 = uvCoords[nextIndex];

                                    double x1 = uv1.X * 512;
                                    double y1 = (1 - uv1.Y) * 512;
                                    double x2 = uv2.X * 512;
                                    double y2 = (1 - uv2.Y) * 512;

                                    var line = new Line
                                    {
                                        X1 = x1,
                                        Y1 = y1,
                                        X2 = x2,
                                        Y2 = y2,
                                        Stroke = strokeBrush,
                                        StrokeThickness = 1,
                                        Opacity = 0.7
                                    };

                                    uvCanvas.Children.Add(line);
                                }
                            }
                        }
                    }
                }
            }

            // 更新状态信息
            if (validMeshCount > 0)
            {
                txtUVInfo.Text = $"显示所有网格 - UV通道: {uvChannel}, " +
                               $"有效网格: {validMeshCount}/{_scene.Meshes.Count}, " +
                               $"总UV点: {totalPoints}" +
                               (_showWireframe ? $", 总面片: {totalFaces}" : "");
            }
            else
            {
                txtUVInfo.Text = $"UV通道 {uvChannel} 在所有网格中都没有数据";
            }
        }

        private void DisplaySingleMeshUV(int uvChannel)
        {
            if (cmbMeshes.SelectedIndex < 0 || cmbMeshes.SelectedIndex >= _scene.Meshes.Count)
                return;

            var mesh = _scene.Meshes[cmbMeshes.SelectedIndex];
            
            if (mesh.TextureCoordinateChannels == null || 
                uvChannel >= mesh.TextureCoordinateChannels.Length ||
                mesh.TextureCoordinateChannels[uvChannel].Count == 0)
            {
                txtUVInfo.Text = $"网格 {cmbMeshes.SelectedIndex + 1} 没有UV通道 {uvChannel} 数据";
                return;
            }

            var uvCoords = mesh.TextureCoordinateChannels[uvChannel];
            if (uvCoords.Count == 0)
            {
                txtUVInfo.Text = $"网格 {cmbMeshes.SelectedIndex + 1} 的UV通道 {uvChannel} 为空";
                return;
            }

            // 使用默认颜色
            var pointBrush = Brushes.Red;
            var lineBrush = Brushes.Blue;

            // 绘制UV点
            foreach (var uv in uvCoords)
            {
                double x = uv.X * 512;
                double y = (1 - uv.Y) * 512; // Y轴翻转

                var point = new Ellipse
                {
                    Width = 4,
                    Height = 4,
                    Fill = pointBrush,
                    Stroke = Brushes.DarkRed,
                    StrokeThickness = 1
                };

                Canvas.SetLeft(point, x - 2);
                Canvas.SetTop(point, y - 2);
                uvCanvas.Children.Add(point);
            }

            // 绘制UV面片（如果显示线框）
            int faceCount = 0;
            if (_showWireframe)
            {
                foreach (var face in mesh.Faces)
                {
                    if (face.IndexCount >= 3)
                    {
                        faceCount++;

                        for (int i = 0; i < face.IndexCount; i++)
                        {
                            int currentIndex = face.Indices[i];
                            int nextIndex = face.Indices[(i + 1) % face.IndexCount];

                            if (currentIndex < uvCoords.Count && nextIndex < uvCoords.Count)
                            {
                                var uv1 = uvCoords[currentIndex];
                                var uv2 = uvCoords[nextIndex];

                                double x1 = uv1.X * 512;
                                double y1 = (1 - uv1.Y) * 512;
                                double x2 = uv2.X * 512;
                                double y2 = (1 - uv2.Y) * 512;

                                var line = new Line
                                {
                                    X1 = x1,
                                    Y1 = y1,
                                    X2 = x2,
                                    Y2 = y2,
                                    Stroke = lineBrush,
                                    StrokeThickness = 1
                                };

                                uvCanvas.Children.Add(line);
                            }
                        }
                    }
                }
            }

            // 更新状态信息
            string meshName = string.IsNullOrEmpty(mesh.Name) 
                ? $"网格 {cmbMeshes.SelectedIndex + 1}" 
                : mesh.Name;
            txtUVInfo.Text = $"{meshName} - UV通道: {uvChannel}, UV点数: {uvCoords.Count}" +
                           (_showWireframe ? $", 面片数: {faceCount}" : "");
        }

        private void OnSaveUVClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "PNG图像|*.png|JPEG图像|*.jpg|所有文件|*.*",
                    DefaultExt = "png",
                    FileName = $"{IOPath.GetFileNameWithoutExtension(_modelName)}_UV.png"
                };

                if (dialog.ShowDialog() == true)
                {
                    // 创建渲染目标
                    var renderBitmap = new RenderTargetBitmap(
                        512, 512, 96, 96, PixelFormats.Pbgra32);

                    // 渲染UV Canvas
                    renderBitmap.Render(uvCanvas);

                    // 保存图像
                    BitmapEncoder encoder;
                    string extension = IOPath.GetExtension(dialog.FileName).ToLower();
                    if (extension == ".jpg" || extension == ".jpeg")
                    {
                        encoder = new JpegBitmapEncoder { QualityLevel = 95 };
                    }
                    else
                    {
                        encoder = new PngBitmapEncoder();
                    }

                    encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                    using (var stream = File.Create(dialog.FileName))
                    {
                        encoder.Save(stream);
                    }

                    MessageBox.Show($"UV图已保存到: {dialog.FileName}", "保存成功", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存UV图失败: {ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerateCacheKey()
        {
            string backgroundTag = "White";
            if (cmbBackground.SelectedItem is ComboBoxItem selectedItem)
            {
                backgroundTag = selectedItem.Tag?.ToString() ?? "White";
            }

            return $"UV{cmbUVChannels.SelectedIndex}_" +
                   $"AllMeshes{_showAllMeshes}_" +
                   $"Mesh{cmbMeshes.SelectedIndex}_" +
                   $"Wire{_showWireframe}_" +
                   $"Bg{backgroundTag}";
        }

        private void ClearCache()
        {
            _cachedUVCanvas = null;
            _currentCacheKey = "";
        }

        private Canvas CloneCanvas(Canvas original)
        {
            var clone = new Canvas
            {
                Width = original.Width,
                Height = original.Height,
                Background = original.Background
            };

            foreach (UIElement child in original.Children)
            {
                if (child is Ellipse ellipse)
                {
                    var clonedEllipse = new Ellipse
                    {
                        Width = ellipse.Width,
                        Height = ellipse.Height,
                        Fill = ellipse.Fill,
                        Stroke = ellipse.Stroke,
                        StrokeThickness = ellipse.StrokeThickness,
                        Opacity = ellipse.Opacity
                    };
                    Canvas.SetLeft(clonedEllipse, Canvas.GetLeft(ellipse));
                    Canvas.SetTop(clonedEllipse, Canvas.GetTop(ellipse));
                    clone.Children.Add(clonedEllipse);
                }
                else if (child is Line line)
                {
                    var clonedLine = new Line
                    {
                        X1 = line.X1,
                        Y1 = line.Y1,
                        X2 = line.X2,
                        Y2 = line.Y2,
                        Stroke = line.Stroke,
                        StrokeThickness = line.StrokeThickness,
                        Opacity = line.Opacity
                    };
                    clone.Children.Add(clonedLine);
                }
            }

            return clone;
        }
    }
} 