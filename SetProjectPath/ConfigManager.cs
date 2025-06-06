using System;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Forms;

namespace SetProjectPath
{
    public static class ConfigManager
    {
        private static readonly string ConfigFileName = "ProjectConfig.ini";
        private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);

        /// <summary>
        /// 检查并设置工程目录，如果配置文件不存在或路径无效，则弹窗让用户选择
        /// </summary>
        /// <returns>工程目录路径，失败返回null</returns>
        public static string CheckAndSetProjectDirectory()
        {
            Console.WriteLine("工程路径配置工具");
            Console.WriteLine("=================");
            
            string projectDir = ReadProjectDirectory();
            
            // 如果配置文件存在且目录有效，无事发生
            if (!string.IsNullOrEmpty(projectDir) && Directory.Exists(projectDir))
            {
                Console.WriteLine($"工程目录已配置: {projectDir}");
                Console.WriteLine("目录存在，配置有效。");
                return projectDir;
            }
            
            // 如果配置文件不存在或目录无效，让用户重新选择
            if (string.IsNullOrEmpty(projectDir))
            {
                Console.WriteLine("未找到配置文件。");
            }
            else
            {
                Console.WriteLine($"配置的目录不存在: {projectDir}");
            }
            
            Console.WriteLine("正在打开文件夹选择对话框...");
            
            projectDir = SelectProjectDirectory();
            if (!string.IsNullOrEmpty(projectDir))
            {
                SaveProjectDirectory(projectDir);
                Console.WriteLine($"工程目录已配置并保存: {projectDir}");
            }
            else
            {
                Console.WriteLine("用户取消了目录选择，配置失败。");
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
                    Console.WriteLine($"从配置文件读取: {content}");
                    return content;
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
                Console.WriteLine($"配置已保存到: {ConfigFilePath}");
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
            // 先弹出置顶提示对话框
            DialogResult result = MessageBox.Show(
                "请选择包含 resources 目录的工程路径\n\n点击\"确定\"继续选择目录",
                "选择工程目录", 
                MessageBoxButtons.OKCancel, 
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.DefaultDesktopOnly); // 置顶显示

            if (result != DialogResult.OK)
            {
                return null; // 用户取消了提示对话框
            }

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
                                MessageBoxButtons.OK, MessageBoxIcon.Warning,
                                MessageBoxDefaultButton.Button1,
                                MessageBoxOptions.DefaultDesktopOnly); // 置顶显示
                            return SelectProjectDirectory(); // 递归重新选择
                        }
                    }
                }
            }
            
            return null;
        }

        /// <summary>
        /// 强制重新配置工程目录（即使当前配置有效）
        /// </summary>
        /// <returns>新配置的工程目录路径</returns>
        public static string ForceReconfigureProjectDirectory()
        {
            Console.WriteLine("强制重新配置工程目录...");
            string projectDir = SelectProjectDirectory();
            if (!string.IsNullOrEmpty(projectDir))
            {
                SaveProjectDirectory(projectDir);
                Console.WriteLine($"工程目录已重新配置: {projectDir}");
            }
            return projectDir;
        }
    }
} 