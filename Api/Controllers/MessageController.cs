
using Microsoft.AspNetCore.Mvc;
using NotificationService.Services;
using Api.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly SmsService _smsService;
        private readonly EmailService _emailService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(SmsService smsService, EmailService emailService, ILogger<NotificationsController> logger)
        {
            _smsService = smsService;
            _emailService = emailService;
            _logger = logger;
        }

        // ارسال پیامک
        [HttpPost("send_sms")]
        public async Task<IActionResult> SendSmsMessage([FromBody] SmsMessage smsMessage)
        {
            if (smsMessage == null || string.IsNullOrEmpty(smsMessage.To) || string.IsNullOrEmpty(smsMessage.Message))
            {
                _logger.LogWarning("قالب پیامک نامعتبر است.");
                return BadRequest(new { Message = "قالب پیامک نامعتبر است.", StatusCode = 400 });
            }

            // بررسی سیستم فرستنده
            if (smsMessage.SystemName.ToLower() == "crm" || smsMessage.SystemName.ToLower() == "afkar")
            {
                try
                {
                    // ارسال پیامک
                    int result = await _smsService.SendSms(smsMessage.To, smsMessage.Message);
                    return Ok(new { Message = "پیامک با موفقیت ارسال شد.", StatusCode = 200, Result = result });
                }
                catch (Exception ex)
                {
                    // خطاها توسط فیلتر خطای سفارشی مدیریت می‌شوند
                    throw;
                }
            }

            _logger.LogWarning("نام سیستم برای پیامک نامعتبر است.");
            return BadRequest(new { Message = "نام سیستم برای پیامک نامعتبر است.", StatusCode = 400 });
        }

        // ارسال ایمیل
        [HttpPost("send_email")]
        public async Task<IActionResult> SendEmailMessage([FromBody] EmailMessage emailMessage)
        {
            if (emailMessage == null || string.IsNullOrEmpty(emailMessage.To) || string.IsNullOrEmpty(emailMessage.Subject) || string.IsNullOrEmpty(emailMessage.Body))
            {
                _logger.LogWarning("قالب ایمیل نامعتبر است.");
                return BadRequest(new { Message = "قالب ایمیل نامعتبر است.", StatusCode = 400 });
            }

            // بررسی سیستم فرستنده
            if (emailMessage.SystemName.ToLower() == "crm" || emailMessage.SystemName.ToLower() == "afkar")
            {
                try
                {
                    // ارسال ایمیل
                    _emailService.SendEmail(emailMessage);
                    return Ok(new { Message = "ایمیل با موفقیت ارسال شد.", StatusCode = 200 });
                }
                catch (Exception ex)
                {
                    // خطاها توسط فیلتر خطای سفارشی مدیریت می‌شوند
                    throw;
                }
            }

            _logger.LogWarning("نام سیستم برای ایمیل نامعتبر است.");
            return BadRequest(new { Message = "نام سیستم برای ایمیل نامعتبر است.", StatusCode = 400 });
        }
    }
}