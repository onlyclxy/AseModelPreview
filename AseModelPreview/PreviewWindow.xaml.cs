using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace AseTest
{
    /// <summary>
    /// PreviewWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PreviewWindow : Window
    {
        private BitmapSource _previewBitmap;
        private string _modelName;
        private Point? lastDragPosition;
        private double currentScale = 1.0;
        private const double ScaleRate = 1.1;
        private const double MinScale = 0.1;
        private const double MaxScale = 10.0;
        private readonly ScaleTransform scaleTransform = new ScaleTransform(1, 1);
        private bool isDragging = false;

        public PreviewWindow(BitmapSource previewBitmap, string modelName = "未知模型")
        {
            InitializeComponent();
            _previewBitmap = previewBitmap;
            _modelName = SanitizeFileName(modelName);
            previewImage.Source = previewBitmap;
            
            // 设置图片变换
            previewImage.LayoutTransform = scaleTransform;

            // 调整窗口大小以适应图片（但不超过屏幕大小）
            double screenWidth = SystemParameters.WorkArea.Width;
            double screenHeight = SystemParameters.WorkArea.Height;
            double imageWidth = previewBitmap.PixelWidth;
            double imageHeight = previewBitmap.PixelHeight;

            // 计算合适的窗口大小（保持宽高比，但不超过屏幕的80%）
            double scale = Math.Min(
                screenWidth * 0.8 / imageWidth,
                screenHeight * 0.8 / imageHeight
            );

            if (scale < 1)
            {
                this.Width = imageWidth * scale + 50; // 添加一些边距
                this.Height = imageHeight * scale + 100; // 为顶部和底部控件留出空间
            }
            else
            {
                this.Width = Math.Min(imageWidth + 50, screenWidth * 0.8);
                this.Height = Math.Min(imageHeight + 100, screenHeight * 0.8);
            }
        }

        private string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "未知模型";

            // 保留完整的文件名（包括扩展名）
            string cleanFileName = fileName;
            
            // 移除或替换不允许在文件名中使用的字符
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                cleanFileName = cleanFileName.Replace(c, '_');
            }
            
            // 限制长度，避免文件名过长
            if (cleanFileName.Length > 50)
                cleanFileName = cleanFileName.Substring(0, 50);
                
            return cleanFileName;
        }

        private void OnSaveImageClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // 创建更清晰的时间戳格式：年-月-日_时-分-秒
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                
                var saveDialog = new SaveFileDialog
                {
                    Title = "保存预览图",
                    Filter = "PNG图片 (*.png)|*.png|JPEG图片 (*.jpg)|*.jpg|所有文件 (*.*)|*.*",
                    DefaultExt = "png",
                    FileName = $"[材质预览]_{_modelName}_{timestamp}.png"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    string filePath = saveDialog.FileName;
                    string extension = Path.GetExtension(filePath).ToLower();

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        BitmapEncoder encoder;
                        switch (extension)
                        {
                            case ".jpg":
                            case ".jpeg":
                                encoder = new JpegBitmapEncoder();
                                break;
                            case ".png":
                            default:
                                encoder = new PngBitmapEncoder();
                                break;
                        }
                        
                        encoder.Frames.Add(BitmapFrame.Create(_previewBitmap));
                        encoder.Save(fileStream);
                    }

                    txtImagePath.Text = $"已保存至: {filePath}";
                    MessageBox.Show($"图片已成功保存至:\n{filePath}", "保存成功", 
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存图片失败: {ex.Message}", "错误", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnOpenImageFolderClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string imagePath = txtImagePath.Text;
                if (imagePath.StartsWith("已保存至: "))
                {
                    imagePath = imagePath.Substring("已保存至: ".Length);
                    if (File.Exists(imagePath))
                    {
                        Process.Start("explorer.exe", $"/select,\"{imagePath}\"");
                        return;
                    }
                }
                MessageBox.Show("请先保存图片", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开目录失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double scale = e.Delta > 0 ? ScaleRate : 1 / ScaleRate;
            double newScale = currentScale * scale;

            if (newScale >= MinScale && newScale <= MaxScale)
            {
                // 获取鼠标相对于图片的位置
                Point mousePos = e.GetPosition(previewImage);
                
                // 获取鼠标相对于可视区域的位置（百分比）
                Point scrollMousePos = e.GetPosition(scrollViewer);
                double scrollXRatio = scrollMousePos.X / scrollViewer.ViewportWidth;
                double scrollYRatio = scrollMousePos.Y / scrollViewer.ViewportHeight;

                // 更新缩放比例
                currentScale = newScale;
                scaleTransform.ScaleX = currentScale;
                scaleTransform.ScaleY = currentScale;
                txtZoomLevel.Text = $"缩放: {(currentScale * 100):F0}%";

                // 等待布局更新
                scrollViewer.UpdateLayout();

                // 计算新的滚动位置
                double newScrollX = (mousePos.X * currentScale) - (scrollViewer.ViewportWidth * scrollXRatio);
                double newScrollY = (mousePos.Y * currentScale) - (scrollViewer.ViewportHeight * scrollYRatio);

                // 设置新的滚动位置
                scrollViewer.ScrollToHorizontalOffset(newScrollX);
                scrollViewer.ScrollToVerticalOffset(newScrollY);
            }

            e.Handled = true;
        }

        private void StartDragging(MouseEventArgs e)
        {
            if (imageContainer.IsMouseOver && !isDragging)
            {
                lastDragPosition = e.GetPosition(scrollViewer);
                imageContainer.CaptureMouse();
                isDragging = true;
                Cursor = Cursors.Hand;
            }
        }

        private void StopDragging()
        {
            if (isDragging)
            {
                lastDragPosition = null;
                imageContainer.ReleaseMouseCapture();
                isDragging = false;
                Cursor = Cursors.Arrow;
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StartDragging(e);
        }

        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            StartDragging(e);
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            StopDragging();
        }

        private void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            StopDragging();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && lastDragPosition.HasValue && 
                (e.LeftButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed))
            {
                Point currentPosition = e.GetPosition(scrollViewer);
                double deltaX = currentPosition.X - lastDragPosition.Value.X;
                double deltaY = currentPosition.Y - lastDragPosition.Value.Y;

                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - deltaX);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - deltaY);

                lastDragPosition = currentPosition;
            }
            else if (isDragging && e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released)
            {
                // 如果两个按钮都释放了，但状态还是拖动，则停止拖动
                StopDragging();
            }
        }
    }
} 