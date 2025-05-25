// Program.cs
using System;
using System.Windows;
using ThumbnailTestConsole;

namespace ThumbnailTestConsole
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var app = new Application();
            app.Run(new MainWindow());
        }
    }
}