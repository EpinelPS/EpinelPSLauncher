using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using EpinelPSLauncher.Clients;
using EpinelPSLauncher.Models;
using EpinelPSLauncher.Utils;
using FluentAvalonia.UI.Controls;
using ServerSelector;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Timers;

namespace EpinelPSLauncher.Views;

public partial class LoggedInView : UserControl
{
    private Process? proc;
    public LoggedInView()
    {
        InitializeComponent();
    }

    private async void Play_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        AuthData data = new()
        {
            extra_json = "{}",
            openid = SessionData.AuthResponse.openid,
            token_expire_time = SessionData.AuthResponse.token_expire_time,
            first_login = SessionData.AuthResponse.first_login,
            bind_list = "", // TODO
            birthday = SessionData.AuthResponse.birthday,
            channelid = 131,
            need_name_auth = SessionData.AuthResponse.need_name_auth,
            token = SessionData.AuthResponse.token,
            channel_info = JsonSerializer.Serialize(SessionData.AuthResponse.channel_info, SourceGenerationContext.Default.ChannelInfo),
            link_li_uid = "",
            gender = SessionData.AuthResponse.gender,
            user_name = SessionData.AuthResponse.user_name,
            picture_url = SessionData.AuthResponse.picture_url,
            email = SessionData.AuthResponse.email,
            channel = "LevelInfinite",
            pf_key = SessionData.AuthResponse.pf_key,
            pf = SessionData.AuthResponse.pf,
            del_account_info = JsonEncodedText.Encode(SessionData.AuthResponse.del_account_info, System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping).Value,
            del_account_status = SessionData.AuthResponse.del_account_status,
            // todo
            confirm_code_expire_time = 0,
            confirm_code = "",
            reg_channel_dis = "Windows",
            link_li_token = "",
            user_status = -1,
            del_li_account_status = -1,
            legal_doc = "",
            method_id = 0,
            msg = "",
            oauth_code = "",
            ret = 0,
            ret_code = 0,
            ret_msg = "",
            transfer_code = "",
            transfer_code_expire_time = 0
        };

        if (string.IsNullOrEmpty(Configuration.Instance.GameResourcePath))
        {
            Configuration.Instance.GameResourcePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow"), "Unity");
            Configuration.Save();
        }

        string executablePath = Path.Combine(Configuration.Instance.GamePath, @"NIKKE/game/nikke.exe");
        if (!File.Exists(executablePath))
        {
            await new ContentDialog()
            {
                Title = "Failed to start",
                Content = $"File does not exist: {executablePath}",
                PrimaryButtonText = "OK",
                DefaultButton = ContentDialogButton.Primary
            }.ShowAsync();
            return;
        }

        if (!OperatingSystem.IsWindows() || Configuration.Instance.DisableAC)
        {
            string newGameExectutable = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UnityInit.dll");
            string newGameDll = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "HelperDll.dll");
            if (!File.Exists(newGameExectutable) || !File.Exists(newGameDll))
            {
                await new ContentDialog()
                {
                    Title = "Warning",
                    Content = $"Files required for Linux/MacOS support are missing. Press OK to run the game anyways.",
                    PrimaryButtonText = "OK",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync();
            }
            else
            {
                try
                {
                    File.Copy(newGameExectutable, Path.Combine(Configuration.Instance.GamePath, @"NIKKE/game/nikkeBase.dll"), true);
                    File.Copy(newGameDll, Path.Combine(Configuration.Instance.GamePath + @"/NIKKE/game/" + Encoding.UTF8.GetString(Convert.FromBase64String("QW50aUNoZWF0RXhwZXJ0L0FDRS1CYXNlNjQuZGxs"))), true);
                }
                catch (Exception ex)
                {
                    await new ContentDialog()
                    {
                        Title = "Error",
                        Content = $"{ex.Message}",
                        PrimaryButtonText = "OK",
                        DefaultButton = ContentDialogButton.Primary
                    }.ShowAsync();
                }
            }
        }

        PlayButton.Content = @"Initializing";
        PlayButton.IsEnabled = false;

