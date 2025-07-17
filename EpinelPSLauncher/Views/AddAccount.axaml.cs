using System.Net;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EpinelPSLauncher.Views;

public partial class AddAccount : UserControl
{
    public bool UseOfficialServer
    {
        get
        {
            return cmbServer.SelectedIndex == 0;
        }
        set
        {
            cmbServer.SelectedIndex = value ? 0 : 1;
        }
    }
    public string ServerIP
    {
        get
        {
            return txtServerIp.Text ?? "";
        }
        set
        {
            txtServerIp.Text = value;
        }
    }
    public string Email
    {
        get
        {
            return txtUser.Text ?? "";
        }
        set
        {
            txtUser.Text = value;
        }
    }
    public string Password
    {
        get
        {
            return txtPass.Text ?? "";
        }
        set
        {
            txtPass.Text = value;
        }
    }
    public AddAccount()
    {
        InitializeComponent();
    }

    private void ComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        if (cmbServer == null) return;

        txtServerIp.IsEnabled = cmbServer.SelectedIndex == 1;
    }
    /// <summary>
    /// https://stackoverflow.com/a/1374644
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    private static bool IsValidEmail(string email)
    {
        var trimmedEmail = email.Trim();

        if (trimmedEmail.EndsWith('.'))
        {
            return false; // suggested by @TK-421
        }
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == trimmedEmail;
        }
        catch
        {
            return false;
        }
    }
    public bool Validate()
    {
        if (cmbServer.SelectedIndex == 1)
        {
            if (string.IsNullOrEmpty(txtServerIp.Text))
            {
                ErrorText.Text = "Server IP is missing";
                return false;
            }

            if (!IPAddress.TryParse(txtServerIp.Text, out _))
            {
                ErrorText.Text = "Server IP is invalid";
                return false;
            }
        }

        if (string.IsNullOrEmpty(txtUser.Text))
        {
            ErrorText.Text = "Email is required";
            return false;
        }
        if (!IsValidEmail(txtUser.Text))
        {
            ErrorText.Text = "Invalid email address";
            return false;
        }
        if (string.IsNullOrEmpty(txtPass.Text))
        {
            ErrorText.Text = "Password is required";
            return false;
        }

        ErrorText.Text = "";

        return true;
    }
}