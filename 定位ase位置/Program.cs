using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
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
        private const uint SHCNF_IDLIST = 0x0000; // 或者使用 SHCNF_FLUSHNOWAIT 0x2000

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
                    MessageBox.Show($"未找到对应的 ASE 文件: {aseFilePath}", "文件未找到",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"处理文件时发生错误: {ex.Message}", "错误",
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
                        Arguments = $"\"{aseFilePath}\"",
                        UseShellExecute = true
                    };

                    Process.Start(startInfo);
                }
                else
                {
                    MessageBox.Show($"未找到 ModelPreview.exe: {modelPreviewPath}", "程序未找到",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动 ModelPreview.exe 时发生错误: {ex.Message}", "错误",
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
                    Arguments = $"/select,\"{filePath}\"",
                    UseShellExecute = true
                };

                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"定位文件时发生错误: {ex.Message}", "错误",
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
                // 主级联菜单的名称
                string mainMenuDisplayText = "ASE 工具";

                MessageBox.Show($"开始注册右键菜单...\n程序路径: {programPath}", "注册信息",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 为 .dml 文件注册 - 提供一个ProgId提示或让函数生成
                try
                {
                    // 例如，为.dml文件指定一个基础ProgId "ASETool.dmlfile"
                    RegisterFileTypeContextMenu(".dml", "ASETool.dmlfile", mainMenuDisplayText, programPath);
                    MessageBox.Show("DML 文件类型右键菜单注册成功！", "注册进度",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"注册 DML 文件类型右键菜单失败: {ex.Message}", "注册错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // 为 .mat 文件注册
                try
                {
                    // 例如，为.mat文件指定一个基础ProgId "ASETool.matfile"
                    RegisterFileTypeContextMenu(".mat", "ASETool.matfile", mainMenuDisplayText, programPath);
                    MessageBox.Show("MAT 文件类型右键菜单注册成功！", "注册进度",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"注册 MAT 文件类型右键菜单失败: {ex.Message}", "注册错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // 刷新资源管理器以应用更改
                SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);

                MessageBox.Show("右键菜单注册完成！\n\n" +
                    "请注意：\n" +
                    "1. 可能需要重启资源管理器或重新登录才能看到菜单。\n" +
                    "2. 右键点击 .dml 或 .mat 文件时应该会看到 \"" + mainMenuDisplayText + "\" 选项及其子菜单。\n" +
                    "3. 如果仍然看不到，请尝试以管理员身份重新运行此程序。",
                    "注册完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("注册失败：需要管理员权限。\n\n请以管理员身份运行此程序来注册右键菜单。",
                    "权限不足", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"注册右键菜单时发生一般性错误: {ex.Message}\n\n详细信息: {ex.StackTrace}", "注册失败",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 为指定文件类型注册右键菜单 (修改后)
        /// </summary>
        /// <param name="extension">文件扩展名 (例如 ".txt")</param>
        /// <param name="progIdHint">建议的程序标识符 (例如 "MyApplication.txtfile")，如果为空则自动生成</param>
        /// <param name="mainMenuDisplayText">主级联菜单项上显示的文本 (例如 "我的工具")</param>
        /// <param name="programPath">执行此菜单命令的程序完整路径</param>
        static void RegisterFileTypeContextMenu(string extension, string progIdHint, string mainMenuDisplayText, string programPath)
        {
            string actualProgIdToUse = null; // 将用于注册Shell命令的ProgId
            try
            {
                // 步骤 1: 确定或创建并关联ProgId
                using (RegistryKey extKeyRead = Registry.ClassesRoot.OpenSubKey(extension))
                {
                    actualProgIdToUse = extKeyRead?.GetValue("") as string;
                }

                bool progIdWasCreatedOrAssociated = false;
                if (string.IsNullOrEmpty(actualProgIdToUse))
                {
                    // 没有现有的ProgId，或者ProgId为空字符串（无效）
                    if (!string.IsNullOrEmpty(progIdHint))
                    {
                        actualProgIdToUse = progIdHint;
                    }
                    else
                    {
                        // 基于扩展名生成一个新的ProgId
                        actualProgIdToUse = "ASELocator." + extension.TrimStart('.').ToLowerInvariant() + "File";
                    }

                    // 将此ProgId与扩展名关联
                    using (RegistryKey extKeyWrite = Registry.ClassesRoot.CreateSubKey(extension))
                    {
                        extKeyWrite.SetValue("", actualProgIdToUse);
                    }
                    progIdWasCreatedOrAssociated = true;
                    // (可选) 为此ProgId设置一个友好名称，会显示在 "打开方式" 对话框中
                    using (RegistryKey progIdFriendlyNameKey = Registry.ClassesRoot.CreateSubKey(actualProgIdToUse))
                    {
                        progIdFriendlyNameKey.SetValue("", $"ASE Locator Handled {extension.ToUpper()} File (ProgId: {actualProgIdToUse})");
                    }
                }
                if (progIdWasCreatedOrAssociated)
                {
                    Console.WriteLine($"ProgId '{actualProgIdToUse}' was associated with '{extension}'.");
                }
                else
                {
                    Console.WriteLine($"Using existing ProgId '{actualProgIdToUse}' for '{extension}'.");
                }


                // 步骤 2: 在确定的ProgId下注册Shell命令和子菜单
                // 注册表路径: HKEY_CLASSES_ROOT\<actualProgIdToUse>\shell\<MainMenuVerb>
                string mainMenuVerb = "ASETools"; // 主菜单的内部名称/动词

                using (RegistryKey progIdNodeKey = Registry.ClassesRoot.OpenSubKey(actualProgIdToUse, true) ?? Registry.ClassesRoot.CreateSubKey(actualProgIdToUse))
                {
                    if (progIdNodeKey == null)
                    {
                        throw new Exception($"Failed to open or create ProgId key: {actualProgIdToUse}");
                    }
                    using (RegistryKey shellKey = progIdNodeKey.CreateSubKey("shell"))
                    {
                        using (RegistryKey mainMenuKey = shellKey.CreateSubKey(mainMenuVerb))
                        {
                            mainMenuKey.SetValue("", mainMenuDisplayText); // 显示文本，例如 "ASE 工具"
                            mainMenuKey.SetValue("MUIVerb", mainMenuDisplayText); // 推荐使用MUIVerb以支持多语言和现代UI
                            mainMenuKey.SetValue("Icon", programPath); // 为父菜单项设置图标
                            mainMenuKey.SetValue("SubCommands", ""); // 关键：表示这是一个级联菜单，其子项在其自己的 "shell" 子键中定义

                            // 创建子菜单项的容器 (shell子键)
                            using (RegistryKey subMenuShellKey = mainMenuKey.CreateSubKey("shell"))
                            {
                                // 子菜单项 1: 定位 ASE 文件
                                string locateVerb = "LocateASE"; // 子菜单项1的内部名称/动词
                                using (RegistryKey locateKey = subMenuShellKey.CreateSubKey(locateVerb))
                                {
                                    locateKey.SetValue("", "定位 ASE 文件"); // 子菜单项1的显示文本
                                    locateKey.SetValue("Icon", programPath); // 为子菜单项设置图标
                                    using (RegistryKey commandKey = locateKey.CreateSubKey("command"))
                                    {
                                        commandKey.SetValue("", $"\"{programPath}\" \"locate\" \"%1\"");
                                    }
                                }

                                // 子菜单项 2: 打开 ASE 文件
                                string openVerb = "OpenASE"; // 子菜单项2的内部名称/动词
                                using (RegistryKey openKey = subMenuShellKey.CreateSubKey(openVerb))
                                {
                                    openKey.SetValue("", "打开 ASE 文件"); // 子菜单项2的显示文本
                                    openKey.SetValue("Icon", programPath); // 为子菜单项设置图标
                                    using (RegistryKey commandKey = openKey.CreateSubKey("command"))
                                    {
                                        commandKey.SetValue("", $"\"{programPath}\" \"open\" \"%1\"");
                                    }
                                }
                                // 如果需要，可以在这里添加更多子菜单项...
                            }
                        }
                    }
                }


                // (可选但推荐) 将此ProgId添加到扩展名的OpenWithProgids列表中
                // 这有助于 "打开方式" 对话框识别此应用程序
                try
                {
                    using (RegistryKey openWithKey = Registry.ClassesRoot.CreateSubKey($"{extension}\\OpenWithProgids"))
                    {
                        openWithKey.SetValue(actualProgIdToUse, ""); // 值为空字符串
                    }
                }
                catch (Exception exOpenWith)
                {
                    Console.WriteLine($"Warning: Could not register {actualProgIdToUse} in OpenWithProgids for {extension}: {exOpenWith.Message}");
                    // 忽略此处的错误，因为它不是右键菜单核心功能所必需的
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                // 由 RegisterContextMenu() 中的外层catch处理权限问题
                throw new UnauthorizedAccessException($"注册文件类型 {extension} (ProgId: {actualProgIdToUse ?? "未确定"}) 失败: 需要管理员权限。", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"为文件类型 {extension} (ProgId: {actualProgIdToUse ?? "未确定"}) 注册右键菜单时发生错误: {ex.Message}", ex);
            }
        }
    }
}
