using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KT_IMAPClient;

public partial class PickMailboxWindow : Window
{
    public Client? client;

    public MailboxWindow? mailbox;

    public UserInfo? currentUserInfo;

    public PickMailboxWindow(List<string> mailboxes, Client clientSession, MailboxWindow mailboxWindow, UserInfo userInfo)
    {
        InitializeComponent();

        DataContext = mailboxes;

        client = clientSession;

        mailbox = mailboxWindow;

        currentUserInfo = userInfo;
    }

    private void MailboxClicked(object sender, MouseButtonEventArgs e)
    {
        var item = (sender as ListView)?.SelectedItem;

        if (item != null)
        {
            App.Mailbox = (string)item;

            if(client != null)
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

                client.SelectMailBox();

                if(mailbox != null)
                {
                    MailboxWindow.UserEmails = new ObservableCollection<Email>(client.FetchEmails().OrderByDescending(email => email.Date));

                    mailbox.Emails.ItemsSource = MailboxWindow.UserEmails;

                    mailbox.MailboxFolder.Text = (string)item;
                }
            }
            
            Close();
        }
    }
}
