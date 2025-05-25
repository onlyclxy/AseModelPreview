﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AseTest
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 创建主窗口
            var mainWindow = new MainWindow();

            // 如果有命令行参数（文件路径），尝试加载模型
            if (e.Args.Length > 0)
            {
                string filePath = e.Args[0];
                // 使用 Dispatcher 确保在UI线程上执行
                mainWindow.Dispatcher.BeginInvoke(new Action(() =>
                {
                    mainWindow.LoadModelFromPath(filePath);
                }));
            }

            mainWindow.Show();
        }
    }
}
