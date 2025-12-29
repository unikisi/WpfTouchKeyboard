using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using WpfTouchKeyboard.Core;
using WpfTouchKeyboard.Keyboards;
using WpfTouchKeyboard.Settings;

namespace WpfTouchKeyboard.Managers
{
    internal static class InternalKeyboardManager
    {
        public static bool IsEnabled { get; set; } = true;

        internal static IKeyboardInputTarget? CurrentTarget { get; set; }
        private static Popup _popup = null!;
        private static KeyboardType _keyboardType = KeyboardType.All;
        private static Button? _closeButton = null;

        public static void Register()
        {
            EventManager.RegisterClassHandler(typeof(TextBox),
                UIElement.PreviewMouseDownEvent,
                new MouseButtonEventHandler((sender, e) =>
                {
                    if (sender is TextBox tb)
                    {
                        // 检查窗口级别是否启用键盘
                        if (!IsKeyboardEnabledForWindow(tb))
                            return;

                        // 检查控件级别是否启用键盘
                        if (KeyboardManager.GetEnableKeyboard(tb))
                        {
                            AttachTo(new TextBoxInputTarget(tb));
                        }
                    }
                }));

            EventManager.RegisterClassHandler(typeof(PasswordBox),
                UIElement.PreviewMouseDownEvent,
                new MouseButtonEventHandler((sender, e) =>
                {
                    if (sender is PasswordBox pb)
                    {
                        // 检查窗口级别是否启用键盘
                        if (!IsKeyboardEnabledForWindow(pb))
                            return;

                        // 检查控件级别是否启用键盘
                        if (KeyboardManager.GetEnableKeyboard(pb))
                        {
                            AttachTo(new PasswordBoxInputTarget(pb));
                        }
                    }
                }));

            EventManager.RegisterClassHandler(typeof(Window),
                UIElement.PreviewMouseDownEvent,
                new MouseButtonEventHandler(OnWindowMouseDown));

            EventManager.RegisterClassHandler(typeof(Window),
                UIElement.PreviewKeyDownEvent,
                new KeyEventHandler(OnWindowKeyDown));
        }

        public static void SetKeyboardType(KeyboardType type)
        {
            _keyboardType = type;
            ResetKeyboardPopup();
        }

        private static void ResetKeyboardPopup()
        {
            if (_popup != null)
            {
                _popup.IsOpen = false;
                _popup.Child = null;
                _popup = null!;
                CurrentTarget = null!;
            }
        }

        private static void AttachTo(IKeyboardInputTarget inputTarget)
        {
            if (!IsEnabled)
                return;

            if (_popup?.IsOpen == true && CurrentTarget == inputTarget)
                return;

            if (_popup == null)
            {
                FrameworkElement keyboardContent = _keyboardType switch
                {
                    KeyboardType.All => new FullKeyboardView(),
                    KeyboardType.Number => new NumericKeyboardView(),
                    _ => throw new ArgumentOutOfRangeException()
                };

                // 创建关闭按钮（始终创建，但通过 Visibility 控制显示）
                _closeButton = new Button
                {
                    Content = "×",
                    FontSize = 24,
                    FontWeight = FontWeights.Bold,
                    Width = 30,
                    Height = 30,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, -5, -5, 0),
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Foreground = Brushes.Gray,
                    Cursor = Cursors.Hand,
                    Focusable = false,
                    Padding = new Thickness(0),
                    Visibility = KeyboardManager.ShowCloseButton ? Visibility.Visible : Visibility.Collapsed
                };
                _closeButton.Click += (s, e) => CloseKeyboard();
                
                // 添加悬停效果
                _closeButton.MouseEnter += (s, e) =>
                {
                    if (s is Button btn)
                    {
                        btn.Foreground = Brushes.Red;
                        btn.Background = new SolidColorBrush(Color.FromArgb(30, 255, 0, 0));
                    }
                };
                _closeButton.MouseLeave += (s, e) =>
                {
                    if (s is Button btn)
                    {
                        btn.Foreground = Brushes.Gray;
                        btn.Background = Brushes.Transparent;
                    }
                };

                // 创建包含键盘和关闭按钮的 Grid
                var grid = new Grid();
                grid.Children.Add(keyboardContent);
                grid.Children.Add(_closeButton);
                var container = grid;

                _popup = new Popup
                {
                    StaysOpen = true,
                    AllowsTransparency = true,
                    Placement = PlacementMode.Bottom,
                    VerticalOffset = 10,
                    Child = new Border
                    {
                        Background = Brushes.White,
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(6),
                        Padding = new Thickness(10),
                        Child = container
                    }
                };
            }

