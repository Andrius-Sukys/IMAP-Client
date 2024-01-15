using System.Windows;

namespace KT_IMAPClient;

public partial class ErrorWindow : Window
{
    public ErrorWindow(string message)
    {
        InitializeComponent();

        DataContext = message;
    }
}
