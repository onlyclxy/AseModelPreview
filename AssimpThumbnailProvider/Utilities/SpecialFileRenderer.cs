using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Media.Media3D;
using System.Diagnostics;
using System.Collections.Generic;
using AssimpThumbnailProvider.DmlPath;

namespace AssimpThumbnailProvider.Utilities
{
    public static class SpecialFileRenderer
    {
        // 是否启用特殊文件渲染
        public static bool EnableSpecialFileRendering = true;

        // 支持的特殊文件类型及其对应的模型文件扩展名
        private static readonly (string specialExt, string modelExt)[] SpecialFileTypes = new[]
        {
            (".dml", ".ase"),
            (".chr", ".ase")
        };

        public static bool IsSpecialFile(string filePath)
        {
            if (!EnableSpecialFileRendering)
            {
                Logger.LogToFile("特殊文件渲染功能已禁用", "SpecialFileRenderer");
                return false;
            }
            
            string ext = Path.GetExtension(filePath)?.ToLower();
            Logger.LogToFile($"检查文件是否为特殊类型: {filePath}, 扩展名: {ext}", "SpecialFileRenderer");
            
            bool isSpecial = Array.Exists(SpecialFileTypes, pair => pair.specialExt == ext);
            Logger.LogToFile($"是否为特殊文件: {isSpecial}", "SpecialFileRenderer");
            return isSpecial;
        }

        public static Bitmap RenderSpecialFile(string filePath, uint width)
        {
            try
            {
                if (!EnableSpecialFileRendering)
                {
                    Logger.LogToFile("特殊文件渲染功能已禁用", "SpecialFileRenderer");
                    return null;
                }

                string ext = Path.GetExtension(filePath)?.ToLower();
                if (string.IsNullOrEmpty(ext))
                {
                    Logger.LogToFile("文件没有扩展名", "SpecialFileRenderer");
                    return null;
                }

                Logger.LogToFile($"开始处理特殊文件: {filePath}", "SpecialFileRenderer");
                var fileType = Array.Find(SpecialFileTypes, pair => pair.specialExt == ext);
                if (fileType == default)
                {
                    Logger.LogToFile($"未找到匹配的特殊文件类型: {ext}", "SpecialFileRenderer");
                    return null;
                }

                string modelFile = null;

                // 根据文件类型使用不同的方法获取模型文件路径
                if (ext == ".dml" || ext == ".chr")
                {
                    Logger.LogToFile($"使用DmlPathGet解析特殊文件: {filePath}", "SpecialFileRenderer");
                    
                    // 使用DmlPathGet解析文件
                    List<string> modelPaths = DmlParser.ParseDmlFile(filePath);
                    
                    if (modelPaths != null && modelPaths.Count > 0)
                    {
                        // 取第一个找到的模型文件路径
                        modelFile = modelPaths[0];
                        Logger.LogToFile($"通过DmlPathGet找到模型文件: {modelFile}", "SpecialFileRenderer");
                    }
                    else
                    {
                        Logger.LogToFile("DmlPathGet未找到任何模型文件路径", "SpecialFileRenderer");
                        
                        // 如果DmlPathGet失败，则回退到原来的简单查找方法
                        string baseFileName = Path.GetFileNameWithoutExtension(filePath);
                        string directory = Path.GetDirectoryName(filePath);
                        modelFile = Path.Combine(directory, baseFileName + fileType.modelExt);
                        Logger.LogToFile($"回退到简单查找: {modelFile}", "SpecialFileRenderer");
                    }
                }
                else
                {
                    // 其他类型文件使用原来的简单查找方法
                    string baseFileName = Path.GetFileNameWithoutExtension(filePath);
                    string directory = Path.GetDirectoryName(filePath);
                    modelFile = Path.Combine(directory, baseFileName + fileType.modelExt);
                    Logger.LogToFile($"使用默认查找方法: {modelFile}", "SpecialFileRenderer");
                }

                if (string.IsNullOrEmpty(modelFile) || !File.Exists(modelFile))
                {
                    Logger.LogToFile($"对应的模型文件不存在: {modelFile}", "SpecialFileRenderer");
                    return null;
                }

                Logger.LogToFile($"找到对应的模型文件，开始加载: {modelFile}", "SpecialFileRenderer");
                // 加载并渲染模型
                var loader = new AssimpLoader();
                var meshes = loader.Load(modelFile);
                if (meshes == null || meshes.Count == 0)
                {
                    Logger.LogToFile("模型加载失败或没有网格数据", "SpecialFileRenderer");
                    return null;
                }
                Logger.LogToFile($"成功加载模型，网格数量: {meshes.Count}", "SpecialFileRenderer");

                var bitmap = RenderHelper.RenderMeshesToBitmap(meshes, (int)width);
                if (bitmap == null)
                {
                    Logger.LogToFile("渲染位图失败", "SpecialFileRenderer");
                    return null;
                }

                Logger.LogToFile("开始添加文件类型标记", "SpecialFileRenderer");
                // 添加文件类型标记
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    // 移除扩展名前的点号
                    string label = ext.TrimStart('.');
                    Logger.LogToFile($"添加标记: {label}", "SpecialFileRenderer");
                    
                    // 设置文字样式
                    using (var font = new Font("Arial", width / 10, FontStyle.Bold))
                    using (var path = new GraphicsPath())
                    {
                        // 添加文字到路径
                        path.AddString(
                            label.ToUpper(),
                            font.FontFamily,
                            (int)font.Style,
                            font.Size,
                            new Point(5, 5),
                            StringFormat.GenericDefault
                        );

                        // 绘制文字轮廓
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.DrawPath(new Pen(Color.Black, width / 50), path);
                        g.FillPath(Brushes.White, path);
                    }
                }

                Logger.LogToFile("特殊文件渲染完成", "SpecialFileRenderer");
                return bitmap;
            }
            catch (Exception ex)
            {
                Logger.LogToFile($"渲染特殊文件时出错: {ex.Message}", "SpecialFileRenderer");
                Logger.LogToFile($"错误堆栈: {ex.StackTrace}", "SpecialFileRenderer");
                return null;
            }
        }
    }
} 