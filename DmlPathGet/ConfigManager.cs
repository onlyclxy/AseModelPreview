using System;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Forms;

namespace DmlPathGet
{
    public static class ConfigManager
    {
        private static readonly string ConfigFileName = "ProjectConfig.ini";
        private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);

        /// <summary>
        /// 获取工程目录，如果配置文件不存在或路径无效，则弹窗让用户选择
        /// </summary>
        /// <returns>工程目录路径，失败返回null</returns>
        public static string GetProjectDirectory()
        {
            string projectDir = ReadProjectDirectory();
            
            // 如果配置文件不存在或目录无效，让用户重新选择
            if (string.IsNullOrEmpty(projectDir) || !Directory.Exists(projectDir))
            {
                projectDir = SelectProjectDirectory();
                if (!string.IsNullOrEmpty(projectDir))
                {
                    SaveProjectDirectory(projectDir);
                }
            }
            
            return projectDir;
        }

        /// <summary>
        /// 从配置文件读取工程目录
        /// </summary>
        /// <returns>工程目录路径</returns>
        private static string ReadProjectDirectory()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string content = File.ReadAllText(ConfigFilePath).Trim();
                    if (!string.IsNullOrEmpty(content) && Directory.Exists(content))
                    {
                        return content;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取配置文件失败: {ex.Message}");
            }
            
            return null;
        }

        /// <summary>
        /// 保存工程目录到配置文件
        /// </summary>
        /// <param name="projectDir">工程目录路径</param>
        private static void SaveProjectDirectory(string projectDir)
        {
            try
            {
                File.WriteAllText(ConfigFilePath, projectDir);
                Console.WriteLine($"工程目录已保存: {projectDir}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存配置文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 弹窗让用户选择工程目录
        /// </summary>
        /// <returns>用户选择的工程目录，取消返回null</returns>
        private static string SelectProjectDirectory()
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;
                dialog.Title = "选择工程目录（通常选择到resources目录）";
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                dialog.EnsurePathExists = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string selectedPath = dialog.FileName;
                    
                    // 如果选择的是resources目录，返回其上一层目录
                    if (selectedPath.EndsWith("resources", StringComparison.OrdinalIgnoreCase))
                    {
                        return Path.GetDirectoryName(selectedPath);
                    }
                    else
                    {
                        // 如果不是resources目录，检查是否包含resources子目录
                        string resourcesPath = Path.Combine(selectedPath, "resources");
                        if (Directory.Exists(resourcesPath))
                        {
                            return selectedPath;
                        }
                        else
                        {
                            MessageBox.Show("选择的目录中没有找到resources子目录，请重新选择。", "警告", 
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return SelectProjectDirectory(); // 递归重新选择
                        }
                    }
                }
            }
            
            return null;
        }
    }
} 