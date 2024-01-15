using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KT_IMAPClient;

public partial class MailboxWindow : Window
{
    public static ObservableCollection<Email>? UserEmails;

    public static List<EmailWindow> EmailWindows = new();

    public static List<PickMailboxWindow> PickMailboxWindows = new();

    public Client? client;

    public static UserInfo? currentUserInfo;

    public MailboxWindow(Client clientSession, UserInfo userInfo)
    {
        InitializeComponent();

        clientSession.SelectMailBox();

        UserEmails = new ObservableCollection<Email>(clientSession.FetchEmails().OrderByDescending(email => email.Date));

        Emails.ItemsSource = UserEmails;

        MailboxName.Text = userInfo.Email;

        MailboxFolder.Text = "Inbox";

        client = clientSession;

        currentUserInfo = userInfo;
    }

    private void EmailClicked(object sender, MouseButtonEventArgs e)
    {
        var item = (sender as ListView)?.SelectedItem;

        if(item != null)
        {
            if (EmailWindows != null)
            {
                for (int i = EmailWindows.Count - 1; i >= 0; --i)
                {
                    EmailWindows[i].Close();
                    EmailWindows.Remove(EmailWindows[i]);
                }
            }

            EmailWindow emailWindow = new((Email)item);

            if(EmailWindows != null)
            {
                EmailWindows.Add(emailWindow);
            }

            emailWindow.Show();
        }
    }
    private void SignOutClicked(object sender, RoutedEventArgs e)
    {
        if(client != null)
        {
            client.LogOut();
        }

        Application.Current.Shutdown();
    }
    private void RefreshClicked(object sender, RoutedEventArgs e)
    {
        if(client != null)
        {
            IMAP refreshIMAP = new("mail.inbox.lt", 993);

            refreshIMAP.Disconnect();

            if(currentUserInfo != null)
            {
                refreshIMAP.Connect(currentUserInfo);
            }
            
            if(currentUserInfo != null)
            {
                client.LogIn(currentUserInfo);
            }

            client.SelectMailBox();
        }

        if (UserEmails != null)
        {
            UserEmails.Clear();
        }
        
        if(client != null)
        {
            UserEmails = new ObservableCollection<Email>(client.FetchEmails().OrderByDescending(email => email.Date));

            Emails.ItemsSource = UserEmails;
        }
    }

    private void ChangeMailboxClicked(object sender, RoutedEventArgs e)
    {
        if (PickMailboxWindows != null)
        {
            for (int i = PickMailboxWindows.Count - 1; i >= 0; --i)
            {
                PickMailboxWindows[i].Close();
                PickMailboxWindows.Remove(PickMailboxWindows[i]);
            }
        }

        if (PickMailboxWindows != null)
        {
            for (int i = PickMailboxWindows.Count - 1; i >= 0; --i)
            {
                PickMailboxWindows[i].Close();
                PickMailboxWindows.Remove(PickMailboxWindows[i]);
            }
        }

        if (client != null)
        {
            IMAP refreshIMAP = new("mail.inbox.lt", 993);

            refreshIMAP.Disconnect();

            if (currentUserInfo != null)
            {
                refreshIMAP.Connect(currentUserInfo);
            }

            if (currentUserInfo != null)
            {
                client.LogIn(currentUserInfo);
            }
               
            if(currentUserInfo != null)
            {
                PickMailboxWindow pickMailboxWindow = new(client.ListMailBoxes(), client, this, currentUserInfo);

                if (PickMailboxWindows != null)
                {
                    PickMailboxWindows.Add(pickMailboxWindow);
                }

                pickMailboxWindow.Show();
            }
            
            
        }
    }
}
