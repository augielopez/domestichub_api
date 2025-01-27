using MailKit;
using MimeKit;

namespace domestichub_api.Models;

public class Email
{
    public int AttachmentCount { get; set; }
    public string Bcc { get; set; }
    public string Cc { get; set; }
    public DateTimeOffset Date { get; set; }
    public string From { get; set; }
    public string? Headers { get; set; }
    public string HtmlBody { get; set; }
    public string Importance { get; set; }
    public string MessageId { get; set; }
    public string Subject { get; set; }
    public string TextBody { get; set; }
    public string To { get; set; }
    public UniqueId UniqueId { get; set; }
}