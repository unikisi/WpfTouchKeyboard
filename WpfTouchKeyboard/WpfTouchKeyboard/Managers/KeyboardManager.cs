using System.Windows;
using WpfTouchKeyboard.Settings;

namespace WpfTouchKeyboard.Managers
{
    /// <summary>
    /// Public-facing API to control the virtual keyboard.
    /// </summary>
    public static class KeyboardManager
    {
        /// <summary>
        /// Gets or sets a value indicating whether the virtual keyboard is enabled.
        /// </summary>
        public static bool IsEnabled
        {
            get => InternalKeyboardManager.IsEnabled;
            set => InternalKeyboardManager.IsEnabled = value;
        }

        /// <summary>
        /// 获取或设置全局默认值：是否在所有窗口默认启用虚拟键盘。
        /// 默认值为 true（全局启用）。设置为 false 时，需要在特定窗口设置 EnableKeyboardForWindow="True" 才能启用。
        /// </summary>
        public static bool GlobalDefaultEnabled { get; set; } = true;

        /// <summary>
        /// Registers keyboard support. Should be called once during application startup.
        /// </summary>
        public static void Register()
        {
            InternalKeyboardManager.Register();
        }

        /// <summary>
        /// Sets the keyboard type (full or numeric).
        /// </summary>
        public static void SetKeyboardType(KeyboardType type)
        {
            InternalKeyboardManager.SetKeyboardType(type);
        }

        /// <summary>
        /// 附加属性：控制该控件是否启用虚拟键盘。
        /// 默认值为 true（启用）。设置为 false 时，该控件将不会显示虚拟键盘。
        /// </summary>
        public static readonly DependencyProperty EnableKeyboardProperty =
            DependencyProperty.RegisterAttached(
                "EnableKeyboard",
                typeof(bool),
                typeof(KeyboardManager),
                new PropertyMetadata(true));

        /// <summary>
        /// 获取指定控件的 EnableKeyboard 属性值。
        /// </summary>
        /// <param name="element">要获取属性的 UI 元素</param>
        /// <returns>如果为 true，则启用虚拟键盘；如果为 false，则禁用虚拟键盘</returns>
        public static bool GetEnableKeyboard(UIElement element)
        {
            return (bool)element.GetValue(EnableKeyboardProperty);
        }

        /// <summary>
        /// 设置指定控件的 EnableKeyboard 属性值。
        /// </summary>
        /// <param name="element">要设置属性的 UI 元素</param>
        /// <param name="value">如果为 true，则启用虚拟键盘；如果为 false，则禁用虚拟键盘</param>
        public static void SetEnableKeyboard(UIElement element, bool value)
        {
            element.SetValue(EnableKeyboardProperty, value);
        }

        /// <summary>
        /// 附加属性：控制该窗口是否启用虚拟键盘。
        /// 默认值为 null（使用全局默认值 GlobalDefaultEnabled）。
        /// 设置为 true 时，该窗口内的所有控件（除非单独设置为 false）将显示虚拟键盘。
        /// 设置为 false 时，该窗口禁用虚拟键盘（即使全局默认启用）。
        /// </summary>
        public static readonly DependencyProperty EnableKeyboardForWindowProperty =
            DependencyProperty.RegisterAttached(
                "EnableKeyboardForWindow",
                typeof(bool?),
                typeof(KeyboardManager),
                new PropertyMetadata(null));

        /// <summary>
        /// 获取指定窗口的 EnableKeyboardForWindow 属性值。
        /// </summary>
        /// <param name="element">要获取属性的窗口元素</param>
        /// <returns>如果为 true，则在该窗口启用虚拟键盘；如果为 false，则禁用虚拟键盘；如果为 null，则使用全局默认值</returns>
        public static bool? GetEnableKeyboardForWindow(Window element)
        {
            return (bool?)element.GetValue(EnableKeyboardForWindowProperty);
        }

        /// <summary>
        /// 设置指定窗口的 EnableKeyboardForWindow 属性值。
        /// </summary>
        /// <param name="element">要设置属性的窗口元素</param>
        /// <param name="value">如果为 true，则在该窗口启用虚拟键盘；如果为 false，则禁用虚拟键盘；如果为 null，则使用全局默认值</param>
        public static void SetEnableKeyboardForWindow(Window element, bool? value)
        {
            element.SetValue(EnableKeyboardForWindowProperty, value);
        }

        /// <summary>
        /// 附加属性：控制该视图（UserControl/Page）是否启用虚拟键盘。
        /// 默认值为 null（使用窗口或全局默认值）。
        /// 设置为 true 时，该视图内的所有控件（除非单独设置为 false）将显示虚拟键盘。
        /// 设置为 false 时，该视图禁用虚拟键盘（即使窗口或全局默认启用）。
        /// 优先级：View > Window > GlobalDefault
        /// </summary>
        public static readonly DependencyProperty EnableKeyboardForViewProperty =
            DependencyProperty.RegisterAttached(
                "EnableKeyboardForView",
                typeof(bool?),
                typeof(KeyboardManager),
                new PropertyMetadata(null));

        /// <summary>
        /// 获取指定视图的 EnableKeyboardForView 属性值。
        /// </summary>
        /// <param name="element">要获取属性的视图元素（UserControl 或 Page）</param>
        /// <returns>如果为 true，则在该视图启用虚拟键盘；如果为 false，则禁用虚拟键盘；如果为 null，则使用窗口或全局默认值</returns>
        public static bool? GetEnableKeyboardForView(FrameworkElement element)
        {
            return (bool?)element.GetValue(EnableKeyboardForViewProperty);
        }

        /// <summary>
        /// 设置指定视图的 EnableKeyboardForView 属性值。
        /// </summary>
        /// <param name="element">要设置属性的视图元素（UserControl 或 Page）</param>
        /// <param name="value">如果为 true，则在该视图启用虚拟键盘；如果为 false，则禁用虚拟键盘；如果为 null，则使用窗口或全局默认值</param>
        public static void SetEnableKeyboardForView(FrameworkElement element, bool? value)
        {
            element.SetValue(EnableKeyboardForViewProperty, value);
        }

        /// <summary>
        /// 检查指定窗口是否启用了虚拟键盘（考虑全局默认值）
        /// </summary>
        internal static bool IsKeyboardEnabledForWindow(Window window)
        {
            var windowValue = GetEnableKeyboardForWindow(window);
            // 如果窗口明确设置了值，使用窗口的值；否则使用全局默认值
            return windowValue ?? GlobalDefaultEnabled;
        }

        /// <summary>
        /// 检查指定视图是否启用了虚拟键盘（考虑窗口和全局默认值）
        /// </summary>
        internal static bool? IsKeyboardEnabledForView(FrameworkElement view)
        {
            return GetEnableKeyboardForView(view);
        }
    }
}
