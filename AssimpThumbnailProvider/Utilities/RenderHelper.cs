using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using HelixToolkit.Wpf;
using MediaBrushes = System.Windows.Media.Brushes;
using DrawingPixelFormat = System.Drawing.Imaging.PixelFormat;
using WpfSize = System.Windows.Size;
using WpfRect = System.Windows.Rect;
using WpfColor = System.Windows.Media.Color;
using System.Windows.Controls;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.IO;

namespace AssimpThumbnailProvider.Utilities
{
    public static class RenderHelper
    {
        // 静态的WPF应用程序实例，用于多线程渲染
        private static Application _wpfApp;
        private static Thread _wpfThread;
        private static readonly object _lockObject = new object();
        private static Dispatcher _wpfDispatcher;

        private static void EnsureWpfApplicationExists()
        {
            lock (_lockObject)
            {
                if (_wpfApp != null && _wpfDispatcher != null)
                    return;

                Logger.LogToFile("初始化WPF应用程序线程", "RenderHelper");
                var resetEvent = new ManualResetEventSlim(false);
                Exception initException = null;

                _wpfThread = new Thread(() =>
                {
                    try
                    {
                        // 创建WPF应用程序
                        _wpfApp = new Application();
                        _wpfDispatcher = Dispatcher.CurrentDispatcher;
                        
                        Logger.LogToFile("WPF应用程序已创建，通知主线程", "RenderHelper");
                        resetEvent.Set();
                        
                        // 启动消息泵
                        Logger.LogToFile("启动WPF消息泵", "RenderHelper");
                        Dispatcher.Run();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogToFile($"WPF应用程序初始化失败: {ex.Message}", "RenderHelper");
                        initException = ex;
                        resetEvent.Set();
                    }
                });

                _wpfThread.SetApartmentState(ApartmentState.STA);
                _wpfThread.IsBackground = true; // 设置为后台线程，主程序退出时自动结束
                _wpfThread.Start();

                // 等待WPF应用程序初始化完成
                resetEvent.Wait();
                
                if (initException != null)
                    throw new Exception("WPF应用程序初始化失败", initException);
                    
                Logger.LogToFile("WPF应用程序初始化完成", "RenderHelper");
            }
        }

        public static Bitmap RenderMeshesToBitmap(IEnumerable<MeshData> meshes, int size)
        {
            try
            {
                Logger.LogToFile($"开始渲染网格到位图，大小: {size}x{size}", "RenderHelper");
                
                // 确保WPF应用程序存在
                EnsureWpfApplicationExists();
                
                if (_wpfDispatcher == null)
                {
                    throw new Exception("WPF Dispatcher未初始化");
                }

                // 在WPF线程中执行渲染
                Bitmap result = null;
                Exception renderException = null;

                Logger.LogToFile("在WPF Dispatcher中执行渲染", "RenderHelper");
                _wpfDispatcher.Invoke(() =>
                {
                    try
                    {
                        result = RenderMeshesInWpfThread(meshes, size);
                        Logger.LogToFile("WPF线程渲染完成", "RenderHelper");
                    }
                    catch (Exception ex)
                    {
                        renderException = ex;
                        Logger.LogToFile($"WPF线程中渲染出错: {ex.Message}", "RenderHelper");
                        Logger.LogToFile(ex.StackTrace, "RenderHelper");
                    }
                });

                if (renderException != null)
                    throw new Exception("渲染过程中出错", renderException);

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogToFile($"渲染缩略图时出错: {ex.Message}", "RenderHelper");
                Logger.LogToFile(ex.StackTrace, "RenderHelper");
                throw;
            }
        }