        try
        {
            proc = await LaunchGame.Launch(executablePath, Configuration.Instance.GameResourcePath + @"/com_proximabeta_NIKKE/", JsonSerializer.Serialize(data, SourceGenerationContext.Default.AuthData));
        }
        catch (Exception ex)
        {
            PlayButton.Content = @"Play";
            PlayButton.IsEnabled = true;
            await new ContentDialog()
            {
                Title = "Failed to start",
                Content = $"{ex.Message}",
                PrimaryButtonText = "OK",
                DefaultButton = ContentDialogButton.Primary
            }.ShowAsync();
        }

        if (proc != null && !proc.HasExited)
        {
            proc.EnableRaisingEvents = true;
            proc.Exited += Proc_Exited;
            PlayButton.Content = @"Running...";
            PlayButton.IsEnabled = false;
        }
    }
    private void Proc_Exited(object? sender, System.EventArgs e)
    {
        Dispatcher.UIThread.Invoke(async () =>
        {
            MainWindow.Instance.Activate();
            if (proc != null && proc.ExitCode != 0)
            {
                await new ContentDialog()
                {
                    Title = "Game exited",
                    Content = $"Exit code: {proc.ExitCode}",
                    PrimaryButtonText = "OK",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync();
            }

            PlayButton.Content = @"Launch";
            PlayButton.IsEnabled = true;
        });
    }
    private async void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (SessionData.UserProfileResponseAccount == null)
        {
            try
            {
                await new ContentDialog()
                {
                    Title = "Invalid session",
                    Content = $"Session data is invalid.",
                    PrimaryButtonText = "OK",
                    DefaultButton = ContentDialogButton.Primary
                }.ShowAsync();
                ShellView.Instance.Frame.Navigate(typeof(LoginView));
                return;
            }
            catch
            {
                // exception is thrown when in preview mode
                return;
            }
        }
        Username.Text = "Username: " + SessionData.UserProfileResponseAccount.user_name;
        UpdateVersionText();
    }

    private void UpdateVersionText()
    {
        GameVersion.Text = "Version: " + (string.IsNullOrEmpty(Configuration.Instance.GameVersion) ? "Unknown" : Configuration.Instance.GameVersion);
    }

    private async void UpdateOrRepair_Click(object? sender2, Avalonia.Interactivity.RoutedEventArgs e)
    {
        GameDownloader.Instance.DownloadPath = Configuration.Instance.GamePath;


        AddGame content = new();
        ContentDialog dlg = new() { Title = "Download/update" };

        content.Page1.IsVisible = false;
        content.InstallPage.IsVisible = true;

        dlg.IsPrimaryButtonEnabled = false;
        dlg.Title = "Downloading (0%)";
        dlg.PrimaryButtonText = "Close";
        content.InstallProgress.IsIndeterminate = true;

        GameDownloader.Instance.DownloadPath = Configuration.Instance.GamePath ?? "C:\\NIKKE\\";

        dlg.Loaded += async delegate (object? sender, RoutedEventArgs e)
        {


            try
            {
                await GameDownloader.Instance.FetchVersionInfoAsync();
                await GameDownloader.Instance.FetchManifestAsync();
            }
            catch (Exception ex)
            {
                dlg.IsPrimaryButtonEnabled = true;
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
                    var percent = (double)GameDownloader.Instance.BytesDownloaded / GameDownloader.Instance.BytesTotal * 100;
                    content.InstallProgress.Value = percent;
                    dlg.Title = $"Downloading ({percent:0}%)";
                });
            };

            tm.Start();
            await GameDownloader.Instance.StartDownloadAsync();
            tm.Stop();

            dlg.Title = $"Configuring...";
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

                var result = await ServerSwitcher.SaveCfg(SessionData.CurrentAccount.ServerIP == null, SessionData.CurrentAccount.ServerIP ?? "", false);

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

            UpdateVersionText();

            dlg.IsPrimaryButtonEnabled = true;
            dlg.Title = $"Update/repair complete";
        };

        await dlg.ShowAsync();
    }
}