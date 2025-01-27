using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using domestichub_api.Services;

namespace domestichub_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly EmailService _emailService;

        public EmailController(EmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpGet("inbox")]
        public async Task<IActionResult> GetInbox()
        {
            try
            {
                var emails = await _emailService.GetEmailsAsync();
                return Ok(emails.Select(email => new
                {
                    Subject = email.Subject,
                    From = email.From.ToString(),
                    Body = email.TextBody,
                    Uid = email.MessageId
                }));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        
        [HttpGet("svetodb")]
        public async Task<IActionResult> SaveToDb()
        {
            try
            {
                var emails = await _emailService.GetEmailsSaveToDbAsync();
                return Ok("Data saved to database successfully!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
        {
            try
            {
                await _emailService.SendEmailAsync(request.To, request.Subject, request.Body);
                return Ok(new { Message = "Email sent successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteEmail([FromBody] DeleteRequest request)
        {
            try
            {
                // Call the bulk delete method with a single UID
                await _emailService.DeleteEmailsAsync(new[] { request.Uid });

                return Ok(new { Message = "Email deleted successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        
        
        
        [HttpPost("delete-bulk")]
        public async Task<IActionResult> DeleteEmails([FromBody] IEnumerable<string> uids)
        {
            try
            {
                // Call the bulk delete method with the provided UIDs
                await _emailService.DeleteEmailsAsync(uids);

                return Ok(new { Message = $"{uids.Count()} emails deleted successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }

    public record EmailRequest(string To, string Subject, string Body);
    public record DeleteRequest(string Uid);
}
