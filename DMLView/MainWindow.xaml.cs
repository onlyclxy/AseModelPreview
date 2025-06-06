using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Windows.Threading;

namespace DMLView
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private string currentFilePath = "";
        private bool isTextChanged = false;
        private bool syntaxHighlightEnabled = true;
        private bool lineNumbersEnabled = true;
        private DispatcherTimer syntaxHighlightTimer;

        public MainWindow()
        {
            InitializeComponent();
            InitializeSyntaxTimer();
            InitializeEditor();
            RegisterKeyBindings();
        }

        private void InitializeSyntaxTimer()
        {
            syntaxHighlightTimer = new DispatcherTimer();
            syntaxHighlightTimer.Interval = TimeSpan.FromMilliseconds(500); // 500ms延迟
            syntaxHighlightTimer.Tick += (s, e) => {
                syntaxHighlightTimer.Stop();
                if (syntaxHighlightEnabled)
                {
                    ApplySyntaxHighlighting();
                }
            };
        }

        private void InitializeEditor()
        {
            // 设置默认内容
            string defaultContent = "// 欢迎使用DML编辑器\r\n// 支持DML格式语法高亮\r\n\r\n[SHADER]\r\n示例内容\r\n[GEOMETRY MESH]\r\n..\\resources\\art\\eff\\chair\\c25851\\model\\c25851_md_xuanzhuan02.ase";
            
            var paragraph = new Paragraph();
            paragraph.Inlines.Add(new Run(defaultContent));
            TextEditor.Document.Blocks.Clear();
            TextEditor.Document.Blocks.Add(paragraph);
            
            // 初始化后应用语法高亮
            if (syntaxHighlightEnabled)
            {
                ApplySyntaxHighlighting();
            }
            
            UpdateLineNumbers();
            UpdateStatusBar();
        }

        private void RegisterKeyBindings()
        {
            // 注册快捷键
            this.KeyDown += MainWindow_KeyDown;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.N:
                        NewFile_Click(null, null);
                        e.Handled = true;
                        break;
                    case Key.O:
                        OpenFile_Click(null, null);
                        e.Handled = true;
                        break;
                    case Key.S:
                        if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
                            SaveAsFile_Click(null, null);
                        else
                            SaveFile_Click(null, null);
                        e.Handled = true;
                        break;
                    case Key.F:
                        Find_Click(null, null);
                        e.Handled = true;
                        break;
                    case Key.H:
                        Replace_Click(null, null);
                        e.Handled = true;
                        break;
                }
            }
        }

        #region 文件操作

        private void NewFile_Click(object sender, RoutedEventArgs e)
        {
            if (CheckUnsavedChanges())
            {
                TextEditor.Document.Blocks.Clear();
                TextEditor.Document.Blocks.Add(new Paragraph());
                currentFilePath = "";
                isTextChanged = false;
                UpdateTitle();
                UpdateStatusBar();
                UpdateLineNumbers();
            }
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            if (CheckUnsavedChanges())
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "DML文件 (*.dml)|*.dml|所有文件 (*.*)|*.*";
                openFileDialog.Title = "打开DML文件";

                if (openFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        string content = File.ReadAllText(openFileDialog.FileName, Encoding.UTF8);
                        var paragraph = new Paragraph();
                        paragraph.Inlines.Add(new Run(content));
                        TextEditor.Document.Blocks.Clear();
                        TextEditor.Document.Blocks.Add(paragraph);

                        currentFilePath = openFileDialog.FileName;
                        isTextChanged = false;
                        UpdateTitle();
                        ApplySyntaxHighlighting();
                        UpdateLineNumbers();
                        UpdateStatusBar();
                        StatusText.Text = $"已打开文件: {System.IO.Path.GetFileName(currentFilePath)}";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"打开文件时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                SaveAsFile_Click(sender, e);
            }
            else
            {
                SaveFile(currentFilePath);
            }
        }

        private void SaveAsFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "DML文件 (*.dml)|*.dml|所有文件 (*.*)|*.*";
            saveFileDialog.Title = "保存DML文件";
            saveFileDialog.DefaultExt = "dml";

            if (saveFileDialog.ShowDialog() == true)
            {
                SaveFile(saveFileDialog.FileName);
                currentFilePath = saveFileDialog.FileName;
                UpdateTitle();
            }
        }

        private void SaveFile(string filePath)
        {
            try
            {
                string content = GetTextFromRichTextBox();
                File.WriteAllText(filePath, content, Encoding.UTF8);
                isTextChanged = false;
                UpdateTitle();
                StatusText.Text = $"已保存文件: {System.IO.Path.GetFileName(filePath)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存文件时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (CheckUnsavedChanges())
            {
                Application.Current.Shutdown();
            }
        }

        #endregion

        #region 编辑操作

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            if (TextEditor.CanUndo)
                TextEditor.Undo();
        }

        private void Redo_Click(object sender, RoutedEventArgs e)
        {
            if (TextEditor.CanRedo)
                TextEditor.Redo();
        }

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            TextEditor.Cut();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            TextEditor.Copy();
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            TextEditor.Paste();
        }

        private void Find_Click(object sender, RoutedEventArgs e)
        {
            // 创建查找对话框
            var findDialog = new FindDialog();
            findDialog.Owner = this;
            if (findDialog.ShowDialog() == true)
            {
                FindText(findDialog.SearchText, findDialog.MatchCase, findDialog.WholeWord);
            }
        }

        private void Replace_Click(object sender, RoutedEventArgs e)
        {
            // 创建替换对话框
            var replaceDialog = new ReplaceDialog();
            replaceDialog.Owner = this;
            replaceDialog.ShowDialog();
        }

        #endregion

        #region 视图操作

        private void Font_Click(object sender, RoutedEventArgs e)
        {
            // 字体选择对话框
            var fontDialog = new System.Windows.Forms.FontDialog();
            fontDialog.Font = new System.Drawing.Font(TextEditor.FontFamily.Source, (float)TextEditor.FontSize);
            
            if (fontDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TextEditor.FontFamily = new FontFamily(fontDialog.Font.Name);
                TextEditor.FontSize = fontDialog.Font.Size;
                LineNumberTextBlock.FontFamily = TextEditor.FontFamily;
                LineNumberTextBlock.FontSize = TextEditor.FontSize;
            }
        }

        private void ToggleSyntaxHighlight(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SyntaxHighlightMenuItem != null)
                {
                    syntaxHighlightEnabled = SyntaxHighlightMenuItem.IsChecked;
                    if (syntaxHighlightEnabled)
                    {
                        ApplySyntaxHighlighting();
                    }
                    else
                    {
                        RemoveSyntaxHighlighting();
                    }
                }
            }
            catch (Exception)
            {
                // 静默处理错误
            }
        }

        private void ToggleLineNumbers(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LineNumberMenuItem != null)
                {
                    lineNumbersEnabled = LineNumberMenuItem.IsChecked;
                    if (LineNumberTextBlock != null)
                    {
                        LineNumberTextBlock.Visibility = lineNumbersEnabled ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
            catch (Exception)
            {
                // 静默处理错误
            }
        }

        #endregion

        #region 语法高亮

        private void ApplySyntaxHighlighting()
        {
            try
            {
                if (!syntaxHighlightEnabled || TextEditor?.Document == null) return;

                // 保存当前光标位置
                var caretPosition = TextEditor.CaretPosition;
                
                // 获取纯文本内容
                string text = new TextRange(TextEditor.Document.ContentStart, TextEditor.Document.ContentEnd).Text;
                if (string.IsNullOrEmpty(text)) return;

                // 暂时禁用事件处理避免递归
                TextEditor.TextChanged -= TextEditor_TextChanged;

                // 清除现有内容
                TextEditor.Document.Blocks.Clear();
                
                // 创建新的段落
                var paragraph = new Paragraph();
                paragraph.Margin = new Thickness(0);
                
                // 按行处理文本
                var lines = text.Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
                
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    
                    // 添加换行符（除了第一行）
                    if (i > 0)
                    {
                        paragraph.Inlines.Add(new LineBreak());
                    }
                    
                    // 应用语法高亮
                    if (string.IsNullOrEmpty(line))
                    {
                        // 空行，添加一个空的Run以保持行结构
                        paragraph.Inlines.Add(new Run(""));
                    }
                    else if (Regex.IsMatch(line, @"^\s*\[.*\]\s*$"))
                    {
                        // 段标题 [SECTION]
                        var run = new Run(line);
                        run.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x66, 0xCC));
                        run.FontWeight = FontWeights.Bold;
                        paragraph.Inlines.Add(run);
                    }
                    else if (Regex.IsMatch(line, @".*[\\\/].*\.(ase|mat|fbx|obj|dae|3ds|blend|max).*", RegexOptions.IgnoreCase))
                    {
                        // 文件路径
                        var run = new Run(line);
                        run.Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0x80, 0x00));
                        paragraph.Inlines.Add(run);
                    }
                    else if (line.TrimStart().StartsWith("//") || line.TrimStart().StartsWith("#"))
                    {
                        // 注释
                        var run = new Run(line);
                        run.Foreground = new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80));
                        run.FontStyle = FontStyles.Italic;
                        paragraph.Inlines.Add(run);
                    }
                    else
                    {
                        // 普通文本
                        paragraph.Inlines.Add(new Run(line));
                    }
                }

                // 添加段落到文档
                TextEditor.Document.Blocks.Add(paragraph);
                
                // 恢复光标位置
                try
                {
                    TextEditor.CaretPosition = caretPosition;
                }
                catch
                {
                    // 如果无法恢复原位置，设置到文档开始
                    TextEditor.CaretPosition = TextEditor.Document.ContentStart;
                }
                
                // 重新启用事件处理
                TextEditor.TextChanged += TextEditor_TextChanged;
            }
            catch (Exception)
            {
                // 如果语法高亮失败，重新启用事件处理
                TextEditor.TextChanged += TextEditor_TextChanged;
            }
        }

        private void RemoveSyntaxHighlighting()
        {
            try
            {
                if (TextEditor?.Document != null)
                {
                    string text = GetTextFromRichTextBox();
                    var paragraph = new Paragraph();
                    paragraph.Inlines.Add(new Run(text));
                    TextEditor.Document.Blocks.Clear();
                    TextEditor.Document.Blocks.Add(paragraph);
                }
            }
            catch (Exception)
            {
                // 静默处理错误
            }
        }

        #endregion

        #region 辅助方法

        private string GetTextFromRichTextBox()
        {
            try
            {
                if (TextEditor?.Document != null)
                {
                    TextRange textRange = new TextRange(TextEditor.Document.ContentStart, TextEditor.Document.ContentEnd);
                    return textRange.Text ?? "";
                }
                return "";
            }
            catch (Exception)
            {
                return "";
            }
        }

        private bool CheckUnsavedChanges()
        {
            if (isTextChanged)
            {
                var result = MessageBox.Show("文件已修改，是否保存更改？", "确认", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    SaveFile_Click(null, null);
                    return !isTextChanged; // 如果保存失败，返回false
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return false;
                }
            }
            return true;
        }

        private void UpdateTitle()
        {
            string fileName = string.IsNullOrEmpty(currentFilePath) ? "无标题" : System.IO.Path.GetFileName(currentFilePath);
            string modified = isTextChanged ? "*" : "";
            this.Title = $"{fileName}{modified} - DML编辑器";
        }

        private void UpdateLineNumbers()
        {
            try
            {
                if (!lineNumbersEnabled || LineNumberTextBlock == null) return;

                string text = GetTextFromRichTextBox();
                int lineCount = text.Split('\n').Length;
                
                StringBuilder lineNumbers = new StringBuilder();
                for (int i = 1; i <= lineCount; i++)
                {
                    lineNumbers.AppendLine(i.ToString());
                }
                
                LineNumberTextBlock.Text = lineNumbers.ToString().TrimEnd();
            }
            catch (Exception)
            {
                // 静默处理错误
            }
        }

        private void UpdateStatusBar()
        {
            try
            {
                if (TextEditor?.CaretPosition == null || LineColumnStatus == null) return;

                var caretPosition = TextEditor.CaretPosition;
                var lineNumber = 1;
                var columnNumber = 1;

                // 计算行号
                var start = TextEditor.Document.ContentStart;
                var lineStart = start.GetLineStartPosition(0);
                
                while (lineStart != null && lineStart.CompareTo(caretPosition) < 0)
                {
                    lineNumber++;
                    lineStart = lineStart.GetLineStartPosition(1);
                }

                // 计算列号
                var lineStartPos = caretPosition.GetLineStartPosition(0);
                if (lineStartPos != null)
                {
                    columnNumber = lineStartPos.GetOffsetToPosition(caretPosition) + 1;
                }

                LineColumnStatus.Text = $"行: {lineNumber}, 列: {columnNumber}";
            }
            catch
            {
                if (LineColumnStatus != null)
                    LineColumnStatus.Text = "行: 1, 列: 1";
            }
        }

        private void FindText(string searchText, bool matchCase, bool wholeWord)
        {
            string content = GetTextFromRichTextBox();
            StringComparison comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            
            int index = content.IndexOf(searchText, comparison);
            if (index >= 0)
            {
                // 选中找到的文本
                TextPointer start = TextEditor.Document.ContentStart.GetPositionAtOffset(index);
                TextPointer end = start.GetPositionAtOffset(searchText.Length);
                TextEditor.Selection.Select(start, end);
                TextEditor.Focus();
                StatusText.Text = "找到匹配项";
            }
            else
            {
                StatusText.Text = "未找到匹配项";
                MessageBox.Show("未找到指定的文本。", "查找", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #endregion

        #region 事件处理

        private void TextEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            isTextChanged = true;
            UpdateTitle();
            UpdateLineNumbers();
            
            // 重启计时器来延迟语法高亮
            if (syntaxHighlightEnabled)
            {
                syntaxHighlightTimer.Stop();
                syntaxHighlightTimer.Start();
            }
        }

        private void TextEditor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            UpdateStatusBar();
        }

        private void TextEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 处理Tab键
            if (e.Key == Key.Tab)
            {
                TextEditor.Selection.Text = "    "; // 4个空格代替Tab
                e.Handled = true;
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("DML编辑器 v1.0\n\n专为DML格式文件设计的文本编辑器\n支持语法高亮、行号显示等功能", 
                          "关于", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion
    }

    #region 对话框类

    public partial class FindDialog : Window
    {
        public string SearchText { get; private set; }
        public bool MatchCase { get; private set; }
        public bool WholeWord { get; private set; }

        public FindDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Title = "查找";
            this.Width = 400;
            this.Height = 200;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.ResizeMode = ResizeMode.NoResize;

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var searchLabel = new Label { Content = "查找内容:", Margin = new Thickness(10) };
            Grid.SetRow(searchLabel, 0);
            grid.Children.Add(searchLabel);

            var searchTextBox = new TextBox { Name = "SearchTextBox", Margin = new Thickness(10) };
            Grid.SetRow(searchTextBox, 1);
            grid.Children.Add(searchTextBox);

            var optionsPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(10) };
            var matchCaseCheckBox = new CheckBox { Content = "区分大小写", Name = "MatchCaseCheckBox" };
            var wholeWordCheckBox = new CheckBox { Content = "全字匹配", Name = "WholeWordCheckBox", Margin = new Thickness(20, 0, 0, 0) };
            optionsPanel.Children.Add(matchCaseCheckBox);
            optionsPanel.Children.Add(wholeWordCheckBox);
            Grid.SetRow(optionsPanel, 2);
            grid.Children.Add(optionsPanel);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(10) };
            var findButton = new Button { Content = "查找", Width = 75, Margin = new Thickness(5) };
            var cancelButton = new Button { Content = "取消", Width = 75, Margin = new Thickness(5) };
            
            findButton.Click += (s, e) => {
                SearchText = searchTextBox.Text;
                MatchCase = matchCaseCheckBox.IsChecked ?? false;
                WholeWord = wholeWordCheckBox.IsChecked ?? false;
                this.DialogResult = true;
            };
            
            cancelButton.Click += (s, e) => this.DialogResult = false;
            
            buttonPanel.Children.Add(findButton);
            buttonPanel.Children.Add(cancelButton);
            Grid.SetRow(buttonPanel, 3);
            grid.Children.Add(buttonPanel);

            this.Content = grid;
        }
    }

    public partial class ReplaceDialog : Window
    {
        public ReplaceDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Title = "替换";
            this.Width = 400;
            this.Height = 250;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.ResizeMode = ResizeMode.NoResize;

            var grid = new Grid();
            for (int i = 0; i < 6; i++)
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var findLabel = new Label { Content = "查找内容:", Margin = new Thickness(10) };
            Grid.SetRow(findLabel, 0);
            grid.Children.Add(findLabel);

            var findTextBox = new TextBox { Margin = new Thickness(10) };
            Grid.SetRow(findTextBox, 1);
            grid.Children.Add(findTextBox);

            var replaceLabel = new Label { Content = "替换为:", Margin = new Thickness(10) };
            Grid.SetRow(replaceLabel, 2);
            grid.Children.Add(replaceLabel);

            var replaceTextBox = new TextBox { Margin = new Thickness(10) };
            Grid.SetRow(replaceTextBox, 3);
            grid.Children.Add(replaceTextBox);

            var optionsPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(10) };
            var matchCaseCheckBox = new CheckBox { Content = "区分大小写" };
            var wholeWordCheckBox = new CheckBox { Content = "全字匹配", Margin = new Thickness(20, 0, 0, 0) };
            optionsPanel.Children.Add(matchCaseCheckBox);
            optionsPanel.Children.Add(wholeWordCheckBox);
            Grid.SetRow(optionsPanel, 4);
            grid.Children.Add(optionsPanel);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(10) };
            var findNextButton = new Button { Content = "查找下一个", Width = 100, Margin = new Thickness(5) };
            var replaceButton = new Button { Content = "替换", Width = 75, Margin = new Thickness(5) };
            var replaceAllButton = new Button { Content = "全部替换", Width = 100, Margin = new Thickness(5) };
            var cancelButton = new Button { Content = "关闭", Width = 75, Margin = new Thickness(5) };
            
            cancelButton.Click += (s, e) => this.Close();
            
            buttonPanel.Children.Add(findNextButton);
            buttonPanel.Children.Add(replaceButton);
            buttonPanel.Children.Add(replaceAllButton);
            buttonPanel.Children.Add(cancelButton);
            Grid.SetRow(buttonPanel, 5);
            grid.Children.Add(buttonPanel);

            this.Content = grid;
        }
    }

    #endregion
}
