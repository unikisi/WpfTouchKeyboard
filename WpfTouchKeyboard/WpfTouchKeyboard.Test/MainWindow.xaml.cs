using System.Windows;
using System.Windows.Controls;
using WpfTouchKeyboard.Managers;
using WpfTouchKeyboard.Settings;

namespace WpfTouchKeyboard.Test;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void ToggleKeyboard_Click(object sender, RoutedEventArgs e)
    {
        KeyboardManager.IsEnabled = !KeyboardManager.IsEnabled;

        if (sender is Button btn)
        {
            btn.Content = KeyboardManager.IsEnabled ? "关闭虚拟键盘" : "开启虚拟键盘";
        }
    }

    private void KeyboardTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (KeyboardTypeComboBox.SelectedItem is ComboBoxItem selectedItem)
        {
            string keyboardType = selectedItem.Tag?.ToString();
            switch (keyboardType)
            {
                case "All":
                    KeyboardManager.SetKeyboardType(KeyboardType.All);
                    break;
                case "Number":
                    KeyboardManager.SetKeyboardType(KeyboardType.Number);
                    break;
            }

            MessageBox.Show($"已切换为：{keyboardType} 键盘");
        }
    }
}