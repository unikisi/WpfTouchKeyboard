namespace WpfTouchKeyboard.Core
{
    internal interface IKeyboardInputTarget
    {
        void InsertText(string text);
        void Backspace();
        void Enter();
        string GetCurrentText();
    }
}
