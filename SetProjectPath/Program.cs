using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetProjectPath
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                // 检查命令行参数
                bool forceReconfigure = args.Length > 0 && 
                    (args[0].Equals("-r", StringComparison.OrdinalIgnoreCase) || 
                     args[0].Equals("--reconfigure", StringComparison.OrdinalIgnoreCase));

                string projectDir;

                if (forceReconfigure)
                {
                    // 强制重新配置
                    projectDir = ConfigManager.ForceReconfigureProjectDirectory();
                }
                else
                {
                    // 正常检查和配置
                    projectDir = ConfigManager.CheckAndSetProjectDirectory();
                }

                if (!string.IsNullOrEmpty(projectDir))
                {
                    Console.WriteLine("\n配置完成！");
                    Console.WriteLine("按任意键退出...");
                }
                else
                {
                    Console.WriteLine("\n配置失败或被取消。");
                    Console.WriteLine("按任意键退出...");
                }
                
                //Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"程序运行出错: {ex.Message}");
                Console.WriteLine("按任意键退出...");
                //Console.ReadKey();
            }
        }
    }
}
