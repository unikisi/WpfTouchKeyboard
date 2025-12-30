using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfTouchKeyboard.Core;
using WpfTouchKeyboard.Managers;

namespace WpfTouchKeyboard.Keyboards
{
    /// <summary>
    /// FullKeyboardView.xaml 的交互逻辑
    /// </summary>
    public partial class FullKeyboardView : UserControl
    {
        private bool _isShifted;
        private bool _isCapsLocked;
        private bool _isChineseMode = false;
        private readonly Dictionary<string, string> _shiftSymbols = new()
        {
            {"`", "~"}, {"1", "!"}, {"2", "@"}, {"3", "#"}, {"4", "$"}, {"5", "%"},
            {"6", "^"}, {"7", "&"}, {"8", "*"}, {"9", "("}, {"0", ")"},
            {"-", "_"}, {"=", "+"}, {"[", "{"}, {"]", "}"}, {"\\", "|"},
            {";", ":"}, {"'", "\""}, {",", "<"}, {".", ">"}, {"/", "?"}
        };

        public FullKeyboardView()
        {
            InitializeComponent();
            // 初始化按钮样式为英文模式
            LanguageToggleButton.Background = new SolidColorBrush(Color.FromRgb(0x21, 0x96, 0xF3));
            LanguageToggleButton.Foreground = new SolidColorBrush(Colors.White);
            
            // 根据全局属性控制按钮可见性
            LanguageToggleButton.Visibility = KeyboardManager.ShowLanguageToggleButton 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        private void Key_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: string key })
                return;

            // 标记事件已处理，防止焦点丢失
            e.Handled = true;

            if (key == "Shift")
            {
                _isShifted = !_isShifted;
                UpdateKeyVisuals();
                return;
            }

            if (key == "CapsLock")
            {
                _isCapsLocked = !_isCapsLocked;
                UpdateKeyVisuals();
                return;
            }

            if (InternalKeyboardManager.CurrentTarget is not { } inputTarget)
                return;

            // 获取目标控件，用于设置 IME 状态
            var targetElement = GetTargetElement(inputTarget);
            
            // 立即将焦点返回到 TextBox，这是关键！
            if (targetElement is Control control && control.Focusable)
            {
                control.Focus();
            }
            
            // 中文模式下，确保 IME 已启用
            if (_isChineseMode && targetElement != null)
            {
                ImeHelper.EnableChineseIME(targetElement);
            }
            else if (!_isChineseMode && targetElement != null)
            {
                ImeHelper.DisableChineseIME(targetElement);
            }

