using System.Diagnostics;
using domestichub_api.Data;
using domestichub_api.Models;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Newtonsoft.Json;

namespace domestichub_api.Services;

public class AppleEmailService
{
     private readonly IConfiguration _configuration;
     private readonly IServiceProvider _serviceProvider;
     private readonly SupabaseHttpClient _supabaseClient;

        public AppleEmailService(IConfiguration configuration, IServiceProvider serviceProvider, SupabaseHttpClient supabaseClient)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _supabaseClient = supabaseClient;
        }

        public async Task<List<Email>> GetEmailsAsync()
        {
            var emailList = new List<Email>();

            var settings = _configuration.GetSection("EmailSettings");

            using var client = new ImapClient();
            await client.ConnectAsync(settings["ImapServer"], int.Parse(settings["Port"]), SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(settings["Email"], settings["Password"]);

            var inbox = client.Inbox;
            await inbox.OpenAsync(MailKit.FolderAccess.ReadOnly);

            // Start the stopwatch
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Fetch email summaries and transform to Email objects
            foreach (var summary in inbox.Fetch(0, 0, MailKit.MessageSummaryItems.Full | MailKit.MessageSummaryItems.UniqueId))
            {
                var message = inbox.GetMessage(summary.UniqueId);

                // Use the new TransformToEmail method to process each email
                var email = TransformToEmail(message, summary.UniqueId);
                emailList.Add(email);
            }

            // Log elapsed time
            TimeSpan elapsed = stopwatch.Elapsed;
            Console.WriteLine($"Time taken to fetch emails: {elapsed.Hours} hours, {elapsed.Minutes} minutes, {elapsed.Seconds} seconds");

            await client.DisconnectAsync(true);
            return emailList;
        }
        
        public async Task<Email?> GetEmailByUniqueIdAsync(string uniqueId)
        {
            using (var client = new ImapClient())
            {
                try
                {
                    var settings = _configuration.GetSection("EmailSettings");
                    await client.ConnectAsync(settings["ImapServer"], int.Parse(settings["Port"]), SecureSocketOptions.SslOnConnect);
                    await client.AuthenticateAsync(settings["Email"], settings["Password"]);

                    // Open the INBOX folder
                    var inbox = client.Inbox;
                    await inbox.OpenAsync(FolderAccess.ReadOnly);

                    // Convert the string unique ID to the correct MailKit UniqueId
                    if (!UniqueId.TryParse(uniqueId, out UniqueId parsedUniqueId))
                    {
                        Console.WriteLine($"Invalid Unique ID: {uniqueId}");
                        return null;
                    }

                    // Fetch the email using the unique ID
                    var message = await inbox.GetMessageAsync(parsedUniqueId);

                    // Map the MimeMessage to the Email entity
                    var email = TransformToEmail(message, parsedUniqueId);

                    // Disconnect cleanly from the server
                    await client.DisconnectAsync(true);

                    return email;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching email: {ex.Message}");
                    return null;
                }
            }
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
            var uniqueIds = new List<UniqueId>();
            
            foreach (var emailUid in emailUids)
            {
                if (UniqueId.TryParse(emailUid, out UniqueId parsedUniqueId))
                {
                    uniqueIds.Add(parsedUniqueId); // Add successfully parsed UniqueId to the list
                }
                else
                {
                    // Log or handle invalid UID if necessary
                    Console.WriteLine($"Invalid UID: {emailUid}");
                }
            }
            
            var settings = _configuration.GetSection("EmailSettings");

            using var client = new ImapClient();
            await client.ConnectAsync(settings["ImapServer"], int.Parse(settings["Port"]), SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(settings["Email"], settings["Password"]);

            var inbox = client.Inbox;
            await inbox.OpenAsync(MailKit.FolderAccess.ReadWrite);
            
            inbox.AddFlags(uniqueIds, MessageFlags.Deleted, true);

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
        
        private Email TransformToEmail(MimeMessage message, UniqueId uniqueId)
        {
            return new Email
            {
                AttachmentCount = message.Attachments.Count(),       // Count attachments
                Bcc = message.Bcc.ToString(),                       // BCC recipients
                Cc = message.Cc.ToString(),                         // CC recipients
                Date = message.Date,                                // Email date
                From = message.From.ToString(),                     // From address
                Headers = message.Headers.ToString(),               // Email headers
                HtmlBody = message.HtmlBody,                        // HTML body
                Importance = message.Importance.ToString(),         // Email importance
                MessageId = message.MessageId,                      // Message ID
                Subject = message.Subject,                          // Email subject
                TextBody = message.TextBody,                        // Text body
                To = message.To.ToString(),                         // To recipients
                UniqueId = uniqueId.ToString(),                     // Unique ID as a string
                Username = ExtractUsername(ExtractEmailAddress(message.From.ToString())),
                Domain = ExtractDomain(ExtractEmailAddress(message.From.ToString()))
            };
        }
        
        // Helper to extract the full email address (e.g., "tonygtime@gmail.com")
        private static string ExtractEmailAddress(string fromAddress)
        {
            int start = fromAddress.IndexOf('<') + 1;
            int end = fromAddress.IndexOf('>');
            return fromAddress.Substring(start, end - start);
        }

        // Helper to extract the username (e.g., "tonygtime" from "tonygtime@gmail.com")
        private static string ExtractUsername(string email)
        {
            int atIndex = email.IndexOf('@');
            return email.Substring(0, atIndex);
        }

        // Helper to extract the domain (e.g., "gmail.com" from "tonygtime@gmail.com")
        private static string ExtractDomain(string email)
        {
            int atIndex = email.IndexOf('@');
            return email.Substring(atIndex + 1);
        }
}