using System;
using System.IO;
using System.Windows.Forms;

namespace AssimpThumbnailProvider.DmlPath
{
    public static class ConfigManager
    {
        private static readonly string ConfigFileName = "ProjectConfig.ini";
        private static readonly string ConfigFilePath = Path.Combine(
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
            ConfigFileName
        );
        
        private static readonly string LogFilePath = Path.Combine(
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
            "config_manager.log"
        );

        private static void LogToFile(string message)
        {
            try
            {
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
                File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
            }
            catch
            {
                // 忽略日志写入错误
            }
        }

        /// <summary>
        /// 获取工程目录，如果配置文件不存在或路径无效，则返回null
        /// </summary>
        /// <returns>工程目录路径，失败返回null</returns>
        public static string GetProjectDirectory()
        {
            LogToFile("开始获取工程目录");
            string projectDir = ReadProjectDirectory();
            
            // 对于缩略图提供程序，我们不弹窗，只返回配置的目录
            if (string.IsNullOrEmpty(projectDir) || !Directory.Exists(projectDir))
            {
                LogToFile("工程目录不存在或无效，返回null");
                return null; // 缩略图提供程序不应该弹窗
            }
            
            LogToFile($"成功获取工程目录: {projectDir}");
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
                LogToFile($"尝试读取配置文件: {ConfigFilePath}");
                if (File.Exists(ConfigFilePath))
                {
                    string content = File.ReadAllText(ConfigFilePath).Trim();
                    if (!string.IsNullOrEmpty(content) && Directory.Exists(content))
                    {
                        LogToFile($"成功读取工程目录: {content}");
                        return content;
                    }
                    else
                    {
                        LogToFile($"配置文件内容无效或目录不存在: {content}");
                    }
                }
                else
                {
                    LogToFile("配置文件不存在");
                }
            }
            catch (Exception ex)
            {
                LogToFile($"读取配置文件失败: {ex.Message}");
            }
            
            return null;
        }

        /// <summary>
        /// 保存工程目录到配置文件
        /// </summary>
        /// <param name="projectDir">工程目录路径</param>
        public static void SaveProjectDirectory(string projectDir)
        {
            try
            {
                LogToFile($"尝试保存工程目录: {projectDir}");
                File.WriteAllText(ConfigFilePath, projectDir);
                LogToFile($"工程目录已保存: {projectDir}");
            }
            catch (Exception ex)
            {
                LogToFile($"保存配置文件失败: {ex.Message}");
            }
        }
    }
} 