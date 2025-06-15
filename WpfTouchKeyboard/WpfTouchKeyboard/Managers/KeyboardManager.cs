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
    }
}
