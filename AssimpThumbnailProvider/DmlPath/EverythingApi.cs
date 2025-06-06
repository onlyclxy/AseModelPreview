using System;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using AssimpThumbnailProvider.Utilities;

namespace AssimpThumbnailProvider.DmlPath
{
    public static class EverythingApi
    {
        private const string EVERYTHING_DLL = "Everything64.dll";
        
        [DllImport(EVERYTHING_DLL, CharSet = CharSet.Unicode)]
        public static extern int Everything_SetSearchW(string lpSearchString);
        
        [DllImport(EVERYTHING_DLL)]
        public static extern void Everything_SetMatchCase(bool bEnable);
        
        [DllImport(EVERYTHING_DLL)]
        public static extern void Everything_SetMatchWholeWord(bool bEnable);
        
        [DllImport(EVERYTHING_DLL)]
        public static extern void Everything_SetMatchPath(bool bEnable);
        
        [DllImport(EVERYTHING_DLL)]
        public static extern void Everything_SetRegex(bool bEnable);
        
        [DllImport(EVERYTHING_DLL)]
        public static extern void Everything_SetMax(int dwMax);
        
        [DllImport(EVERYTHING_DLL)]
        public static extern bool Everything_QueryW(bool bWait);
        
        [DllImport(EVERYTHING_DLL)]
        public static extern int Everything_GetNumResults();
        
        [DllImport(EVERYTHING_DLL, CharSet = CharSet.Unicode)]
        public static extern void Everything_GetResultFullPathNameW(int nIndex, StringBuilder lpString, int nMaxCount);
        
        [DllImport(EVERYTHING_DLL)]
        public static extern void Everything_Reset();
        
        /// <summary>
        /// 搜索文件
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <param name="searchPath">搜索路径，为空则搜索全盘</param>
        /// <returns>找到的第一个文件的完整路径，如果没找到返回null</returns>
        public static string SearchFile(string filename, string searchPath)
        {
            try
            {
                Logger.LogToFile($"开始搜索文件: {filename}, 搜索路径: {searchPath ?? "全盘"}", "EverythingApi");
                
                Everything_Reset();
                
                // 设置搜索条件
                string searchString;
                if (string.IsNullOrEmpty(searchPath))
                {
                    // 全盘搜索，只搜索文件名
                    searchString = $"\"{filename}\"";
                }
                else
                {
                    // 在指定路径下搜索
                    searchString = $"path:\"{searchPath}\" \"{filename}\"";
                }

                Logger.LogToFile($"Everything搜索字符串: {searchString}", "EverythingApi");

                Everything_SetSearchW(searchString);
                Everything_SetMatchCase(false);
                Everything_SetMatchWholeWord(false);
                Everything_SetMatchPath(true);
                Everything_SetRegex(false);
                Everything_SetMax(1); // 只取第一个结果
                
                // 执行搜索
                if (Everything_QueryW(true))
                {
                    int numResults = Everything_GetNumResults();
                    Logger.LogToFile($"Everything搜索完成，找到 {numResults} 个结果", "EverythingApi");
                    
                    if (numResults > 0)
                    {
                        StringBuilder sb = new StringBuilder(260);
                        Everything_GetResultFullPathNameW(0, sb, 260);
                        string result = sb.ToString();
                        Logger.LogToFile($"Everything搜索成功，返回结果: {result}", "EverythingApi");
                        return result;
                    }
                    else
                    {
                        Logger.LogToFile("Everything搜索未找到匹配文件", "EverythingApi");
                    }
                }
                else
                {
                    Logger.LogToFile("Everything查询失败", "EverythingApi");
                }
            }
            catch (Exception ex)
            {
                Logger.LogToFile($"Everything搜索失败: {ex.Message}", "EverythingApi");
            }
            
            Logger.LogToFile("搜索失败，返回null", "EverythingApi");
            return null;
        }
    }
} 