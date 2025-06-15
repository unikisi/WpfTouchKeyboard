namespace WpfTouchKeyboard.Settings
{
    public class AppSettings
    {
        public static KeyboardType KeyboardType { get; set; } = KeyboardType.All;

        public static bool IsEnabled { get; set; } = true;
    }

    public enum KeyboardType
    {
        Number,
        All
    }
}
