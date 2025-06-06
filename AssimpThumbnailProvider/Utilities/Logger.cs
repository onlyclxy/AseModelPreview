using System;
using System.IO;

namespace AssimpThumbnailProvider.Utilities
{
    /// <summary>
    /// 统一的日志管理器，用于控制所有模块的日志记录
    /// </summary>
    public static class Logger
    {
        // 全局日志开关 - 修改这里可以控制所有模块的日志
        public const bool ENABLE_FILE_LOGGING = false;  // 暂时启用用于测试
        
        // 日志文件路径
        private static readonly string LogFilePath = Path.Combine(
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
            "assimp_thumbnail.log"
        );

        /// <summary>
        /// 写入日志到文件
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="moduleName">模块名称（可选，用于标识日志来源）</param>
        public static void LogToFile(string message, string moduleName = null)
        {
            if (!ENABLE_FILE_LOGGING) return;

            try
            {
                string prefix = string.IsNullOrEmpty(moduleName) ? "" : $"[{moduleName}] ";
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {prefix}{message}";
                File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
            }
            catch
            {
                // 忽略日志写入错误
            }
        }

        /// <summary>
        /// 清理日志文件（可选，在需要时调用）
        /// </summary>
        public static void ClearLog()
        {
            try
            {
                if (File.Exists(LogFilePath))
                {
                    File.Delete(LogFilePath);
                }
            }
            catch
            {
                // 忽略清理错误
            }
        }
    }
} 