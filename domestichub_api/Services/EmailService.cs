using System.Diagnostics;
using domestichub_api.Models;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace domestichub_api.Services;

public class EmailService
{
     private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<MimeMessage>> GetEmailsAsync()
        {
            var emailList = new List<MimeMessage>();

            var settings = _configuration.GetSection("EmailSettings");

            using var client = new ImapClient();
            await client.ConnectAsync(settings["ImapServer"], int.Parse(settings["Port"]), SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(settings["Email"], settings["Password"]);

            var inbox = client.Inbox;
            await inbox.OpenAsync(MailKit.FolderAccess.ReadOnly);

            // Start the stopwatch
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            foreach (var summary in inbox.Fetch(0, -1, MailKit.MessageSummaryItems.Full | MailKit.MessageSummaryItems.UniqueId))
            {
                var message = inbox.GetMessage(summary.UniqueId);

                emailList.Add(message);
            }
            
            // Convert elapsed time to a TimeSpan
            TimeSpan elapsed = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds);

            // Extract hours, minutes, seconds
            int hours = elapsed.Hours;
            int minutes = elapsed.Minutes;
            int seconds = elapsed.Seconds;

            Console.WriteLine($"Time taken to fetch emails: {hours} hours, {minutes} minutes, {seconds} seconds");

            await client.DisconnectAsync(true);
            return emailList;
        }
        
        public async Task<List<MimeMessage>> GetEmailsSaveToDbAsync()
        {
            var emailList = new List<MimeMessage>();
            var emailListDto = new List<Email>();
            
            var settings = _configuration.GetSection("EmailSettings");

            using var client = new ImapClient();
            await client.ConnectAsync(settings["ImapServer"], int.Parse(settings["Port"]), SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(settings["Email"], settings["Password"]);

            var inbox = client.Inbox;
            await inbox.OpenAsync(MailKit.FolderAccess.ReadOnly);

            // Start the stopwatch
            Stopwatch stopwatch = Stopwatch.StartNew();
            
            foreach (var summary in inbox.Fetch(0, -1, MailKit.MessageSummaryItems.Full | MailKit.MessageSummaryItems.UniqueId))
            {
                var message = inbox.GetMessage(summary.UniqueId);
                
                
                var email = new Email
                {
                    AttachmentCount = message.Attachments.Count(), // Count attachments
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
                    To = message.To.ToString(),
                    UniqueId = summary.UniqueId
                };
                
                emailList.Add(message);
                emailListDto.Add(email);
            }
            
            // Convert elapsed time to a TimeSpan
            TimeSpan elapsed = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds);

            // Extract hours, minutes, seconds
            int hours = elapsed.Hours;
            int minutes = elapsed.Minutes;
            int seconds = elapsed.Seconds;

            Console.WriteLine($"Time taken to fetch emails: {hours} hours, {minutes} minutes, {seconds} seconds");

            await client.DisconnectAsync(true);
            return emailList;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var settings = _configuration.GetSection("EmailSettings");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Your Name", settings["Email"]));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(settings["SmtpServer"], 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(settings["Email"], settings["Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        
        public async Task DeleteEmailsAsyncc(IEnumerable<string> emailUids)
        {
            var settings = _configuration.GetSection("EmailSettings");

            using var client = new ImapClient();
            await client.ConnectAsync(settings["ImapServer"], int.Parse(settings["Port"]), SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(settings["Email"], settings["Password"]);

            var inbox = client.Inbox;
            await inbox.OpenAsync(MailKit.FolderAccess.ReadWrite);

            // Parse UIDs and add the Deleted flag in bulk
            // var uids = emailUids.Select(uid => new UniqueId(uint.Parse(uid))).ToList();
            //var message = await inbox.GetMessageAsync(emailUids);
            // inbox.AddFlags(uids, MessageFlags.Deleted, true);

            // Expunge to permanently remove messages marked for deletion
            await inbox.ExpungeAsync();

            await client.DisconnectAsync(true);
        }
        
        public async Task DeleteEmailsAsync(IEnumerable<string> emailUids)
        {
            var settings = _configuration.GetSection("EmailSettings");

            using var client = new ImapClient();
            await client.ConnectAsync(settings["ImapServer"], int.Parse(settings["Port"]), SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(settings["Email"], settings["Password"]);

            var inbox = client.Inbox;
            await inbox.OpenAsync(MailKit.FolderAccess.ReadWrite);

            foreach (var emailUid in emailUids)
            {
                // Parse the string UID into a UniqueId
                var uid = new UniqueId(uint.Parse(emailUid));

                // Fetch the email to ensure it exists (optional but useful for validation)
                var message = await inbox.GetMessageAsync(uid);

                if (message != null)
                {
                    // Mark the email as deleted
                    inbox.AddFlags(uid, MessageFlags.Deleted, true);

                    Console.WriteLine($"Deleted email: {message.Subject} from {message.From}");
                }
                else
                {
                    Console.WriteLine($"Email with UID {emailUid} not found.");
                }
            }

            // Permanently delete messages marked for deletion
            await inbox.ExpungeAsync();

            await client.DisconnectAsync(true);
        }


}