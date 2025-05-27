using System;
using System.Drawing;
using System.Runtime.InteropServices;
using SharpShell.Attributes;
using SharpShell.SharpThumbnailHandler;
using AssimpThumbnailProvider.Utilities;
using System.IO;
using System.Diagnostics;

namespace AssimpThumbnailProvider.Thumbnail
{
    [ComVisible(true)]
    [Guid("B4FB3A0A-3B0D-4B58-B89C-3E4E83E76A08")]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".ase")]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".fbx")]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".obj")]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".gltf")]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".glb")]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".dae")]
    [DisplayName("Assimp 3D Thumbnail Provider")]
    [ProgId("AssimpThumbnailProvider.Thumbnail")]
    public class AssimpThumbnailProvider : SharpThumbnailHandler
    {
        protected override Bitmap GetThumbnailImage(uint width)
        {
            try
            {
                Debug.WriteLine($"开始处理缩略图请求: {SelectedItemStream?.Name}, 大小: {width}x{width}");

                if (SelectedItemStream == null)
                {
                    throw new Exception("没有选中的文件流");
                }

                // 获取文件扩展名
                string extension = Path.GetExtension(SelectedItemStream.Name)?.ToLower() ?? "";
                Debug.WriteLine($"文件扩展名: {extension}");

                // 创建临时文件
                string tempFile = Path.Combine(Path.GetTempPath(), $"model_preview_{Guid.NewGuid()}{extension}");
                Debug.WriteLine($"临时文件路径: {tempFile}");

                try
                {
                    // 将流内容复制到临时文件
                    using (var fileStream = File.Create(tempFile))
                    {
                        byte[] buffer = new byte[8192];
                        int bytesRead;
                        long totalBytes = 0;

                        // 确保流位置在开始
                        SelectedItemStream.Seek(0, SeekOrigin.Begin);

                        while ((bytesRead = SelectedItemStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            fileStream.Write(buffer, 0, bytesRead);
                            totalBytes += bytesRead;
                        }
                        
                        fileStream.Flush();
                        Debug.WriteLine($"已复制 {totalBytes} 字节到临时文件");
                    }

                    // 验证临时文件
                    if (!File.Exists(tempFile))
                    {
                        throw new Exception("临时文件创建失败");
                    }

                    var fileInfo = new FileInfo(tempFile);
                    if (fileInfo.Length == 0)
                    {
                        throw new Exception("临时文件是空的");
                    }

                    Debug.WriteLine($"临时文件大小: {fileInfo.Length} 字节");

                    // 加载模型数据
                    var loader = new AssimpLoader();
                    var meshes = loader.Load(tempFile);

                    if (meshes == null || meshes.Count == 0)
                    {
                        throw new Exception("没有加载到任何网格数据");
                    }

                    Debug.WriteLine($"成功加载了 {meshes.Count} 个网格");

                    // 渲染到 Bitmap
                    return RenderHelper.RenderMeshesToBitmap(meshes, (int)width);
                }
                finally
                {
                    // 清理临时文件
                    try
                    {
                        if (File.Exists(tempFile))
                        {
                            File.Delete(tempFile);
                            Debug.WriteLine("临时文件已删除");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"删除临时文件时出错: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"生成缩略图时出错: {ex.Message}");
                Debug.WriteLine($"错误堆栈: {ex.StackTrace}");

                // 显示错误信息的缩略图
                Bitmap bmp = new Bitmap((int)width, (int)width);
                Graphics g = Graphics.FromImage(bmp);
                try
                {
                    g.Clear(Color.LightGray);
                    using (Font font = new Font("Arial", 8))
                    {
                        string errorMessage = $"Error loading {Path.GetFileName(SelectedItemStream?.Name ?? "unknown")}: {ex.Message}";
                        g.DrawString(errorMessage, font, Brushes.Red, new RectangleF(5, 5, bmp.Width - 10, bmp.Height - 10));
                    }
                }
                finally
                {
                    g.Dispose();
                }
                return bmp;
            }
        }
    }
} 