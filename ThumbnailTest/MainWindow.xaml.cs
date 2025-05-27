using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using Microsoft.Win32;
using AssimpThumbnailProvider.Utilities;
using System.Windows.Input;
using System.Collections.Generic;

namespace ThumbnailTestConsole
{
    public partial class MainWindow : Window
    {
        private readonly int ThumbnailSize = 256;
        private readonly int MaxThumbnails = 6;
        private int currentIndex = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void PathTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PathTextBox.Text == "将文件拖放到此处或输入路径...")
            {
                PathTextBox.Text = string.Empty;
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "支持的3D模型|*.ase;*.fbx;*.obj;*.gltf;*.glb;*.dae|所有文件|*.*",
                Multiselect = true
            };

            if (dialog.ShowDialog() == true)
            {
                ProcessFiles(dialog.FileNames);
            }
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            string path = PathTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(path) || path == "将文件拖放到此处或输入路径...")
            {
                MessageBox.Show("请输入有效的文件或文件夹路径！", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ProcessPath(path);
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                ProcessFiles(files);
            }
        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void ProcessPath(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    var files = Directory.GetFiles(path, "*.*")
                        .Where(f => new[] { ".ase", ".fbx", ".obj", ".gltf", ".glb",".dae" }
                            .Contains(Path.GetExtension(f).ToLower()))
                        .ToArray();
                    ProcessFiles(files);
                }
                else if (File.Exists(path))
                {
                    ProcessFiles(new[] { path });
                }
                else
                {
                    MessageBox.Show($"路径不存在：{path}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"处理路径时出错：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProcessFiles(string[] files)
        {
            if (files == null || files.Length == 0) return;

            Mouse.OverrideCursor = Cursors.Wait;
            StatusText.Text = "正在处理...";

            try
            {
                foreach (var file in files)
                {
                    try
                    {
                        var loader = new AssimpLoader();
                        var meshes = loader.Load(file);
                        var bitmap = RenderHelper.RenderMeshesToBitmap(meshes, ThumbnailSize);

                        // 创建预览卡片
                        var card = new Border
                        {
                            Margin = new Thickness(5),
                            BorderBrush = Brushes.LightGray,
                            BorderThickness = new Thickness(1),
                            Padding = new Thickness(5),
                            Width = ThumbnailSize + 20,  // 添加一些边距
                            Height = ThumbnailSize + 40  // 为文件名留出空间
                        };

                        var stack = new StackPanel();
                        
                        // 添加图像
                        var image = new Image
                        {
                            Width = ThumbnailSize,
                            Height = ThumbnailSize,
                            Source = ConvertBitmapToImageSource(bitmap),
                            Stretch = Stretch.Uniform
                        };
                        stack.Children.Add(image);

                        // 添加文件名
                        stack.Children.Add(new TextBlock
                        {
                            Text = Path.GetFileName(file),
                            TextAlignment = TextAlignment.Center,
                            TextWrapping = TextWrapping.Wrap,
                            Margin = new Thickness(0, 5, 0, 0)
                        });

                        card.Child = stack;

                        // 按顺序替换缩略图
                        if (currentIndex < MaxThumbnails)
                        {
                            if (currentIndex < PreviewPanel.Children.Count)
                            {
                                PreviewPanel.Children.RemoveAt(currentIndex);
                                PreviewPanel.Children.Insert(currentIndex, card);
                            }
                            else
                            {
                                PreviewPanel.Children.Add(card);
                            }
                        }
                        else
                        {
                            currentIndex = 0;
                            PreviewPanel.Children.RemoveAt(currentIndex);
                            PreviewPanel.Children.Insert(currentIndex, card);
                        }
                        currentIndex++;

                        StatusText.Text = $"已处理：{Path.GetFileName(file)}";
                    }
                    catch (Exception ex)
                    {
                        // 创建错误提示卡片
                        var errorCard = new Border
                        {
                            Margin = new Thickness(5),
                            BorderBrush = Brushes.Red,
                            BorderThickness = new Thickness(1),
                            Padding = new Thickness(5),
                            Width = ThumbnailSize + 20,
                            Height = ThumbnailSize + 40
                        };

                        var errorStack = new StackPanel();
                        errorStack.Children.Add(new TextBlock
                        {
                            Text = Path.GetFileName(file),
                            TextAlignment = TextAlignment.Center,
                            Foreground = Brushes.Red
                        });
                        errorStack.Children.Add(new TextBlock
                        {
                            Text = $"错误: {ex.Message}",
                            TextAlignment = TextAlignment.Center,
                            TextWrapping = TextWrapping.Wrap,
                            Foreground = Brushes.Red,
                            Width = ThumbnailSize
                        });

                        errorCard.Child = errorStack;

                        // 按顺序替换错误卡片
                        if (currentIndex < MaxThumbnails)
                        {
                            if (currentIndex < PreviewPanel.Children.Count)
                            {
                                PreviewPanel.Children.RemoveAt(currentIndex);
                                PreviewPanel.Children.Insert(currentIndex, errorCard);
                            }
                            else
                            {
                                PreviewPanel.Children.Add(errorCard);
                            }
                        }
                        else
                        {
                            currentIndex = 0;
                            PreviewPanel.Children.RemoveAt(currentIndex);
                            PreviewPanel.Children.Insert(currentIndex, errorCard);
                        }
                        currentIndex++;
                    }
                }

                StatusText.Text = $"完成，共处理 {files.Length} 个文件";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"处理文件时出错：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "处理出错";
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private ImageSource ConvertBitmapToImageSource(System.Drawing.Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // 使图像可以跨线程使用

                return bitmapImage;
            }
        }
    }
} 