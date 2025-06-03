using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace DmlPathGet
{
    public class DmlParser
    {
        /// <summary>
        /// 解析DML文件并获取指定节下的第一行内容
        /// </summary>
        /// <param name="dmlFilePath">DML文件路径</param>
        /// <returns>文件路径列表</returns>
        public static List<string> ParseDmlFile(string dmlFilePath)
        {
            List<string> paths = new List<string>();
            
            if (!File.Exists(dmlFilePath))
            {
                Console.WriteLine($"DML文件不存在: {dmlFilePath}");
                return paths;
            }

            try
            {
                string[] lines = File.ReadAllLines(dmlFilePath);
                bool inTargetSection = false;
                bool foundFirstLine = false;
                string currentSection = "";

                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine)) continue;
                    
                    // 检查是否是节标题
                    if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                    {
                        currentSection = trimmedLine;
                        inTargetSection = currentSection.Equals("[GEOMETRY MESH]", StringComparison.OrdinalIgnoreCase) ||
                                        currentSection.Equals("[SKIN]", StringComparison.OrdinalIgnoreCase);
                        foundFirstLine = false; // 重置标记
                        continue;
                    }

                    // 如果在目标节中且还没找到第一行内容
                    if (inTargetSection && !foundFirstLine)
                    {
                        // 确保这一行不是节标题（不包含中括号）
                        if (!trimmedLine.Contains("[") && !trimmedLine.Contains("]"))
                        {
                            string resolvedPath = ResolveFilePath(trimmedLine);
                            if (!string.IsNullOrEmpty(resolvedPath))
                            {
                                paths.Add(resolvedPath);
                                Console.WriteLine($"找到文件路径: {resolvedPath}");
                            }
                            foundFirstLine = true; // 标记已找到第一行
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"解析DML文件失败: {ex.Message}");
            }

            return paths;
        }

        /// <summary>
        /// 解析文件路径，将相对路径转换为绝对路径
        /// </summary>
        /// <param name="filePath">文件路径（可能是相对路径）</param>
        /// <returns>绝对路径，失败返回空字符串</returns>
        private static string ResolveFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return "";

            string fileName = Path.GetFileName(filePath);
            string projectDir = ConfigManager.GetProjectDirectory();

            // 1. 如果是相对路径，先尝试基于工程目录解析
            if (filePath.Contains("..") || filePath.StartsWith("."))
            {
                if (!string.IsNullOrEmpty(projectDir))
                {
                    string fullPath = TryResolveRelativePath(filePath, projectDir);
                    if (!string.IsNullOrEmpty(fullPath))
                        return fullPath;
                }

                // 如果基于工程目录解析失败，尝试搜索
                return SearchFile(fileName);
            }
            // 2. 如果是单个文件名（孤立文件）
            else if (!string.IsNullOrEmpty(fileName) && fileName.Equals(filePath))
            {
                return SearchFile(fileName);
            }
            // 3. 如果是绝对路径
            else if (File.Exists(filePath))
            {
                return filePath;
            }
            // 4. 其他情况，尝试搜索
            else
            {
                return SearchFile(fileName);
            }
        }

        /// <summary>
        /// 尝试解析相对路径
        /// </summary>
        private static string TryResolveRelativePath(string relativePath, string basePath)
        {
            try
            {
                // 移除开头的..\或./
                string cleanPath = relativePath;
                while (cleanPath.StartsWith("..\\") || cleanPath.StartsWith("../"))
                {
                    cleanPath = cleanPath.Substring(3);
                }
                while (cleanPath.StartsWith(".\\") || cleanPath.StartsWith("./"))
                {
                    cleanPath = cleanPath.Substring(2);
                }

                // 拼接工程目录
                string fullPath = Path.Combine(basePath, cleanPath);
                fullPath = Path.GetFullPath(fullPath);

                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"路径解析失败: {ex.Message}");
            }
            return "";
        }

        /// <summary>
        /// 使用Everything搜索文件
        /// </summary>
        private static string SearchFile(string fileName)
        {
            string projectDir = ConfigManager.GetProjectDirectory();
            string foundPath = "";

            // 1. 如果有工程目录，先在工程目录中搜索
            if (!string.IsNullOrEmpty(projectDir))
            {
                Console.WriteLine($"在工程目录中搜索: {fileName}");
                foundPath = EverythingApi.SearchFile(fileName, projectDir);
                if (!string.IsNullOrEmpty(foundPath))
                {
                    Console.WriteLine($"在工程目录中找到文件: {foundPath}");
                    return foundPath;
                }
            }

            // 2. 在所有磁盘中搜索
            Console.WriteLine($"在全盘搜索: {fileName}");
            foundPath = EverythingApi.SearchFile(fileName, "");
            if (!string.IsNullOrEmpty(foundPath))
            {
                Console.WriteLine($"在全盘中找到文件: {foundPath}");
                return foundPath;
            }

            Console.WriteLine($"未找到文件: {fileName}");
            return "";
        }
    }
}