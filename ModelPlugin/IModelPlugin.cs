using System.Windows.Controls;

namespace ModelPlugin
{
    public interface IModelPlugin
    {
        /// <summary>
        /// 获取插件名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 获取插件描述
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 获取插件版本
        /// </summary>
        string Version { get; }

        /// <summary>
        /// 获取插件按钮
        /// </summary>
        /// <returns>要添加到工具栏的按钮</returns>
        Button[] GetButtons();

        /// <summary>
        /// 初始化插件
        /// </summary>
        /// <param name="modelPath">当前加载的模型路径</param>
        void Initialize(string modelPath);
    }
} 