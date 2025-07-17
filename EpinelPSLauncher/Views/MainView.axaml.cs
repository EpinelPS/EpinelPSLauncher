using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using EpinelPSLauncher.Models;

namespace EpinelPSLauncher.Views;

public partial class MainView : UserControl
{
    public static MainView Instance { get; private set; } = null!;
    public MainView()
    {
        InitializeComponent();

        Instance = this;
        Frame.Navigate(typeof(LoginView));
    }
}