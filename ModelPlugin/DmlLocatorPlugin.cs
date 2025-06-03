using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace ModelPlugin
{
    public class DmlLocatorPlugin : IModelPlugin
    {
        private string _currentModelPath;

        public string Name => "配置文件定位器";
        public string Description => "定位到ASE文件同目录下的同名配置文件（DML或CHR）";
        public string Version => "1.0.0";

        public void Initialize(string modelPath)
        {
            _currentModelPath = modelPath;
        }

        public Button[] GetButtons()
        {
            var locateButton = new Button
            {
                Content = "定位DML文件",
                Padding = new Thickness(8, 3, 8, 3),
                Margin = new Thickness(0, 0, 5, 0)
            };

            locateButton.Click += OnLocateConfigClick;

            var openButton = new Button
            {
                Content = "用记事本打开DML",
                Padding = new Thickness(8, 3, 8, 3),
                Margin = new Thickness(0, 0, 5, 0)
            };

            openButton.Click += OnOpenConfigClick;

            return new[] { locateButton, openButton };
        }

        private void OnLocateConfigClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var (configPath, configType) = GetConfigPath();
                if (!string.IsNullOrEmpty(configPath) && File.Exists(configPath))
                {
                    Process.Start("explorer.exe", $"/select,\"{configPath}\"");
                }
                else
                {
                    string fileName = Path.GetFileNameWithoutExtension(_currentModelPath);
                    MessageBox.Show($"未找到对应的配置文件：\n{fileName}.dml 或 {fileName}.chr", 
                        "配置文件不存在", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"定位配置文件失败：{ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnOpenConfigClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var (configPath, configType) = GetConfigPath();
                if (!string.IsNullOrEmpty(configPath) && File.Exists(configPath))
                {
                    // 使用记事本打开配置文件
                    Process.Start("notepad.exe", $"\"{configPath}\"");
                }
                else
                {
                    string fileName = Path.GetFileNameWithoutExtension(_currentModelPath);
                    MessageBox.Show($"未找到对应的配置文件：\n{fileName}.dml 或 {fileName}.chr", 
                        "配置文件不存在", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开配置文件失败：{ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private (string path, string type) GetConfigPath()
        {
            if (string.IsNullOrEmpty(_currentModelPath))
                throw new InvalidOperationException("未加载模型文件");

            string directory = Path.GetDirectoryName(_currentModelPath);
            string fileName = Path.GetFileNameWithoutExtension(_currentModelPath);
            
            // 优先查找 .dml 文件
            string dmlPath = Path.Combine(directory, $"{fileName}.dml");
            if (File.Exists(dmlPath))
            {
                return (dmlPath, "dml");
            }
            
            // 如果 .dml 不存在，查找 .chr 文件
            string chrPath = Path.Combine(directory, $"{fileName}.chr");
            if (File.Exists(chrPath))
            {
                return (chrPath, "chr");
            }
            
            // 都不存在，返回空
            return (null, null);
        }
    }
} 