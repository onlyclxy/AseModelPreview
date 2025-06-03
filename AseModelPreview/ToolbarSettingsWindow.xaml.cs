using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;

namespace AseTest
{
    /// <summary>
    /// ToolbarSettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ToolbarSettingsWindow : Window
    {
        private Dictionary<string, CheckBox> _coreButtons;
        private Dictionary<string, CheckBox> _vertexDisplayButtons;
        private Dictionary<string, CheckBox> _pluginButtons;
        private ToolbarSettings _settings;

        public ToolbarSettings Settings => _settings;

        public ToolbarSettingsWindow(ToolbarSettings currentSettings, List<string> pluginNames)
        {
            InitializeComponent();
            
            _settings = new ToolbarSettings(currentSettings);
            _coreButtons = new Dictionary<string, CheckBox>();
            _vertexDisplayButtons = new Dictionary<string, CheckBox>();
            _pluginButtons = new Dictionary<string, CheckBox>();

            InitializeCoreButtons();
            InitializeVertexDisplayButtons();
            InitializePluginButtons(pluginNames);
            LoadCurrentSettings();
        }

        private void InitializeCoreButtons()
        {
            _coreButtons["LoadModel"] = chkLoadModel;
            _coreButtons["MaterialPreview"] = chkMaterialPreview;
            _coreButtons["TransparentPreview"] = chkTransparentPreview;
            _coreButtons["UVMapping"] = chkUVMapping;
            _coreButtons["AlphaEditor"] = chkAlphaEditor;
        }

        private void InitializeVertexDisplayButtons()
        {
            _vertexDisplayButtons["VertexColor"] = chkVertexColor;
            _vertexDisplayButtons["VertexAlpha"] = chkVertexAlpha;
        }

        private void InitializePluginButtons(List<string> pluginNames)
        {
            pluginCheckBoxPanel.Children.Clear();
            _pluginButtons.Clear();

            if (pluginNames == null || pluginNames.Count == 0)
            {
                grpPlugins.Visibility = Visibility.Collapsed;
                return;
            }

            foreach (string pluginName in pluginNames)
            {
                var checkBox = new CheckBox
                {
                    Content = pluginName,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                
                _pluginButtons[pluginName] = checkBox;
                pluginCheckBoxPanel.Children.Add(checkBox);
            }
        }

        private void LoadCurrentSettings()
        {
            // 加载核心按钮设置
            foreach (var kvp in _coreButtons)
            {
                kvp.Value.IsChecked = _settings.CoreButtons.ContainsKey(kvp.Key) ? _settings.CoreButtons[kvp.Key] : true;
            }

            // 加载顶点显示按钮设置
            foreach (var kvp in _vertexDisplayButtons)
            {
                kvp.Value.IsChecked = _settings.VertexDisplayButtons.ContainsKey(kvp.Key) ? _settings.VertexDisplayButtons[kvp.Key] : true;
            }

            // 加载插件按钮设置
            foreach (var kvp in _pluginButtons)
            {
                kvp.Value.IsChecked = _settings.PluginButtons.ContainsKey(kvp.Key) ? _settings.PluginButtons[kvp.Key] : true;
            }

            // 加载其他设置
            chkCompactMode.IsChecked = _settings.CompactMode;
            chkShowButtonText.IsChecked = _settings.ShowButtonText;
        }

        private void SaveCurrentSettings()
        {
            // 保存核心按钮设置
            _settings.CoreButtons.Clear();
            foreach (var kvp in _coreButtons)
            {
                _settings.CoreButtons[kvp.Key] = kvp.Value.IsChecked ?? false;
            }

            // 保存顶点显示按钮设置
            _settings.VertexDisplayButtons.Clear();
            foreach (var kvp in _vertexDisplayButtons)
            {
                _settings.VertexDisplayButtons[kvp.Key] = kvp.Value.IsChecked ?? false;
            }

            // 保存插件按钮设置
            _settings.PluginButtons.Clear();
            foreach (var kvp in _pluginButtons)
            {
                _settings.PluginButtons[kvp.Key] = kvp.Value.IsChecked ?? false;
            }

            // 保存其他设置
            _settings.CompactMode = chkCompactMode.IsChecked ?? false;
            _settings.ShowButtonText = chkShowButtonText.IsChecked ?? true;
        }

        private void OnShowAllClick(object sender, RoutedEventArgs e)
        {
            foreach (var checkBox in _coreButtons.Values)
                checkBox.IsChecked = true;
            
            foreach (var checkBox in _vertexDisplayButtons.Values)
                checkBox.IsChecked = true;
            
            foreach (var checkBox in _pluginButtons.Values)
                checkBox.IsChecked = true;
        }

        private void OnHideAllClick(object sender, RoutedEventArgs e)
        {
            foreach (var checkBox in _coreButtons.Values)
                checkBox.IsChecked = false;
            
            foreach (var checkBox in _vertexDisplayButtons.Values)
                checkBox.IsChecked = false;
            
            foreach (var checkBox in _pluginButtons.Values)
                checkBox.IsChecked = false;
        }

        private void OnRestoreDefaultClick(object sender, RoutedEventArgs e)
        {
            // 恢复默认设置：核心功能全部显示，插件按需显示
            chkLoadModel.IsChecked = true;
            chkMaterialPreview.IsChecked = true;
            chkTransparentPreview.IsChecked = true;
            chkUVMapping.IsChecked = true;
            chkAlphaEditor.IsChecked = true;

            // 顶点显示功能默认显示
            chkVertexColor.IsChecked = true;
            chkVertexAlpha.IsChecked = true;

            foreach (var checkBox in _pluginButtons.Values)
                checkBox.IsChecked = true;

            chkCompactMode.IsChecked = false;
            chkShowButtonText.IsChecked = true;
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            SaveCurrentSettings();
            DialogResult = true;
            Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    /// <summary>
    /// 工具栏设置类
    /// </summary>
    public class ToolbarSettings
    {
        public Dictionary<string, bool> CoreButtons { get; set; }
        public Dictionary<string, bool> VertexDisplayButtons { get; set; }
        public Dictionary<string, bool> PluginButtons { get; set; }
        public bool CompactMode { get; set; }
        public bool ShowButtonText { get; set; }

        public ToolbarSettings()
        {
            CoreButtons = new Dictionary<string, bool>();
            VertexDisplayButtons = new Dictionary<string, bool>();
            PluginButtons = new Dictionary<string, bool>();
            CompactMode = false;
            ShowButtonText = true;
        }

        public ToolbarSettings(ToolbarSettings source) : this()
        {
            if (source != null)
            {
                foreach (var kvp in source.CoreButtons)
                    CoreButtons[kvp.Key] = kvp.Value;
                
                foreach (var kvp in source.VertexDisplayButtons)
                    VertexDisplayButtons[kvp.Key] = kvp.Value;
                
                foreach (var kvp in source.PluginButtons)
                    PluginButtons[kvp.Key] = kvp.Value;
                
                CompactMode = source.CompactMode;
                ShowButtonText = source.ShowButtonText;
            }
        }

        /// <summary>
        /// 保存设置到配置文件
        /// </summary>
        /// <param name="filePath">配置文件路径</param>
        public void SaveToFile(string filePath)
        {
            try
            {
                // 确保目录存在
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 如果文件存在且为只读，尝试移除只读属性
                if (File.Exists(filePath))
                {
                    FileAttributes attributes = File.GetAttributes(filePath);
                    if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(filePath, attributes & ~FileAttributes.ReadOnly);
                    }
                }

                using (var writer = new StreamWriter(filePath))
                {
                    // 保存核心按钮设置
                    writer.WriteLine("[CoreButtons]");
                    foreach (var kvp in CoreButtons)
                    {
                        writer.WriteLine($"{kvp.Key}={kvp.Value}");
                    }

                    // 保存顶点显示按钮设置
                    writer.WriteLine("[VertexDisplayButtons]");
                    foreach (var kvp in VertexDisplayButtons)
                    {
                        writer.WriteLine($"{kvp.Key}={kvp.Value}");
                    }

                    // 保存插件按钮设置
                    writer.WriteLine("[PluginButtons]");
                    foreach (var kvp in PluginButtons)
                    {
                        writer.WriteLine($"{kvp.Key}={kvp.Value}");
                    }

                    // 保存其他设置
                    writer.WriteLine("[Options]");
                    writer.WriteLine($"CompactMode={CompactMode}");
                    writer.WriteLine($"ShowButtonText={ShowButtonText}");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new Exception($"无法访问配置文件（可能是权限问题）: {ex.Message}");
            }
            catch (IOException ex)
            {
                throw new Exception($"保存配置文件时发生IO错误: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"保存配置文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 从配置文件加载设置
        /// </summary>
        /// <param name="filePath">配置文件路径</param>
        /// <returns>加载的设置对象</returns>
        public static ToolbarSettings LoadFromFile(string filePath)
        {
            var settings = new ToolbarSettings();
            
            if (!File.Exists(filePath))
                return settings;

            try
            {
                string currentSection = "";
                foreach (string line in File.ReadAllLines(filePath))
                {
                    string trimmedLine = line.Trim();
                    
                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                        continue;

                    if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                    {
                        currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                        continue;
                    }

                    int equalIndex = trimmedLine.IndexOf('=');
                    if (equalIndex > 0)
                    {
                        string key = trimmedLine.Substring(0, equalIndex);
                        string value = trimmedLine.Substring(equalIndex + 1);

                        switch (currentSection)
                        {
                            case "CoreButtons":
                                if (bool.TryParse(value, out bool coreValue))
                                    settings.CoreButtons[key] = coreValue;
                                break;
                            case "VertexDisplayButtons":
                                if (bool.TryParse(value, out bool vertexValue))
                                    settings.VertexDisplayButtons[key] = vertexValue;
                                break;
                            case "PluginButtons":
                                if (bool.TryParse(value, out bool pluginValue))
                                    settings.PluginButtons[key] = pluginValue;
                                break;
                            case "Options":
                                if (key == "CompactMode" && bool.TryParse(value, out bool compactMode))
                                    settings.CompactMode = compactMode;
                                else if (key == "ShowButtonText" && bool.TryParse(value, out bool showText))
                                    settings.ShowButtonText = showText;
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"加载配置文件失败: {ex.Message}");
            }

            return settings;
        }
    }
} 