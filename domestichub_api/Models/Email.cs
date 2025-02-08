using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MailKit;
using MimeKit;
using Newtonsoft.Json;

namespace domestichub_api.Models;

[Table("tb_emails")]
public class Email
{
    [Key]
    [Column("pk")]
    [JsonProperty("pk")]
    public string UniqueId { get; set; }
    
    [Column("attachment_count")]
    [JsonProperty("attachment_count")]
    public int AttachmentCount { get; set; }
    
    [Column("bcc")]
    [JsonProperty("bcc")]
    public string Bcc { get; set; }
    
    [Column("cc")]
    [JsonProperty("cc")]
    public string Cc { get; set; }
    
    [Column("date")]
    [JsonProperty("date")]
    public DateTimeOffset Date { get; set; }
    
    [Column("from")]
    [JsonProperty("from")]
    public string From { get; set; }
    
    [Column("headers")]
    [JsonProperty("headers")]
    public string? Headers { get; set; }
    
    [Column("html_body")]
    [JsonProperty("html_body")]
    public string HtmlBody { get; set; }
    
    [Column("importance")]
    [JsonProperty("importance")]
    public string Importance { get; set; }
    
    [Column("message_id")]
    [JsonProperty("message_id")]
    public string MessageId { get; set; }
    
    [Column("subject")]
    [JsonProperty("subject")]
    public string Subject { get; set; }
    
    [Column("text_body")]
    [JsonProperty("text_body")]
    public string TextBody { get; set; }
    
    [Column("to")]
    [JsonProperty("to")]
    public string To { get; set; }
    
    [Column("user_name")]
    [JsonProperty("user_name")]
    public string Username { get; set; } // New property to store username

    [Column("domain")]
    [JsonProperty("domain")]
    public string Domain { get; set; }   // New property to store domain


    public static Email FromMimeMessage(MimeMessage message)
    {
        return new Email
        {
            UniqueId = message.MessageId,
            AttachmentCount = message.Attachments.Count(),
            Bcc = message.Bcc.ToString(),
            Cc = message.Cc.ToString(),
            Date = message.Date,
            From = message.From.ToString(),
            Headers = message.Headers.ToString(),
            HtmlBody = message.HtmlBody,
            Importance = message.Importance.ToString(),
            MessageId = message.MessageId,
            Subject = message.Subject,
            TextBody = message.TextBody,
            To = message.To.ToString()
        };
    }
}
