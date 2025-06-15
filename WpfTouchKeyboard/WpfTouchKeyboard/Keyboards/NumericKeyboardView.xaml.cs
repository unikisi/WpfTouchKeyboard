using System.Windows.Controls;
using System.Windows.Input;
using WpfTouchKeyboard.Core;
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
            if (VirtualKeyboardPopupManager.CurrentTarget is not { } target)
                return;

            switch (key)
            {
                case "Back":
                    target.Backspace();
                    break;

                case "Space":
                    target.InsertText(" ");
                    break;

                default:
                    target.InsertText(key);
                    break;
            }
        }

        private static void OnKeyPressed(string key)
        {
            SendKey(key);
        }
    }
}
