using System.Windows;
using System.Windows.Controls;
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
        AppSettings.IsEnabled = !AppSettings.IsEnabled;

        if (sender is Button btn)
        {
            btn.Content = AppSettings.IsEnabled ? "关闭虚拟键盘" : "开启虚拟键盘";
        }
    }
}