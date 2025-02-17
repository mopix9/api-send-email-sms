

using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Logging;
using System;
using Api.Models;

namespace NotificationService.Services
{
    public class EmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public void SendEmail(EmailMessage email)
        {
            try
            {
                var mimeMessage = new MimeMessage();

                // تنظیم آدرس فرستنده بر اساس SystemName
                string fromAddress;
                switch (email.SystemName.ToLower())
                {
                    case "afkar":
                        fromAddress = "afkar@xxxxx.com";
                        break;
                    case "crm":
                        fromAddress = "crm@xxxx.com";
                        break;
                    default:
                        fromAddress = "no-reply@example.com"; // آدرس پیش‌فرض
                        break;
                }

                mimeMessage.From.Add(new MailboxAddress(email.SystemName, fromAddress));
                mimeMessage.To.Add(new MailboxAddress(email.To, email.To));
                mimeMessage.Subject = email.Subject;

                // بررسی محتوا که آیا HTML است یا ساده
                if (IsHtmlContent(email.Body))
                {
                    // برای ایمیل HTML
                    mimeMessage.Body = new TextPart("html")
                    {
                        Text = email.Body + "<hr/><b>واحد توسعه نرم افزار شرکت تجارت الکترونیک  کیش</b>"
                    };
                }
                else
                {
                    // برای ایمیل ساده
                    mimeMessage.Body = new TextPart("plain")
                    {
                        Text = email.Body + "\n________________________________________\n*واحد توسعه نرم افزار شرکت تجارت الکترونیک  کیش*"
                    };
                }

                using (var client = new SmtpClient())
                {
                    // نادیده گرفتن خطای اعتبار گواهی در صورت عدم تطابق
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    // اتصال به سرور SMTP
                    client.Connect("linux20.sgnetway.net", 587, SecureSocketOptions.StartTls);

                    // احراز هویت با اطلاعات صحیح
                    client.Authenticate("user-name", "your-pass");

                    // ارسال ایمیل
                    client.Send(mimeMessage);

                    // قطع اتصال به صورت تمیز
                    client.Disconnect(true);
                }

                _logger.LogInformation($"ایمیل با موفقیت به {email.To} ارسال شد.");
            }
            catch (Exception ex)
            {
                // لاگ خطا
                _logger.LogError(ex, $"خطا در ارسال ایمیل به {email.To}.");
                throw; // خطا به کنترل‌کننده منتقل می‌شود
            }
        }

        private bool IsHtmlContent(string body)
        {
            // بررسی اینکه آیا محتوای ایمیل HTML است یا خیر
            return body.Contains("<html>") || body.Contains("<body>");
        }
    }
}
