using System.IO;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System;

namespace KT_IMAPClient;

public partial class EmailWindow : Window
{
    private static readonly ImageConverter _imageConverter = new ImageConverter();

    Bitmap imageBitmap = null!;

    public EmailWindow(Email email)
    {
        InitializeComponent();

        if(email.AttachmentContent != null)
        {
            MemoryStream ms = new MemoryStream(email.AttachmentContent);

            imageBitmap = (Bitmap)Image.FromStream(ms);

            var handle = imageBitmap.GetHbitmap();

            ImageSource newSource = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            email.AttachmentImage = newSource;
        }

        DataContext = email;

    }
}
