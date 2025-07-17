using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using EpinelPSLauncher.Utils;
using FluentAvalonia.UI.Media.Animation;

namespace EpinelPSLauncher.Views;

public partial class MainWindow : Window
{
    public static MainWindow Instance { get; private set; } = null!;
    public MainWindow()
    {
        InitializeComponent();
        Instance = this;
    }

    private void WindowPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        if (AccountButton.Flyout == null) return;

        AccountButton.Flyout.Hide();
        
        // TODO issue logout request
        SessionData.AuthResponse = null!;
        SessionData.CurrentAccount = null!;
        SessionData.UserProfileResponse = null!;
        SessionData.UserProfileResponseAccount = null!;

        MainView.Instance.Frame.Navigate(typeof(LoginView), new(), new DrillInNavigationTransitionInfo());
        MainView.Instance.Frame.BackStack.Clear();
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        MainView.Instance.Frame.Navigate(typeof(LoginView));
    }
}
