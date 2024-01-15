using System.Windows.Media;

namespace KT_IMAPClient;

public class Email
{
    public int Id { get; set; }

    public string? Sender { get; set; }

    public string? Receiver { get; set; }

    public string? Subject { get; set; }

    public string? Body { get; set; }

    public string? Date { get; set; }

    public string? AttachmentName { get; set; }

    public byte[] AttachmentContent { get; set; } = null!;

    public ImageSource AttachmentImage { get; set; } = null!;
}
