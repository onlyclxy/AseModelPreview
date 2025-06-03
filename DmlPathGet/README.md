# DML路径获取工具

这是一个用于从DML文件中提取ASE文件路径的工具。它可以处理相对路径和孤立文件，并将它们转换为绝对路径。

## 功能特点

- 支持从[GEOMETRY MESH]和[SKIN]节中提取ASE文件路径
- 自动将相对路径转换为绝对路径
- 使用Everything搜索引擎查找孤立文件
- 支持工程目录配置管理
- 结果可以保存到文本文件

## 使用方法

1. 首次运行时，程序会提示选择工程目录：
   - 选择到resources目录（如：E:\X52_Project\QQX5-2_Exe\Vacation\resources）
   - 或选择其父目录（如：E:\X52_Project\QQX5-2_Exe\Vacation）

2. 工程目录会被保存在程序目录下的ProjectConfig.txt文件中

3. 输入要处理的DML文件路径，程序会：
   - 解析文件中的ASE路径
   - 将相对路径转换为绝对路径
   - 搜索孤立文件的实际位置
   - 显示所有找到的路径
   - 将结果保存到同名的_paths.txt文件中

## 依赖项

- Everything搜索引擎（需要安装Everything64.dll）
- .NET Framework 4.8

## 注意事项

1. 确保Everything搜索引擎已安装并运行
2. 工程目录必须包含resources子目录
3. 相对路径会基于工程目录进行解析
4. 孤立文件会在工程目录下递归搜索 