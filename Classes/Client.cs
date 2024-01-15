using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace KT_IMAPClient;

public class Client : IMAP
{
    private readonly string Email;

    private readonly string Password;

    private int emailCount;

    private int currentEmail;

    private UserInfo? currentUserInfo;

    public Client(string host, int port, UserInfo userInfo) : base(host, port)
    {
        Email = userInfo.Email;
        Password = userInfo.Password;
        currentUserInfo = userInfo;
    }

    public bool LogIn(UserInfo userInfo)
    {
        if (Connect(userInfo))
        {
            if (SendIMAP("LOGIN " + userInfo.Email + " " + userInfo.Password + "\r\n"))
            {
                SelectMailBox();

                return true;
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

                ErrorWindow errorWindow = new("Invalid credentials!");

                if (App.ErrorWindows != null)
                {
                    App.ErrorWindows.Add(errorWindow);
                }

                errorWindow.Show();

                return false;
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

            ErrorWindow errorWindow = new("Connection error!");

            if (App.ErrorWindows != null)
            {
                App.ErrorWindows.Add(errorWindow);
            }

            errorWindow.Show();

            return false;
        }
    }

    public void LogOut()
    {
        SendIMAP("LOGOUT" + "\r\n");
        Disconnect();
    }

    public List<string> ListMailBoxes()
    {
        IMAP refreshIMAP = new("mail.inbox.lt", 993);

        refreshIMAP.Disconnect();

        if (currentUserInfo != null)
        {
            refreshIMAP.Connect(currentUserInfo);
        }

        if (currentUserInfo != null)
        {
            LogIn(currentUserInfo);
        }

        output = null;

        List<string> mailboxes = new();

        SendIMAP("LIST \"\" \"*\"" + "\r\n");

        if (output != null)
        {
            using (StringReader reader = new StringReader(output))
            {
                string? line = reader.ReadLine();
                if (reader != null)
                {
                    if (line != null)
                    {
                        while (line != null && line.StartsWith("* LIST"))
                        {
                            string split = line.Split(" \"/\" ")[1];
                            mailboxes.Add(split);
                            line = reader.ReadLine();
                        }
                    }

                }
            }
        }

        return mailboxes;
    }

    public void SelectMailBox()
    {

        SendIMAP("SELECT " + App.Mailbox + "\r\n");

        emailCount = GetEmailCount();

        currentEmail = emailCount;
    }

    private int GetEmailCount()
    {
        int count = 0;

        string regexPattern = @"\* (\d+) EXISTS";

        Regex regex = new(regexPattern);

        if (output != null)
        {
            using (StringReader reader = new StringReader(output))
            {
                string? line;
                if (reader != null)
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        Match match = regex.Match(line);
                        if (match.Success)
                        {
                            count = int.Parse(match.Groups[1].Value);
                        }
                    }
                }
            }
        }

        return count;
    }

    public List<Email> FetchEmails()
    {
        List<Email> emails = new();

        while (currentEmail > 0)
        {
            Email email = ExtractInfo(currentEmail);

            emails.Add(email);

            currentEmail--;
        }

        return emails;
    }

    public Email ExtractInfo(int currentEmail)
    {
        output = string.Empty;

        SendIMAP("FETCH " + currentEmail.ToString() + " (BODY[HEADER])" + "\r\n");

        Email email = new();

        email.Id = currentEmail;

        string senderPattern = @"From:\s*(.+)";
        string receiverPattern = @"To:\s*(.+)";
        string datePattern = @"Date:\s*(.+)";
        string subjectPattern = @"Subject:\s*(.+)";

        using (StringReader reader = new StringReader(output))
        {
            string? line;
            if (reader != null)
            {
                while ((line = reader.ReadLine()) != null)
                {
                    Match senderMatch = Regex.Match(line, senderPattern);
                    Match receiverMatch = Regex.Match(line, receiverPattern);
                    Match dateMatch = Regex.Match(line, datePattern);
                    Match subjectMatch = Regex.Match(line, subjectPattern);

                    if (senderMatch.Success)
                    {
                        email.Sender = senderMatch.Groups[1].Value.Trim();
                    }
                    if (receiverMatch.Success)
                    {
                        email.Receiver = receiverMatch.Groups[1].Value.Trim();
                    }
                    if (dateMatch.Success)
                    {
                        email.Date = dateMatch.Groups[1].Value.Trim();
                    }
                    if (subjectMatch.Success)
                    {
                        email.Subject = subjectMatch.Groups[1].Value.Trim();
                    }
                }
            }
        }

        if (email.Date != null)
        {
            email.Date = DateTime.Parse(email.Date).ToString("yyyy-MM-dd HH:mm:ss");
        }

        email.Body = ExtractBody(currentEmail);

        FetchBody();

        string pattern = @"filename=([\w\.]+)";

        Match match = Regex.Match(output, pattern);
        if (match.Success)
        {
            string fileName = match.Groups[1].Value;

            int startIndex = output.IndexOf(match.Value) + match.Value.Length;
            string content = output.Substring(startIndex).TrimEnd('\r', '\n');

            content.TrimStart();
            content = content.Split("\r\n\r\n")[1];
            content = content.Replace("\n", "").Replace("\r", "");

            byte[] contentBytes = Convert.FromBase64String(content);

            email.AttachmentName = fileName;
            email.AttachmentContent = contentBytes;
        }

        return email;
    }
    public string? ExtractBody(int currentEmail)
    {
        output = string.Empty;

        string? body = null;

        SendIMAP("FETCH " + currentEmail + " BODY[1]" + "\r\n");

        Regex regex = new Regex(@"\d+\sFETCH\s\(BODY\[1\]\s\{(\d+)\}\r\n([^\)]+)\)\r\n" + "CMD" + commandNumber + " OK");
        Match bodyMatch = regex.Match(output);

        if (bodyMatch.Success)
        {
            body = bodyMatch.Groups[2].Value;

            body = body.TrimEnd('\r', '\n');

        }

        return body;
    }

    public void FetchBody()
    {
        output = null;

        SendIMAP("FETCH " + currentEmail.ToString() + " BODY[]" + "\r\n");
    }

}
