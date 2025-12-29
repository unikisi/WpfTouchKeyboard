using System.Windows;
using WpfTouchKeyboard.Managers;

namespace WpfTouchKeyboard.Test;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        KeyboardManager.Register();
        KeyboardManager.GlobalDefaultEnabled = true;
        KeyboardManager.ShowCloseButton = true;
    }
}

