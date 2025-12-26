using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using EpinelPSLauncher.Utils;
using FluentAvalonia.UI.Controls;
using System;
using System.IO;
using System.Net;
using System.Reflection;

namespace EpinelPSLauncher.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ShellView.Instance.Frame.GoBack();
    }

    private void ChkCompat_Checked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Configuration.Instance.DisableAC = ChkCompat.IsChecked == true;
        Configuration.Save();
    }

    private void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SettingsExpanderVersion.Description = $"v{Assembly.GetExecutingAssembly().GetName().Version}";
        ChkCompat.IsChecked = Configuration.Instance.DisableAC;
        UpdateResourceLocationText();
    }

    private void UpdateResourceLocationText()
    {
        SettingsExpanderResource.Description = Configuration.Instance.GetFullResourcePath();
    }

    private async void ButtonResourceChange_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var result = await MainWindow.Instance.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "Select folder where to place the com_proximabeta_NIKKE directory",
            SuggestedFileName = "Unity",
            SuggestedStartLocation = await MainWindow.Instance.StorageProvider.TryGetFolderFromPathAsync(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow")))
        });

        var oldPath = Configuration.Instance.GetFullResourcePath();
        if (result.Count > 0)
        {
            Configuration.Instance.GameResourcePath = result[0].TryGetLocalPath() ?? null;
            Configuration.Save();

            UpdateResourceLocationText();


            ContentDialog dlg = new()
            {
                Title = "Notice",
                Content = $"The contents of the com_proximabeta_NIKKE directory should be moved manually from {oldPath} to {Configuration.Instance.GameResourcePath}",
                DefaultButton = ContentDialogButton.Primary,
                PrimaryButtonText = "OK"
            };
            await dlg.ShowAsync();
        }
    }
}