            if (key == "Back")
            {
                SendKeyPress(Key.Back, targetElement);
            }
            else if (key == "Enter")
            {
                SendKeyPress(Key.Enter, targetElement);
            }
            else if (key == "Tab")
            {
                SendKeyPress(Key.Tab, targetElement);
            }
            else if (key == " ")
            {
                SendKeyPress(Key.Space, targetElement);
            }
            else
            {
                // 中文模式下，使用 SendChar 让系统 IME 处理
                // 英文模式下，直接插入文本
                if (_isChineseMode)
                {
                    // 中文模式：使用字符输入，让系统 IME 处理
                    var ch = key[0];
                    if (char.IsLetter(ch))
                    {
                        var letter = (_isCapsLocked ^ _isShifted) ? char.ToUpper(ch) : char.ToLower(ch);
                        SendCharInput(letter, targetElement, _isShifted);
                    }
                    else if (char.IsDigit(ch))
                    {
                        SendCharInput(ch, targetElement);
                    }
                    else
                    {
                        // 处理符号键，包括shift符号
                        if (key == "`")
                        {
                            var charToSend = _isShifted ? '~' : '`';
                            SendCharInput(charToSend, targetElement);
                        }
                        else
                        {
                            // 符号也通过按键发送
                            var virtualKey = GetVirtualKeyFromChar(ch);
                            if (virtualKey.HasValue)
                            {
                                SendKeyPress((Key)virtualKey.Value, targetElement);
                            }
                        }
                    }
                }
                else
                {
                    // 英文模式：直接插入文本
                    var valueToInsert = key;
                    if (_shiftSymbols.TryGetValue(key, out var symbol) && _isShifted)
                    {
                        valueToInsert = symbol;
                    }
                    else if (char.IsLetter(key[0]))
                    {
                        valueToInsert = (_isCapsLocked ^ _isShifted) ? key.ToUpper() : key.ToLower();
                    }
                    inputTarget.InsertText(valueToInsert);
                }

                if (_isShifted)
                {
                    _isShifted = false;
                    UpdateKeyVisuals();
                }
            }
        }

        private FrameworkElement? GetTargetElement(IKeyboardInputTarget inputTarget)
        {
            return inputTarget switch
            {
                TextBoxInputTarget t => t.Target,
                PasswordBoxInputTarget p => p.Target,
                _ => null
            };
        }

        private void SendKeyPress(Key key, FrameworkElement? targetElement)
        {
            if (targetElement == null)
                return;

            // 确保目标元素获得焦点
            if (targetElement is Control control && control.Focusable)
            {
                if (!control.IsFocused)
                {
                    control.Focus();
                    System.Threading.Thread.Sleep(30);
                }
            }

            // 中文模式下确保 IME 启用
            if (_isChineseMode)
            {
                ImeHelper.EnableChineseIME(targetElement);
            }

            // 将 WPF Key 转换为虚拟键码
            var virtualKey = KeyInterop.VirtualKeyFromKey(key);
            ImeHelper.SendVirtualKey((ushort)virtualKey, false);
            System.Threading.Thread.Sleep(10); // 短暂延迟
            ImeHelper.SendVirtualKey((ushort)virtualKey, true);
            
            // 发送后立即将焦点返回到 TextBox
            System.Threading.Thread.Sleep(10);
            if (targetElement is Control ctrl && ctrl.Focusable && !ctrl.IsFocused)
            {
                ctrl.Focus();
            }
        }

        private void SendCharInput(char ch, FrameworkElement? targetElement, bool shiftPressed = false)
        {
            if (targetElement == null)
                return;

            // 确保目标元素始终有焦点，这是持续输入的关键！
            if (targetElement is Control control && control.Focusable)
            {
                if (!control.IsFocused)
                {
                    control.Focus();
                    System.Threading.Thread.Sleep(30);
                }
            }

            // 确保 IME 已启用
            ImeHelper.EnableChineseIME(targetElement);

            // 使用虚拟键码发送按键，让系统 IME（搜狗输入法）处理
            ImeHelper.SendChar(ch, shiftPressed);
            
            // 发送后立即将焦点返回到 TextBox，确保可以继续输入
            System.Threading.Thread.Sleep(10);
            if (targetElement is Control ctrl && ctrl.Focusable && !ctrl.IsFocused)
            {
                ctrl.Focus();
            }
        }

        private Key? GetVirtualKeyFromChar(char ch)
        {
            // 将字符映射到虚拟键码（简化版）
            return ch switch
            {
                '-' => Key.OemMinus,
                '+' => Key.OemPlus,
                '[' => Key.OemOpenBrackets,
                ']' => Key.OemCloseBrackets,
                '\\' => Key.OemPipe,
                '{' => Key.OemOpenBrackets,
                '}' => Key.OemCloseBrackets,
                '|' => Key.OemPipe,
                ';' => Key.OemSemicolon,
                ':' => Key.OemSemicolon,
                '\'' => Key.OemQuotes,
                '"' => Key.OemQuotes,
                ',' => Key.OemComma,
                '<' => Key.OemComma,
                '.' => Key.OemPeriod,
                '>' => Key.OemPeriod,
                '/' => Key.OemQuestion,
                '?' => Key.OemQuestion,
                _ => null
            };
        }

        private void LanguageToggle_Click(object sender, RoutedEventArgs e)
        {
            // 标记事件已处理，防止焦点问题
            e.Handled = true;
            
            _isChineseMode = !_isChineseMode;
            
            var target = InternalKeyboardManager.CurrentTarget;
            var targetElement = target != null ? GetTargetElement(target) : null;
            
            if (_isChineseMode)
            {
                LanguageToggleButton.Content = "中";
                // 中文模式：绿色背景，白色文字
                LanguageToggleButton.Background = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
                LanguageToggleButton.Foreground = new SolidColorBrush(Colors.White);
                
                // 切换系统输入法到中文（类似 Win+空格的效果）
                ImeHelper.SwitchToChineseInput();
                System.Threading.Thread.Sleep(50); // 等待系统输入法切换完成
                
                if (targetElement != null)
                {
                    // 确保控件有焦点
                    if (targetElement is Control control && control.Focusable)
                    {
                        control.Focus();
                        System.Threading.Thread.Sleep(30);
                    }
                    // 启用IME状态
                    ImeHelper.EnableChineseIME(targetElement);
                }
            }
            else
            {
                LanguageToggleButton.Content = "英";
                // 英文模式：蓝色背景，白色文字
                LanguageToggleButton.Background = new SolidColorBrush(Color.FromRgb(0x21, 0x96, 0xF3));
                LanguageToggleButton.Foreground = new SolidColorBrush(Colors.White);
                
                // 切换系统输入法到英文（类似 Win+空格的效果）
                ImeHelper.SwitchToEnglishInput();
                System.Threading.Thread.Sleep(50); // 等待系统输入法切换完成
                
                if (targetElement != null)
                {
                    // 确保控件有焦点
                    if (targetElement is Control control && control.Focusable)
                    {
                        control.Focus();
                        System.Threading.Thread.Sleep(30);
                    }
                    ImeHelper.DisableChineseIME(targetElement);
                }
            }
        }

        private void UpdateKeyVisuals()
        {
            foreach (var btn in FindVisualChildren<Button>(this))
            {
                if (btn.Tag is not string key) continue;

                if (key == "Shift" || key == "CapsLock" || key == "Back" || key == " " || key == "Enter" || key == "Tab")
                    continue;

                if (char.IsLetter(key[0]))
                    btn.Content = (_isCapsLocked ^ _isShifted) ? key.ToUpper() : key.ToLower();
                else if (_shiftSymbols.TryGetValue(key, out var symbol))
                    btn.Content = _isShifted ? symbol : key;
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T t)
                    yield return t;

                foreach (var childOfChild in FindVisualChildren<T>(child))
                    yield return childOfChild;
            }
        }
    }
}
