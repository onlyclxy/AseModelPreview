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
        private readonly string[] _supportedExtensions = { ".obj", ".fbx", ".ase", ".gltf", ".glb",".dae" };

        public MainWindow()
        {
            InitializeComponent();
            _models = new List<GeometryModel3D>();
            _originalMaterials = new List<DiffuseMaterial>();
            
            // 设置窗口的拖放事件处理
            this.AllowDrop = true;
            this.DragEnter += OnDragEnter;
            this.Drop += OnDrop;
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

                // 自动保存图片
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string exePath = AppDomain.CurrentDomain.BaseDirectory;
                string imagePath = Path.Combine(exePath, $"材质预览_半透明_{timestamp}.png");

                using (var fileStream = new FileStream(imagePath, FileMode.Create))
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(finalBitmap));
                    encoder.Save(fileStream);
                }

                // 显示预览窗口
                var previewWindow = new PreviewWindow(finalBitmap, imagePath);
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

                // 自动保存图片
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string exePath = AppDomain.CurrentDomain.BaseDirectory;
                string imagePath = Path.Combine(exePath, $"材质预览_孤立_{timestamp}.png");

                using (var fileStream = new FileStream(imagePath, FileMode.Create))
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(finalBitmap));
                    encoder.Save(fileStream);
                }

                // 显示预览窗口
                var previewWindow = new PreviewWindow(finalBitmap, imagePath);
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
    }
}
