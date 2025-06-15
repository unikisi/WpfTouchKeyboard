using System.Windows.Controls;
using System.Windows.Input;
using WpfTouchKeyboard.Managers;

namespace WpfTouchKeyboard.Keyboards
{
    /// <summary>
    /// NumericKeyboardView.xaml 的交互逻辑
    /// </summary>
    public partial class NumericKeyboardView : UserControl
    {
        public ICommand KeyPressCommand { get; }

        public NumericKeyboardView()
        {
            InitializeComponent();

            KeyPressCommand = new RelayCommand<string>(OnKeyPressed);
        }

        public static void SendKey(string key)
        {
            var tb = VirtualKeyboardPopupManager.CurrentTarget;

            if (!tb.IsFocused)
                return;

            var caretIndex = tb.CaretIndex;
            var text = tb.Text;

            switch (key)
            {
                case "Back":
                    if (caretIndex > 0)
                    {
                        tb.Text = text.Remove(caretIndex - 1, 1);
                        tb.CaretIndex = caretIndex - 1;
                    }
                    break;
                case "Space":
                    tb.Text = text.Insert(caretIndex, " ");
                    tb.CaretIndex = caretIndex + 1;
                    break;

                default:
                    tb.Text = text.Insert(caretIndex, key);
                    tb.CaretIndex = caretIndex + key.Length;
                    break;
            }
        }

        private static void OnKeyPressed(string key)
        {
            SendKey(key);
        }
    }
}
