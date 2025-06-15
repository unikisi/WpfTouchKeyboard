using System.Windows.Controls;

namespace WpfTouchKeyboard.Core
{
    public class TextBoxInputTarget(TextBox textBox) : IKeyboardInputTarget
    {
        public TextBox Target { get; } = textBox;

        public void InsertText(string text)
        {
            if (!Target.IsFocused)
                Target.Focus();

            var caretIndex = Target.CaretIndex;
            Target.Text = Target.Text.Insert(caretIndex, text);
            Target.CaretIndex = caretIndex + text.Length;
        }

        public void Backspace()
        {
            if (!Target.IsFocused)
                Target.Focus();

            var caretIndex = Target.CaretIndex;
            if (caretIndex > 0)
            {
                Target.Text = Target.Text.Remove(caretIndex - 1, 1);
                Target.CaretIndex = caretIndex - 1;
            }
        }

        public string GetCurrentText()
        {
            return Target.Text;
        }
    }
}
