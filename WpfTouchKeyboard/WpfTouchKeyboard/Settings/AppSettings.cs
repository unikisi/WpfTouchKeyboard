namespace WpfTouchKeyboard.Settings
{
    public class AppSettings
    {
        public static KeyboardType UseFullKeyboard { get; set; } = KeyboardType.Number;

        public static bool IsEnabled { get; set; } = true;
    }

    public enum KeyboardType
    {
        Number,
        All
    }
}
