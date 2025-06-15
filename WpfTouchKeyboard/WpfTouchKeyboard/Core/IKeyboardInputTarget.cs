namespace WpfTouchKeyboard.Core
{
    public interface IKeyboardInputTarget
    {
        void InsertText(string text);
        void Backspace();
        string GetCurrentText();
    }
}
