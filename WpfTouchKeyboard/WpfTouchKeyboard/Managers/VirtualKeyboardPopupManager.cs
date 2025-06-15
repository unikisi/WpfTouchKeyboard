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
    public class VirtualKeyboardPopupManager
    {
        public static IKeyboardInputTarget CurrentTarget { get; set; }
        private static Popup _popup = null!;

        public static void Register()
        {
            EventManager.RegisterClassHandler(typeof(TextBox),
                UIElement.PreviewMouseDownEvent,
                new MouseButtonEventHandler((sender, e) =>
                {
                    if (sender is TextBox tb)
                        AttachTo(new TextBoxInputTarget(tb));
                }));

            EventManager.RegisterClassHandler(typeof(PasswordBox),
                UIElement.PreviewMouseDownEvent,
                new MouseButtonEventHandler((sender, e) =>
                {
                    if (sender is PasswordBox pb)
                        AttachTo(new PasswordBoxInputTarget(pb));
                }));

            EventManager.RegisterClassHandler(typeof(Window),
                UIElement.PreviewMouseDownEvent,
                new MouseButtonEventHandler(OnWindowMouseDown));

            EventManager.RegisterClassHandler(typeof(Window),
                UIElement.PreviewKeyDownEvent,
                new KeyEventHandler(OnWindowKeyDown));
        }

        private static void OnWindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_popup?.IsOpen != true || _popup.Child == null)
                return;

            if (e.OriginalSource is DependencyObject clickedElement)
            {
                // 判断点击是否在 popup 内部
                bool isInsidePopup = IsDescendantOf(clickedElement, _popup.Child);
                if (!isInsidePopup)
                {
                    _popup.IsOpen = false;
                    CurrentTarget = null!;
                }
            }
        }

        private static void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (_popup.IsOpen && e.Key == Key.Escape)
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

        public static void AttachTo(IKeyboardInputTarget inputTarget)
        {
            if (!AppSettings.IsEnabled)
                return;

            if (_popup?.IsOpen == true && CurrentTarget == inputTarget)
                return;


            if (_popup == null)
            {
                FrameworkElement keyboardContent = AppSettings.UseFullKeyboard switch
                {
                    KeyboardType.All => new FullKeyboardView { },
                    KeyboardType.Number => new NumericKeyboardView { },
                    _ => throw new ArgumentOutOfRangeException()
                };

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
                        Child = keyboardContent
                    }
                };
            }

            if (CurrentTarget != inputTarget)
            {
                _popup.IsOpen = false;
                CurrentTarget = inputTarget;

                var element = GetUIElement(inputTarget);
                _popup.PlacementTarget = element;
                _popup.IsOpen = true;

                // 重新获取焦点
                (element as Control)?.Focus();
            }
        }

        private static UIElement GetUIElement(IKeyboardInputTarget target)
        {
            return target switch
            {
                TextBoxInputTarget t => t.Target,
                PasswordBoxInputTarget p => p.Target,
                _ => null
            };
        }
    }
}
