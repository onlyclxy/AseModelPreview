using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DmlPathGet
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("DML路径获取工具 - 测试程序");
            Console.WriteLine("=======================");

            while (true)
            {
                Console.WriteLine("\n请输入要处理的DML文件路径 (输入'exit'退出):");
                string input = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("输入为空，请重新输入");
                    continue;
                }

                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                // 处理输入的文件
                ProcessDmlFile(input.Trim('"'));

                Console.WriteLine("\n按Enter继续处理下一个文件...");
                Console.ReadLine();
                Console.Clear();
            }
        }

        /// <summary>
        /// 处理单个DML文件
        /// </summary>
        /// <param name="dmlFilePath">DML文件路径</param>
        private static void ProcessDmlFile(string dmlFilePath)
        {
            Console.WriteLine($"\n正在处理文件: {dmlFilePath}");
            Console.WriteLine(new string('-', 50));

            try
            {
                // 解析DML文件
                List<string> asePaths = DmlParser.ParseDmlFile(dmlFilePath);

                if (asePaths.Count == 0)
                {
                    Console.WriteLine("未找到任何ASE文件路径");
                    return;
                }

                Console.WriteLine($"\n找到 {asePaths.Count} 个ASE文件:");
                foreach (string path in asePaths)
                {
                    Console.WriteLine(path);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理文件失败: {ex.Message}");
            }
        }
    }
}
