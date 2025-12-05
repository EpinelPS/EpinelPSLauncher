using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EpinelPSLauncher.Models;

namespace EpinelPSLauncher.Views;

public partial class ShellView : UserControl
{
    public static ShellView Instance { get; private set; } = null!;
    public ShellView()
    {
        InitializeComponent();

        Instance = this;
        Frame.Navigate(typeof(LoginView));
    }
}