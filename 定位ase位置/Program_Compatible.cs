using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace 定位ase位置
{
    internal class Program
    {
        // 用于刷新资源管理器的Win32 API
        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
        
        private const uint SHCNE_ASSOCCHANGED = 0x08000000;
        private const uint SHCNF_IDLIST = 0x0000;

        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                // 检查是否通过命令行参数传入了文件路径（右键菜单调用）
                if (args.Length >= 2)
                {
                    string operation = args[0].ToLower(); // 操作类型：locate 或 open
                    string inputFile = args[1];
                    
                    if (File.Exists(inputFile))
                    {
                        string extension = Path.GetExtension(inputFile).ToLower();
                        
                        // 检查是否为支持的文件类型
                        if (extension == ".dml" || extension == ".mat")
                        {
                            ProcessFile(inputFile, operation);
                        }
                        else
                        {
                            ShowUnsupportedFileTypeMessage();
                        }
                    }
                    else
                    {
                        MessageBox.Show("文件不存在: " + inputFile, "错误", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else if (args.Length == 1 && File.Exists(args[0]))
                {
                    // 兼容旧版本：单个文件参数，默认执行定位和打开
                    string inputFile = args[0];
                    string extension = Path.GetExtension(inputFile).ToLower();
                    
                    if (extension == ".dml" || extension == ".mat")
                    {
                        ProcessFile(inputFile, "both");
                    }
                    else
                    {
                        ShowUnsupportedFileTypeMessage();
                    }
                }
                else
                {
                    // 没有文件参数，显示注册菜单的选项
                    ShowRegistrationDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("发生错误: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 处理 dml 或 mat 文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="operation">操作类型："locate" 或 "open" 或 "both"</param>
        static void ProcessFile(string filePath, string operation)
        {
            try
            {
                // 获取文件所在目录和文件名（不含扩展名）
                string directory = Path.GetDirectoryName(filePath);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
                
                // 查找同名的 ase 文件
                string aseFilePath = Path.Combine(directory, fileNameWithoutExtension + ".ase");
                
                if (File.Exists(aseFilePath))
                {
                    if (operation == "locate" || operation == "both")
                    {
                        // 定位文件（在文件资源管理器中选中）
                        SelectFileInExplorer(aseFilePath);
                    }
                    if (operation == "open" || operation == "both")
                    {
                        // 用 ModelPreview.exe 打开 ase 文件
                        OpenWithModelPreview(aseFilePath);
                    }
                }
                else
                {
                    MessageBox.Show("未找到对应的 ASE 文件: " + aseFilePath, "文件未找到", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("处理文件时发生错误: " + ex.Message, "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 使用 ModelPreview.exe 打开 ase 文件
        /// </summary>
        /// <param name="aseFilePath">ase 文件路径</param>
        static void OpenWithModelPreview(string aseFilePath)
        {
            try
            {
                // 获取程序自身的目录
                string programDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string modelPreviewPath = Path.Combine(programDirectory, "ModelPreview.exe");
                
                // 检查 ModelPreview.exe 是否存在
                if (File.Exists(modelPreviewPath))
                {
                    // 启动 ModelPreview.exe 打开 ase 文件
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = modelPreviewPath,
                        Arguments = "\"" + aseFilePath + "\"",
                        UseShellExecute = true
                    };
                    
                    Process.Start(startInfo);
                }
                else
                {
                    MessageBox.Show("未找到 ModelPreview.exe: " + modelPreviewPath, "程序未找到", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("启动 ModelPreview.exe 时发生错误: " + ex.Message, "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 在文件资源管理器中选中文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        static void SelectFileInExplorer(string filePath)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = "/select,\"" + filePath + "\"",
                    UseShellExecute = true
                };
                
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("定位文件时发生错误: " + ex.Message, "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 显示不支持的文件类型消息
        /// </summary>
        static void ShowUnsupportedFileTypeMessage()
        {
            DialogResult result = MessageBox.Show(
                "此程序仅支持 .dml 和 .mat 文件类型。\n\n是否要将此程序注册到 dml 和 mat 文件的右键菜单中？", 
                "不支持的文件类型", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Information);
                
            if (result == DialogResult.Yes)
            {
                RegisterContextMenu();
            }
        }

        /// <summary>
        /// 显示注册对话框
        /// </summary>
        static void ShowRegistrationDialog()
        {
            DialogResult result = MessageBox.Show(
                "ASE 文件定位器\n\n此程序可以帮助您:\n1. 根据 dml/mat 文件查找同名的 ase 文件\n2. 使用 ModelPreview.exe 打开 ase 文件\n3. 在文件资源管理器中定位 ase 文件\n\n是否要将此程序注册到 dml 和 mat 文件的右键菜单中？", 
                "ASE 文件定位器", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question);
                
            if (result == DialogResult.Yes)
            {
                RegisterContextMenu();
            }
        }

        /// <summary>
        /// 注册右键菜单到注册表
        /// </summary>
        static void RegisterContextMenu()
        {
            try
            {
                string programPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string menuText = "定位 ASE 文件";
                
                MessageBox.Show("开始注册右键菜单...\n程序路径: " + programPath, "注册信息", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // 注册 dml 文件类型 - 使用实际的程序类型
                try
                {
                    RegisterFileTypeContextMenu(".dml", "", menuText, programPath);
                    MessageBox.Show("DML 文件类型注册成功！", "注册进度", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("注册 DML 文件类型失败: " + ex.Message, "注册错误", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                
                // 注册 mat 文件类型 - 使用实际的程序类型
                try
                {
                    RegisterFileTypeContextMenu(".mat", "", menuText, programPath);
                    MessageBox.Show("MAT 文件类型注册成功！", "注册进度", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("注册 MAT 文件类型失败: " + ex.Message, "注册错误", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                
                // 刷新资源管理器以应用更改
                SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
                
                MessageBox.Show("右键菜单注册完成！\n\n" +
                    "请注意：\n" +
                    "1. 可能需要重启资源管理器或重新登录才能看到菜单\n" +
                    "2. 右键点击 .dml 或 .mat 文件时应该会看到 \"定位 ASE 文件\" 选项\n" +
                    "3. 如果仍然看不到，请尝试以管理员身份重新运行此程序", 
                    "注册完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("注册失败：需要管理员权限。\n\n请以管理员身份运行此程序来注册右键菜单。", 
                    "权限不足", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("注册右键菜单时发生错误: " + ex.Message + "\n\n详细信息: " + ex.ToString(), "注册失败", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 为指定文件类型注册右键菜单
        /// </summary>
        /// <param name="extension">文件扩展名</param>
        /// <param name="progId">程序标识符</param>
        /// <param name="menuText">菜单文本</param>
        /// <param name="programPath">程序路径</param>
        static void RegisterFileTypeContextMenu(string extension, string progId, string menuText, string programPath)
        {
            try
            {
                // 获取当前扩展名的程序标识符
                string currentProgId = "";
                using (RegistryKey extensionKey = Registry.ClassesRoot.OpenSubKey(extension))
                {
                    if (extensionKey != null)
                    {
                        object value = extensionKey.GetValue("");
                        if (value != null)
                            currentProgId = value.ToString();
                    }
                }
                
                // 如果没有现有的程序标识符，创建一个
                if (string.IsNullOrEmpty(currentProgId))
                {
                    using (RegistryKey extensionKey = Registry.ClassesRoot.CreateSubKey(extension))
                    {
                        extensionKey.SetValue("", progId);
                        currentProgId = progId;
                    }
                }
                else
                {
                    // 使用现有的程序标识符
                    progId = currentProgId;
                }
                
                // 在程序标识符下创建主菜单项 "ASE 工具"
                using (RegistryKey progIdKey = Registry.ClassesRoot.CreateSubKey(progId))
                {
                    using (RegistryKey shellKey = progIdKey.CreateSubKey("shell"))
                    {
                        // 创建主菜单 "ASE 工具"
                        using (RegistryKey mainMenuKey = shellKey.CreateSubKey("ASETools"))
                        {
                            mainMenuKey.SetValue("", "ASE 工具");
                            mainMenuKey.SetValue("Icon", programPath); // 添加图标
                            mainMenuKey.SetValue("MUIVerb", "ASE 工具");
                            mainMenuKey.SetValue("SubCommands", "");
                            
                            // 创建子菜单容器
                            using (RegistryKey shellSubKey = mainMenuKey.CreateSubKey("shell"))
                            {
                                // 子菜单1: 定位 ASE 文件
                                using (RegistryKey locateKey = shellSubKey.CreateSubKey("locate"))
                                {
                                    locateKey.SetValue("", "定位 ASE 文件");
                                    locateKey.SetValue("Icon", programPath);
                                    
                                    using (RegistryKey locateCommandKey = locateKey.CreateSubKey("command"))
                                    {
                                        locateCommandKey.SetValue("", "\"" + programPath + "\" \"locate\" \"%1\"");
                                    }
                                }
                                
                                // 子菜单2: 打开 ASE 文件  
                                using (RegistryKey openKey = shellSubKey.CreateSubKey("open"))
                                {
                                    openKey.SetValue("", "打开 ASE 文件");
                                    openKey.SetValue("Icon", programPath);
                                    
                                    using (RegistryKey openCommandKey = openKey.CreateSubKey("command"))
                                    {
                                        openCommandKey.SetValue("", "\"" + programPath + "\" \"open\" \"%1\"");
                                    }
                                }
                            }
                        }
                    }
                }
                
                // 如果扩展名有对应的 OpenWithProgids，也添加到那里
                try
                {
                    using (RegistryKey openWithKey = Registry.ClassesRoot.CreateSubKey(extension + "\\OpenWithProgids"))
                    {
                        openWithKey.SetValue(progId, "");
                    }
                }
                catch
                {
                    // 忽略 OpenWithProgids 的错误
                }
            }
            catch (Exception ex)
            {
                throw new Exception("注册文件类型 " + extension + " 失败: " + ex.Message, ex);
            }
        }
    }
} 