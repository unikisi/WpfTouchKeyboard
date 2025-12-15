using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfTouchKeyboard.Core;
using WpfTouchKeyboard.Managers;

namespace WpfTouchKeyboard.Keyboards
{
    /// <summary>
    /// FullKeyboardView.xaml 的交互逻辑
    /// </summary>
    public partial class FullKeyboardView : UserControl
    {
        private bool _isShifted;
        private bool _isCapsLocked;
        private readonly Dictionary<string, string> _shiftSymbols = new()
        {
            {"1", "!"}, {"2", "@"}, {"3", "#"}, {"4", "$"}, {"5", "%"},
            {"6", "^"}, {"7", "&"}, {"8", "*"}, {"9", "("}, {"0", ")"},
            {"-", "_"}, {"+", "="}
        };

        public FullKeyboardView()
        {
            InitializeComponent();
        }

        private void Key_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: string key })
                return;

            if (key == "Shift")
            {
                _isShifted = !_isShifted;
                UpdateKeyVisuals();
                return;
            }

            if (key == "CapsLock")
            {
                _isCapsLocked = !_isCapsLocked;
                UpdateKeyVisuals();
                return;
            }

            if (InternalKeyboardManager.CurrentTarget is not { } inputTarget)
                return;

            if (key == "Back")
            {
                inputTarget.Backspace();
            }
            else if (key == "Enter")
            {
                inputTarget.Enter();
            }
            else
            {
                var valueToInsert = key;

                if (_shiftSymbols.TryGetValue(key, out var symbol) && _isShifted)
                {
                    valueToInsert = symbol;
                }
                else if (char.IsLetter(key[0]))
                {
                    valueToInsert = (_isCapsLocked ^ _isShifted) ? key.ToUpper() : key.ToLower();
                }

                inputTarget.InsertText(valueToInsert);

                if (_isShifted)
                {
                    _isShifted = false;
                    UpdateKeyVisuals();
                }
            }
        }

        private void UpdateKeyVisuals()
        {
            foreach (var btn in FindVisualChildren<Button>(this))
            {
                if (btn.Tag is not string key) continue;

                if (key == "Shift" || key == "CapsLock" || key == "Back" || key == " " || key == "Enter")
                    continue;

                if (char.IsLetter(key[0]))
                    btn.Content = (_isCapsLocked ^ _isShifted) ? key.ToUpper() : key.ToLower();
                else if (_shiftSymbols.TryGetValue(key, out var symbol))
                    btn.Content = _isShifted ? symbol : key;
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T t)
                    yield return t;

                foreach (var childOfChild in FindVisualChildren<T>(child))
                    yield return childOfChild;
            }
        }
    }
}