        private static Bitmap RenderMeshesInWpfThread(IEnumerable<MeshData> meshes, int size)
        {
            // 创建和配置Grid来托管视口
            var hostGrid = new Grid
            {
                Width = size,
                Height = size,
                Background = MediaBrushes.Gray
            };

            // 初始化离屏 HelixViewport3D
            var viewport = new HelixViewport3D 
            { 
                Width = size, 
                Height = size,
                Background = MediaBrushes.Gray,
                IsHeadLightEnabled = true,
                ShowViewCube = false,
                ShowFrameRate = false,
                ShowTriangleCountInfo = false,
                ShowCoordinateSystem = false,
                ModelUpDirection = new Vector3D(0, 1, 0),
                ZoomExtentsWhenLoaded = true,
                RotateAroundMouseDownPoint = true
            };
            
            hostGrid.Children.Add(viewport);
            
            // 测量和排列
            hostGrid.Measure(new WpfSize(size, size));
            hostGrid.Arrange(new WpfRect(0, 0, size, size));
            
            // 添加光源
            viewport.Children.Clear();
            
            // 添加多个光源以获得更好的照明效果
            var lightGroup = new Model3DGroup();
            lightGroup.Children.Add(new DirectionalLight(WpfColor.FromRgb(200, 200, 200), new Vector3D(-1, -1, -1)));
            lightGroup.Children.Add(new DirectionalLight(WpfColor.FromRgb(120, 120, 120), new Vector3D(1, -1, 1)));
            lightGroup.Children.Add(new AmbientLight(WpfColor.FromRgb(80, 80, 80)));
            viewport.Children.Add(new ModelVisual3D { Content = lightGroup });

            // 创建一个模型组以便一次性渲染
            var modelGroup = new Model3DGroup();
            bool meshesAdded = false;

            // 创建统一的材质
            var material = new MaterialGroup();
            material.Children.Add(new DiffuseMaterial(new SolidColorBrush(WpfColor.FromRgb(239, 238, 234))));

            foreach (var mesh in meshes)
            {
                if (mesh.Vertices.Count < 3) continue;
                
                var geo = new MeshGeometry3D();
                
                for (int i = 0; i < mesh.Vertices.Count; i += 3)
                {
                    if (i + 2 < mesh.Vertices.Count)
                    {
                        geo.Positions.Add(new Point3D(mesh.Vertices[i], mesh.Vertices[i+1], mesh.Vertices[i+2]));
                    }
                }
                
                for (int i = 0; i < mesh.Normals.Count; i += 3)
                {
                    if (i + 2 < mesh.Normals.Count)
                        geo.Normals.Add(new Vector3D(mesh.Normals[i], mesh.Normals[i+1], mesh.Normals[i+2]));
                }
                
                for (int i = 0; i < mesh.UVs.Count; i += 2)
                {
                    if (i + 1 < mesh.UVs.Count)
                        geo.TextureCoordinates.Add(new System.Windows.Point(mesh.UVs[i], mesh.UVs[i+1]));
                }
                
                geo.TriangleIndices = new Int32Collection(mesh.Indices);
                
                if (geo.Positions.Count == 0 || geo.TriangleIndices.Count == 0)
                    continue;

                var model = new GeometryModel3D
                {
                    Geometry = geo,
                    Material = material,
                    BackMaterial = material
                };
                
                modelGroup.Children.Add(model);
                meshesAdded = true;
            }

            if (!meshesAdded)
            {
                var cube = new BoxVisual3D
                {
                    Center = new Point3D(0, 0, 0),
                    Width = 1,
                    Height = 1,
                    Length = 1,
                    Material = material
                };
                viewport.Children.Add(cube);
            }
            else
            {
                viewport.Children.Add(new ModelVisual3D { Content = modelGroup });
            }
            
            viewport.ZoomExtents();
            
            var cameraOffset = new Vector3D(0.15, 0.2,0.1);

            var scale = 1.2; // 缩放倍数，越大越远
            var basePosition = new Point3D(1.4, 2.8, 1.4);
            viewport.Camera.Position = new Point3D(
                basePosition.X * scale + cameraOffset.X,
                basePosition.Y * scale + cameraOffset.Y,
                basePosition.Z * scale + cameraOffset.Z
            );

            var targetPoint = new Point3D(0, 0, 0) + cameraOffset;
            viewport.Camera.LookDirection = new Vector3D(
                targetPoint.X - viewport.Camera.Position.X,
                targetPoint.Y - viewport.Camera.Position.Y,
                targetPoint.Z - viewport.Camera.Position.Z
            );
            viewport.Camera.UpDirection = new Vector3D(0, 1, 0);

            var rotationGroup = new Transform3DGroup();
            rotationGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), 136)));
            rotationGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), 60)));
            rotationGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), -20)));
            viewport.Camera.Transform = rotationGroup;

            if (viewport.Camera is PerspectiveCamera perspectiveCamera)
            {
                perspectiveCamera.FieldOfView = 42;
            }

            hostGrid.UpdateLayout();
            
            var renderTarget = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
            renderTarget.Render(hostGrid);

            return BitmapFromSource(renderTarget);
        }

        private static Bitmap BitmapFromSource(BitmapSource source)
        {
            var width = source.PixelWidth;
            var height = source.PixelHeight;
            var pixelFormat = DrawingPixelFormat.Format32bppPArgb;

            var bmp = new Bitmap(width, height, pixelFormat);
            var bmpData = bmp.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, pixelFormat);

            source.CopyPixels(Int32Rect.Empty, bmpData.Scan0, bmpData.Height * bmpData.Stride, bmpData.Stride);
            bmp.UnlockBits(bmpData);
            return bmp;
        }

        // 清理资源的方法（可选，在应用程序退出时调用）
        public static void Cleanup()
        {
            lock (_lockObject)
            {
                if (_wpfDispatcher != null)
                {
                    Logger.LogToFile("关闭WPF应用程序", "RenderHelper");
                    _wpfDispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
                    _wpfDispatcher = null;
                    _wpfApp = null;
                }
            }
        }
    }
} 