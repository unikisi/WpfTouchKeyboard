using System.Windows.Controls;

namespace WpfTouchKeyboard.Core
{
    internal class PasswordBoxInputTarget(PasswordBox passwordBox) : IKeyboardInputTarget
    {
        public PasswordBox Target { get; } = passwordBox;

        public void InsertText(string text)
        {
            var current = Target.Password ?? string.Empty;
            var caretIndex = current.Length;
            var updated = current.Insert(caretIndex, text);
            Target.Password = updated;
        }

        public void Backspace()
        {
            var current = Target.Password ?? string.Empty;
            if (current.Length > 0)
            {
                Target.Password = current.Substring(0, current.Length - 1);
            }
        }

        public string GetCurrentText()
        {
            return Target.Password;
        }
    }
}
