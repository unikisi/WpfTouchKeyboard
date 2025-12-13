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
    }
}
