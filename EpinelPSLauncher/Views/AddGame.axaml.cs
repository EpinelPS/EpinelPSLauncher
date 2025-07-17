using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform.Storage;

namespace EpinelPSLauncher.Views;

public partial class AddGame : UserControl
{
    public bool CanMoveNext { get; set; }
    public AddGame()
    {
        InitializeComponent();
    }

    private async void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        if (topLevel != null)
        {
            // Start async operation to open the dialog.
            var files = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select game path, with launcher and game folder",
                AllowMultiple = false
            });

            if (files.Count >= 1)
            {
                TextPath.Text = files[0].TryGetLocalPath();
                ValidatePath();
            }
        }
    }

    private void ValidatePath()
    {
        if (selectDownload.IsChecked == true)
        {
            // validate if the folder has game exe
            if (!string.IsNullOrEmpty(TextPath.Text))
            {
                if (!File.Exists(Path.Combine(TextPath.Text, "NIKKE", "game", "nikke.exe")))
                {
                    Error.Text = "Path is invalid. Make sure that nikke.exe exists in the game folder";
                    CanMoveNext = false;
                    return;
                }
            }
        }

        if (string.IsNullOrEmpty(TextPath.Text))
        {
            Error.Text = "Path cannot be empty";
            CanMoveNext = false;
            return;
        }
        if (!Directory.Exists(TextPath.Text))
        {
            Error.Text = "Path does not exist";
            CanMoveNext = false;
            return;
        }

        Error.Text = "";

        CanMoveNext = true;
    }

    private void TextBox_TextChanged(object? sender, RoutedEventArgs e)
    {
        ValidatePath();
    }

    private void RadioButton_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        ValidatePath();
    }
}