            if (CurrentTarget != inputTarget)
            {
                _popup.IsOpen = false;
                CurrentTarget = inputTarget;

                var element = GetUIElement(inputTarget);
                _popup.PlacementTarget = element;
                
                // 更新关闭按钮的可见性（每次显示时都检查）
                if (_closeButton != null)
                {
                    _closeButton.Visibility = KeyboardManager.ShowCloseButton 
                        ? Visibility.Visible 
                        : Visibility.Collapsed;
                }
                
                _popup.IsOpen = true;

                (element as Control)?.Focus();
            }
            else
            {
                // 即使目标相同，也更新关闭按钮的可见性（属性可能已更改）
                if (_closeButton != null)
                {
                    _closeButton.Visibility = KeyboardManager.ShowCloseButton 
                        ? Visibility.Visible 
                        : Visibility.Collapsed;
                }
            }
        }

        private static void OnWindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_popup?.IsOpen != true || _popup.Child == null)
                return;

            if (e.OriginalSource is DependencyObject clickedElement &&
                !IsDescendantOf(clickedElement, _popup.Child))
            {
                _popup.IsOpen = false;
                CurrentTarget = null!;
            }
        }

        private static void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (_popup?.IsOpen == true && e.Key == Key.Escape)
            {
                CloseKeyboard();
            }
        }

        /// <summary>
        /// 关闭虚拟键盘
        /// </summary>
        internal static void CloseKeyboard()
        {
            if (_popup != null)
            {
                _popup.IsOpen = false;
                CurrentTarget = null!;
            }
        }

        private static bool IsDescendantOf(DependencyObject source, DependencyObject target)
        {
            while (source != null)
            {
                if (source == target)
                    return true;
                source = VisualTreeHelper.GetParent(source);
            }
            return false;
        }

        private static UIElement? GetUIElement(IKeyboardInputTarget target)
        {
            return target switch
            {
                TextBoxInputTarget t => t.Target,
                PasswordBoxInputTarget p => p.Target,
                _ => null
            };
        }

        /// <summary>
        /// 检查指定控件所在的窗口/视图是否启用了虚拟键盘
        /// 优先级：View > Window > GlobalDefault
        /// </summary>
        private static bool IsKeyboardEnabledForWindow(DependencyObject element)
        {
            // 1. 先向上查找 UserControl 或 Page（视图级别）
            var view = FindAncestor<FrameworkElement>(element, 
                e => e is System.Windows.Controls.UserControl || e is System.Windows.Controls.Page);
            if (view != null)
            {
                var viewValue = KeyboardManager.IsKeyboardEnabledForView(view);
                if (viewValue.HasValue)
                {
                    // 视图明确设置了值，使用视图的值
                    return viewValue.Value;
                }
            }

            // 2. 向上查找窗口（窗口级别）
            var window = Window.GetWindow(element);
            if (window != null)
            {
                var windowValue = KeyboardManager.GetEnableKeyboardForWindow(window);
                if (windowValue.HasValue)
                {
                    // 窗口明确设置了值，使用窗口的值
                    return windowValue.Value;
                }
                // 窗口没有设置值，使用全局默认值
                return KeyboardManager.GlobalDefaultEnabled;
            }

            // 3. 如果找不到窗口（可能是在设计时或特殊场景），使用全局默认值
            return KeyboardManager.GlobalDefaultEnabled;
        }

        /// <summary>
        /// 向上查找指定类型的祖先元素
        /// </summary>
        private static T? FindAncestor<T>(DependencyObject element, Func<T, bool>? predicate = null) where T : DependencyObject
        {
            var current = VisualTreeHelper.GetParent(element);
            while (current != null)
            {
                if (current is T t)
                {
                    if (predicate == null || predicate(t))
                    {
                        return t;
                    }
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
