using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;

namespace KT_IMAPClient
{
    public partial class App : Application
    {
        public static List<ErrorWindow> ErrorWindows = new();

        public static string Mailbox = "INBOX";
    }
}
