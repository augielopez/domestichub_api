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
        private readonly AppleEmailService _appleEmailService;
        private readonly SupabaseEmailService _supabaseEmailService;

        public EmailController(AppleEmailService appleEmailService, SupabaseEmailService supabaseEmailService)
        {
            _appleEmailService = appleEmailService;
            _supabaseEmailService = supabaseEmailService;
        }

        [HttpGet("inbox")]
        public async Task<IActionResult> GetInbox()
        {
            try
            {
                var emails = await _appleEmailService.GetEmailsAsync();
                
                if(emails == null)
                {
                    return NotFound("No emails found!");
                }
                
                return Ok("Emails fetched successfully!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        
        [HttpPost("send-email")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
        {
            try
            {
                await _appleEmailService.SendEmailAsync(request.To, request.Subject, request.Body);
                return Ok(new { Message = "Email sent successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("delete-email-from-icloud")]
        public async Task<IActionResult> DeleteEmail([FromBody] DeleteRequest request)
        {
            try
            {
                // Call the bulk delete method with a single UID
                await _appleEmailService.DeleteEmailsAsync(new[] { request.Uid });

                return Ok(new { Message = "Email deleted successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        
        
        
        [HttpPost("delete-bulk-emails-from-icloud")]
        public async Task<IActionResult> DeleteEmails([FromBody] IEnumerable<string> uids)
        {
            try
            {
                // Call the bulk delete method with the provided UIDs
                await _appleEmailService.DeleteEmailsAsync(uids);

                return Ok(new { Message = $"{uids.Count()} emails deleted successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        
        [HttpGet("save-all-to-db")]
        public async Task<IActionResult> SaveToDb()
        {
            try
            {
                var emails = await _appleEmailService.GetEmailsAsync();
                await _supabaseEmailService.SaveEmailsAsync(emails);
                
                return Ok("Data saved to database successfully!");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        
        [HttpGet("get-db-emails")]
        public async Task<IActionResult> GetDbEmails()
        {
            try
            {
                var emails = await _supabaseEmailService.GetEmailsAsync();
                return Ok(emails);
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
