using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Controls;
using Avalonia.Threading;
using EpinelPSLauncher.Clients;
using EpinelPSLauncher.Models;
using EpinelPSLauncher.Utils;
using FluentAvalonia.UI.Controls;
using ServerSelector;

namespace EpinelPSLauncher.Views;

public partial class LoginView : UserControl
{
    private readonly ObservableCollection<AccountEntry> Accounts = [];
    public LoginView()
    {
        InitializeComponent();

        AccountList.ItemsSource = Accounts;

        LoadAccounts();
    }

    private void ListBox_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        e.Handled = true;
    }

    private async void ListBox_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (AccountList.SelectedItem == null) return;

        var item = (AccountEntry)AccountList.SelectedItem;
        LoginSubpage.IsVisible = false;
        LoadingUI.IsVisible = true;

        LoadingText.Text = "Authenticating";

        SessionData.CurrentAccount = item;

        await LevelInfiniteClient.Instance.ConfigureAsync(item.ServerIP == null, item.ServerIP ?? "");
        await AccountControllerClient.Instance.ConfigureAsync(item.ServerIP == null, item.ServerIP ?? "");

        var rsp = await LevelInfiniteClient.Instance.AuthenticateAsync(item);
        if (rsp.IsSuccess && rsp.ResponseData != null)
        {
            SessionData.AuthResponse = rsp.ResponseData;
        }
        else
        {
            LoginSubpage.IsVisible = true;
            LoadingUI.IsVisible = false;
            await new ContentDialog()
            {
                Title = "Failed to authenticate",
                Content = $"Error from server: {rsp}",
                PrimaryButtonText = "OK",
                DefaultButton = ContentDialogButton.Primary
            }.ShowAsync();
            return;
        }

        LoadingText.Text = "Loading profile data (1/3)";

        var rsp2 = await LevelInfiniteClient.Instance.GetMinorcerStatus(item);
        if (!rsp2.IsSuccess || rsp2.ResponseData == null)
        {
            LoginSubpage.IsVisible = true;
            LoadingUI.IsVisible = false;
            await new ContentDialog()
            {
                Title = "Failed to load profile info (part 1)",
                Content = $"Error from server: {rsp2}",
                PrimaryButtonText = "OK",
                DefaultButton = ContentDialogButton.Primary
            }.ShowAsync();
            return;
        }

        LoadingText.Text = "Loading profile data (2/3)";
        var rsp3 = await LevelInfiniteClient.Instance.GetUserInfo();
        if (!rsp3.IsSuccess || rsp3.ResponseData == null)
        {
            LoginSubpage.IsVisible = true;
            LoadingUI.IsVisible = false;
            await new ContentDialog()
            {
                Title = "Failed to load profile info (part 2)",
                Content = $"Error from server: {rsp3}",
                PrimaryButtonText = "OK",
                DefaultButton = ContentDialogButton.Primary
            }.ShowAsync();
            return;
        }

        SessionData.UserProfileResponse = rsp3.ResponseData;
        LoadingText.Text = "Loading profile data (3/3)";
        var rsp4 = await AccountControllerClient.Instance.GetUserInfo(item);
        if (!rsp4.IsSuccess || rsp4.ResponseData == null)
        {
            LoginSubpage.IsVisible = true;
            LoadingUI.IsVisible = false;
            await new ContentDialog()
            {
                Title = "Failed to load profile info (part 3)",
                Content = $"Error from server: {rsp3}",
                PrimaryButtonText = "OK",
                DefaultButton = ContentDialogButton.Primary
            }.ShowAsync();
            return;
        }
        SessionData.UserProfileResponseAccount = rsp4.ResponseData;


        LoadingText.Text = "Configuring game...";

        try
        {

            (bool, string?) pathResult = ServerSwitcher.SetBasePath(Configuration.Instance.GamePath ?? "");
            if (!pathResult.Item1)
            {
                await new ContentDialog()
                {
                    Title = "Server Selector",
                    Content = pathResult.Item2,
                    PrimaryButtonText = "OK",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync();
            }

            var result = await ServerSwitcher.SaveCfg(item.ServerIP == null, item.ServerIP ??
        "", false);

            if (!result.IsSupported)
            {
                await new ContentDialog()
                {
                    Title = "Server Selector",
                    Content = $"Game version might not be supported",
                    PrimaryButtonText = "OK",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync();
            }
            if (!result.Ok)
            {
                await new ContentDialog()
                {
                    Title = "Server Selector",
                    Content = $"Failed to switch to server:\n{result.Exception}",
                    PrimaryButtonText = "OK",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync();
            }
        }
        catch (Exception ex)
        {
            await new ContentDialog()
            {
                Title = "Server Selector",
                Content = $"Failed to switch to server:\n{ex}",
                PrimaryButtonText = "OK",
                DefaultButton = ContentDialogButton.Primary
            }.ShowAsync();
        }

        ShellView.Instance.Frame.Navigate(typeof(LoggedInView));
        ShellView.Instance.Frame.BackStack.Clear();

        LoginSubpage.IsVisible = true;
        LoadingUI.IsVisible = false;
    }

    private void LoadAccounts()
    {
        Accounts.Clear();

        foreach (var item in Configuration.Instance.Accounts)
        {
            Accounts.Add(item);
        }

        if (Accounts.Count == 0) SelAcct.IsVisible = false;
        else SelAcct.IsVisible = true;
    }

    private async void AddAccount_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        AddAccount content = new();
        ContentDialog dlg = new()
        {
            Title = "Add new account",
            Content = content,
            PrimaryButtonText = "Add",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        };

        dlg.PrimaryButtonClick += AddAccountDlg_PrimaryButtonClick;

        if (await dlg.ShowAsync() == ContentDialogResult.Primary)
        {
            LoadAccounts();
        }
    }

    private async Task AddAccountAsync(bool official, string ip, string user, string pass)
    {
        LoginSubpage.IsVisible = false;
        LoadingUI.IsVisible = true;

        await AccountControllerClient.Instance.ConfigureAsync(official, ip);

        var result = await AccountControllerClient.Instance.LoginAsync(user, pass);

        if (result.IsSuccess && result.ResponseData != null)
        {
            Configuration.Instance.Accounts.Add(new(official ? null : ip, user, result.ResponseData.token, result.ResponseData.uid, result.ResponseData.expire));
            Configuration.Save();
            LoadAccounts();
        }
        else
        {
            LoginSubpage.IsVisible = true;
            LoadingUI.IsVisible = false;

            if (result.Ex != null)
                throw result.Ex;
            else
                throw new Exception(result.Message);
        }

        LoginSubpage.IsVisible = true;
        LoadingUI.IsVisible = false;

    }
    private async void AddAccountDlg_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        AddAccount? content = (AddAccount?)sender.Content;
        if (content == null) return;

        args.Cancel = true;

        if (!content.Validate()) return;

        try
        {
            await AddAccountAsync(content.UseOfficialServer, content.ServerIP, content.Email, content.Password);
        }
        catch (Exception ex)
        {
            content.ErrorText.Text = ex.Message;
            return;
        }

        sender.Hide();
        args.Cancel = false;
    }

    private async void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(Configuration.Instance.GamePath))
        {
            ContentDialog dlg = new()
            {
                Title = "Select game path",
                Content = new AddGame(),
                PrimaryButtonText = "OK",
                DefaultButton = ContentDialogButton.Primary
            };

            dlg.PrimaryButtonClick += AddGameDlg_PrimaryButtonClick;

            await dlg.ShowAsync();
        }
    }

    private async void AddGameDlg_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var content = (AddGame?)sender.Content;
        if (content == null) return;

        if (content.CanMoveNext)
        {
            if (content.radioDownload.IsChecked == true)
            {
                content.Page1.IsVisible = false;
                content.InstallPage.IsVisible = true;

                args.Cancel = true;
                sender.IsPrimaryButtonEnabled = false;
                sender.Title = "Downloading (0%)";
                content.InstallProgress.IsIndeterminate = true;

                GameDownloader.Instance.DownloadPath = content.TextPath.Text ?? "C:\\NIKKE\\";


                try
                {
                    await GameDownloader.Instance.FetchVersionInfoAsync();
                    await GameDownloader.Instance.FetchManifestAsync();
                }
                catch (Exception ex)
                {
                    sender.IsPrimaryButtonEnabled = true;
                    content.Error2.Text = ex.ToString();
                    return;
                }
                content.InstallProgress.IsIndeterminate = false;
                Timer tm = new()
                {
                    Interval = 500
                };
                tm.Elapsed += delegate (object? sender2, ElapsedEventArgs e)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        var percent = ((double)GameDownloader.Instance.BytesDownloaded / GameDownloader.Instance.BytesTotal) * 100;
                        content.InstallProgress.Value = percent;
                        sender.Title = $"Downloading ({percent:00}%)";
                    });
                };
                tm.Start();

                await GameDownloader.Instance.StartDownloadAsync();

                Configuration.Instance.GamePath = content.TextPath.Text ?? "C:\\NIKKE\\";
                Configuration.Save();

                args.Cancel = false;
                sender.Hide();
            }
            else
            {
                Configuration.Instance.GamePath = content.TextPath.Text ?? "C:\\NIKKE\\";
                Configuration.Save();
            }
        }
        else
        {
            args.Cancel = true;
        }
    }
}
