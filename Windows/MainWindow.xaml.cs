using System.Text.RegularExpressions;
using System.Windows;

namespace KT_IMAPClient;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        EmailTextBox.Text = "EMAIL";

        PasswordTextBox.Password = "PASSWORD";
    }

    private void LogInButtonClicked(object sender, RoutedEventArgs e)
    {
        if(ValidateEmail(EmailTextBox.Text))
        {
            UserInfo userInfo = new UserInfo(EmailTextBox.Text, PasswordTextBox.Password);

            Client clientSession = new Client("mail.inbox.lt", 993, userInfo);

            if(clientSession.LogIn(userInfo))
            {
                OpenMailboxWindow(clientSession, userInfo);
            }
        }
        else
        {
            if (App.ErrorWindows != null)
            {
                for (int i = App.ErrorWindows.Count - 1; i >= 0; --i)
                {
                    App.ErrorWindows[i].Close();
                    App.ErrorWindows.Remove(App.ErrorWindows[i]);
                }
            }

            ErrorWindow errorWindow = new("Invalid email address!");

            if(App.ErrorWindows != null)
            {
                App.ErrorWindows.Add(errorWindow);
            }

            errorWindow.Show();
        }
    }

    private void OpenMailboxWindow(Client clientSession, UserInfo userInfo)
    {
        MailboxWindow mailbox = new MailboxWindow(clientSession, userInfo);

        mailbox.Show();

        Close();
    }

    private bool ValidateEmail(string email)
    {
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        Regex regex = new Regex(pattern);
        Match match = regex.Match(email);

        return match.Success;
    }

}
