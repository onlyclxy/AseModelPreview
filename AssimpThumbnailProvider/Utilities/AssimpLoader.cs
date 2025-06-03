using Assimp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace AssimpThumbnailProvider.Utilities
{
    public class AssimpLoader
    {
        private readonly AssimpContext _context = new AssimpContext();
        
        // 使用与RenderHelper相同的日志开关
        private const bool ENABLE_FILE_LOGGING = true;
        private static readonly string LogFilePath = Path.Combine(
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
            "thumbnail_loader.log"
        );

        private static void LogToFile(string message)
        {
            if (!ENABLE_FILE_LOGGING) return;

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

        static AssimpLoader()
        {
            try
            {
                // 获取当前程序集所在目录
                string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string assemblyDir = Path.GetDirectoryName(assemblyLocation);
                LogToFile($"程序集位置: {assemblyLocation}");
                LogToFile($"程序集目录: {assemblyDir}");

                // 检查是否是64位进程
                bool is64BitProcess = Environment.Is64BitProcess;
                LogToFile($"是否64位进程: {is64BitProcess}");

                // 获取基础目录（往上一级找到包含runtimes的目录）
                string baseDir = assemblyDir;
                string runtimesPath = null;
                while (!string.IsNullOrEmpty(baseDir))
                {
                    string testPath = Path.Combine(baseDir, "runtimes");
                    LogToFile($"检查runtimes目录: {testPath}");
                    if (Directory.Exists(testPath))
                    {
                        runtimesPath = testPath;
                        LogToFile($"找到runtimes目录: {runtimesPath}");
                        break;
                    }
                    baseDir = Path.GetDirectoryName(baseDir);
                }

                // 构建可能的DLL路径
                var possiblePaths = new List<string>();

                // 如果找到了runtimes目录，将其作为首选路径
                if (!string.IsNullOrEmpty(runtimesPath))
                {
                    string runtimeDllPath = Path.Combine(runtimesPath, 
                        is64BitProcess ? "win-x64" : "win-x86", 
                        "native", 
                        "assimp.dll");
                    possiblePaths.Add(runtimeDllPath);
                }

                // 添加其他可能的路径
                possiblePaths.AddRange(new[]
                {
                    // 程序集目录
                    Path.Combine(assemblyDir, "assimp.dll"),
                    // x86/x64子目录
                    Path.Combine(assemblyDir, is64BitProcess ? "x64" : "x86", "assimp.dll"),
                    // 当前目录
                    "assimp.dll"
                });

                bool dllLoaded = false;
                foreach (var path in possiblePaths)
                {
                    LogToFile($"检查DLL路径: {path}");
                    bool exists = File.Exists(path);
                    LogToFile($"文件存在: {exists}");

                    if (exists)
                    {
                        try
                        {
                            LogToFile($"尝试加载DLL: {path}");
                            IntPtr handle = LoadLibrary(path);
                            if (handle != IntPtr.Zero)
                            {
                                LogToFile($"DLL加载成功: {path}");
                                dllLoaded = true;
                                break;
                            }
                            else
                            {
                                int error = Marshal.GetLastWin32Error();
                                LogToFile($"DLL加载失败，错误代码: {error}");
                            }
                        }
                        catch (Exception ex)
                        {
                            LogToFile($"加载DLL时出错: {ex.Message}");
                        }
                    }
                }

                if (!dllLoaded)
                {
                    LogToFile("警告: 未能成功加载assimp.dll");
                }
            }
            catch (Exception ex)
            {
                LogToFile($"初始化时出错: {ex.Message}");
                LogToFile($"错误堆栈: {ex.StackTrace}");
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        public List<MeshData> Load(string filePath)
        {
            LogToFile($"开始加载文件: {filePath}");
            
            try
            {
                var fileInfo = new FileInfo(filePath);
                LogToFile($"文件大小: {fileInfo.Length / 1024} KB");
                LogToFile($"文件最后修改时间: {fileInfo.LastWriteTime}");
                
                // 根据文件扩展名设置特定的导入选项
                var extension = Path.GetExtension(filePath)?.ToLower();
                var importFlags = GetImportFlagsForFormat(extension);
                
                LogToFile($"文件扩展名: {extension}");
                LogToFile($"使用导入标志: {importFlags}");

                var scene = _context.ImportFile(filePath, importFlags);
                
                if (scene == null)
                {
                    LogToFile("场景加载失败，返回默认立方体");
                    return new List<MeshData> { CreateDefaultCube() };
                }

                LogToFile($"场景加载成功:");
                LogToFile($"- 网格数量: {scene.MeshCount}");
                LogToFile($"- 材质数量: {scene.MaterialCount}");
                LogToFile($"- 动画数量: {scene.AnimationCount}");
                LogToFile($"- 纹理数量: {scene.TextureCount}");
                
                if (scene.MeshCount == 0)
                {
                    LogToFile("警告: 场景中没有网格，返回默认立方体");
                    return new List<MeshData> { CreateDefaultCube() };
                }

                var meshes = new List<MeshData>();
                foreach (var mesh in scene.Meshes)
                {
                    LogToFile($"处理网格: {mesh.Name}");
                    LogToFile($"- 顶点数量: {mesh.VertexCount}");
                    LogToFile($"- 面数量: {mesh.FaceCount}");
                    LogToFile($"- 是否有法线: {mesh.HasNormals}");
                    LogToFile($"- 是否有纹理坐标: {mesh.HasTextureCoords(0)}");
                    
                    if (mesh.VertexCount == 0 || mesh.FaceCount == 0)
                    {
                        LogToFile("警告: 跳过空网格");
                        continue;
                    }
                    
                    var data = new MeshData(mesh);
                    
                    LogToFile($"转换后的网格数据:");
                    LogToFile($"- 顶点数量: {data.Vertices.Count/3}");
                    LogToFile($"- 法线数量: {data.Normals.Count/3}");
                    LogToFile($"- UV数量: {data.UVs.Count/2}");
                    LogToFile($"- 索引数量: {data.Indices.Count}");
                    
                    // 规范化网格大小
                    NormalizeMeshSize(data);
                    
                    meshes.Add(data);
                }
                
                if (meshes.Count == 0)
                {
                    LogToFile("警告: 所有网格都是空的或无效的，返回默认立方体");
                    return new List<MeshData> { CreateDefaultCube() };
                }
                
                LogToFile($"成功加载了 {meshes.Count} 个有效网格");
                return meshes;
            }
            catch (Exception ex)
            {
                LogToFile($"加载模型时出错: {ex.Message}");
                LogToFile($"错误堆栈: {ex.StackTrace}");
                
                // 返回一个默认几何体便于调试
                return new List<MeshData> { CreateDefaultCube() };
            }
        }

        private PostProcessSteps GetImportFlagsForFormat(string extension)
        {
            var baseFlags = PostProcessSteps.Triangulate |
                PostProcessSteps.GenerateNormals |
                PostProcessSteps.JoinIdenticalVertices |
                PostProcessSteps.ValidateDataStructure |
                PostProcessSteps.OptimizeMeshes |
                PostProcessSteps.ImproveCacheLocality |
                PostProcessSteps.RemoveRedundantMaterials;

            switch (extension)
            {
                case ".ase":
                    return baseFlags | PostProcessSteps.FlipUVs;
                case ".fbx":
                    return baseFlags | PostProcessSteps.FlipUVs | 
                           PostProcessSteps.CalculateTangentSpace |
                           PostProcessSteps.GenerateUVCoords;
                case ".obj":
                    return baseFlags | PostProcessSteps.FlipWindingOrder |
                           PostProcessSteps.GenerateUVCoords;
                default:
                    return baseFlags;
            }
        }
        
        // 创建一个默认立方体几何体
        private MeshData CreateDefaultCube()
        {
            Debug.WriteLine("创建默认立方体几何体");
            
            // 立方体顶点 (-0.5, -0.5, -0.5) 到 (0.5, 0.5, 0.5)
            var vertices = new List<float>
            {
                // 前面 (z = 0.5)
                -0.5f, -0.5f, 0.5f,
                0.5f, -0.5f, 0.5f,
                0.5f, 0.5f, 0.5f,
                -0.5f, 0.5f, 0.5f,
                
                // 后面 (z = -0.5)
                -0.5f, -0.5f, -0.5f,
                -0.5f, 0.5f, -0.5f,
                0.5f, 0.5f, -0.5f,
                0.5f, -0.5f, -0.5f,
                
                // 上面 (y = 0.5)
                -0.5f, 0.5f, -0.5f,
                -0.5f, 0.5f, 0.5f,
                0.5f, 0.5f, 0.5f,
                0.5f, 0.5f, -0.5f,
                
                // 下面 (y = -0.5)
                -0.5f, -0.5f, -0.5f,
                0.5f, -0.5f, -0.5f,
                0.5f, -0.5f, 0.5f,
                -0.5f, -0.5f, 0.5f,
                
                // 右面 (x = 0.5)
                0.5f, -0.5f, -0.5f,
                0.5f, 0.5f, -0.5f,
                0.5f, 0.5f, 0.5f,
                0.5f, -0.5f, 0.5f,
                
                // 左面 (x = -0.5)
                -0.5f, -0.5f, -0.5f,
                -0.5f, -0.5f, 0.5f,
                -0.5f, 0.5f, 0.5f,
                -0.5f, 0.5f, -0.5f
            };
            
            // 为每个顶点设置相同的法线（简化处理）
            var normals = new List<float>();
            for (int i = 0; i < 24; i++) // 24个顶点
            {
                switch (i / 4) // 每个面4个顶点
                {
                    case 0: normals.AddRange(new[] { 0f, 0f, 1f }); break; // 前面
                    case 1: normals.AddRange(new[] { 0f, 0f, -1f }); break; // 后面
                    case 2: normals.AddRange(new[] { 0f, 1f, 0f }); break; // 上面
                    case 3: normals.AddRange(new[] { 0f, -1f, 0f }); break; // 下面
                    case 4: normals.AddRange(new[] { 1f, 0f, 0f }); break; // 右面
                    case 5: normals.AddRange(new[] { -1f, 0f, 0f }); break; // 左面
                }
            }
            
            // 纹理坐标
            var uvs = new List<float>();
            for (int i = 0; i < 6; i++) // 6个面
            {
                uvs.AddRange(new[] { 0f, 0f }); // 左下
                uvs.AddRange(new[] { 1f, 0f }); // 右下
                uvs.AddRange(new[] { 1f, 1f }); // 右上
                uvs.AddRange(new[] { 0f, 1f }); // 左上
            }
            
            // 索引（每个面2个三角形）
            var indices = new List<int>();
            for (int i = 0; i < 6; i++) // 6个面
            {
                int baseIndex = i * 4;
                // 第一个三角形
                indices.Add(baseIndex);
                indices.Add(baseIndex + 1);
                indices.Add(baseIndex + 2);
                // 第二个三角形
                indices.Add(baseIndex);
                indices.Add(baseIndex + 2);
                indices.Add(baseIndex + 3);
            }
            
            return new MeshData(vertices, normals, uvs, indices);
        }
        
        // 规范化网格大小，使其适合视图
        private void NormalizeMeshSize(MeshData mesh)
        {
            if (mesh.Vertices.Count < 3)
                return;
            
            // 找出边界框
            float minX = float.MaxValue, minY = float.MaxValue, minZ = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue, maxZ = float.MinValue;
            
            for (int i = 0; i < mesh.Vertices.Count; i += 3)
            {
                float x = mesh.Vertices[i];
                float y = mesh.Vertices[i + 1];
                float z = mesh.Vertices[i + 2];
                
                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                minZ = Math.Min(minZ, z);
                
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
                maxZ = Math.Max(maxZ, z);
            }
            
            // 计算中心点和大小
            float centerX = (minX + maxX) / 2;
            float centerY = (minY + maxY) / 2;
            float centerZ = (minZ + maxZ) / 2;
            
            float sizeX = maxX - minX;
            float sizeY = maxY - minY;
            float sizeZ = maxZ - minZ;
            
            // 找出最大尺寸
            float maxSize = Math.Max(Math.Max(sizeX, sizeY), sizeZ);
            if (maxSize < 0.001f) // 防止除零
                return;
            
            // 将模型缩放到合适大小，并将中心放在原点
            float scaleFactor = 2.0f / maxSize; // 将最大尺寸缩放到2个单位
            
            for (int i = 0; i < mesh.Vertices.Count; i += 3)
            {
                mesh.Vertices[i] = (mesh.Vertices[i] - centerX) * scaleFactor;
                mesh.Vertices[i + 1] = (mesh.Vertices[i + 1] - centerY) * scaleFactor;
                mesh.Vertices[i + 2] = (mesh.Vertices[i + 2] - centerZ) * scaleFactor;
            }
            
            Debug.WriteLine($"网格已规范化: 中心 ({centerX}, {centerY}, {centerZ}), 大小 ({sizeX}, {sizeY}, {sizeZ}), 缩放因子 {scaleFactor}");
        }
    }

    public class MeshData
    {
        public List<float> Vertices { get; private set; }
        public List<float> Normals { get; private set; }
        public List<float> UVs { get; private set; }
        public List<int> Indices { get; private set; }

        public MeshData(List<float> vertices, List<float> normals, List<float> uvs, List<int> indices)
        {
            Vertices = vertices ?? new List<float>();
            Normals = normals ?? new List<float>();
            UVs = uvs ?? new List<float>();
            Indices = indices ?? new List<int>();
        }

        public MeshData(Mesh mesh)
        {
            try
            {
                // 提取顶点
                Vertices = mesh.Vertices != null && mesh.Vertices.Count > 0
                    ? mesh.Vertices.SelectMany(v => new[] { v.X, v.Y, v.Z }).ToList()
                    : new List<float>();
                
                // 提取法线
                Normals = mesh.Normals != null && mesh.Normals.Count > 0
                    ? mesh.Normals.SelectMany(n => new[] { n.X, n.Y, n.Z }).ToList()
                    : new List<float>();
                
                // 提取纹理坐标
                UVs = mesh.TextureCoordinateChannels != null && 
                      mesh.TextureCoordinateChannels.Length > 0 && 
                      mesh.TextureCoordinateChannels[0] != null
                    ? mesh.TextureCoordinateChannels[0].SelectMany(uv => new[] { uv.X, uv.Y }).ToList()
                    : new List<float>();
                
                // 提取三角形索引
                Indices = mesh.Faces != null && mesh.Faces.Count > 0
                    ? mesh.Faces.SelectMany(f => f.Indices).ToList() 
                    : new List<int>();
                
                // 检查数据完整性
                if (Vertices.Count == 0 || Indices.Count == 0)
                {
                    Debug.WriteLine("警告: 网格数据不完整");
                }
                
                // 确保法线数量与顶点相匹配
                if (Normals.Count == 0 && Vertices.Count > 0)
                {
                    Debug.WriteLine("警告: 缺少法线数据，生成默认法线");
                    // 为每个顶点生成简单的向外法线
                    Normals = new List<float>();
                    for (int i = 0; i < Vertices.Count; i += 3)
                    {
                        Normals.AddRange(new[] { 0f, 1f, 0f }); // 简单的向上法线
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"处理网格数据时出错: {ex.Message}");
                // 防止空数据
                Vertices = new List<float>();
                Normals = new List<float>();
                UVs = new List<float>();
                Indices = new List<int>();
            }
        }
    }
} 