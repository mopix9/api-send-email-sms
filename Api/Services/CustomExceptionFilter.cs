using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Net;

namespace NotificationService.Filters
{
    public class CustomExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<CustomExceptionFilter> _logger;

        public CustomExceptionFilter(ILogger<CustomExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;

            // لاگ خطا
            _logger.LogError(exception, "یک خطا رخ داد.");

            // بررسی نوع خطا
            if (exception is SmtpCommandException smtpEx)
            {
                // خطای مربوط به سرور SMTP
                context.Result = new ObjectResult(new
                {
                    Message = "خطا در اتصال به سرور SMTP.",
                    StatusCode = 502,
                    Details = smtpEx.Message
                })
                {
                    StatusCode = 502
                };
            }
            else if (exception is HttpRequestException httpEx)
            {
                // خطای مربوط به سرویس پیامک
                context.Result = new ObjectResult(new
                {
                    Message = "خطا در اتصال به سرویس پیامک.",
                    StatusCode = 503,
                    Details = httpEx.Message
                })
                {
                    StatusCode = 503
                };
            }
            else
            {
                // سایر خطاها
                context.Result = new ObjectResult(new
                {
                    Message = "یک خطای داخلی رخ داد.",
                    StatusCode = 500,
                    Details = exception.Message
                })
                {
                    StatusCode = 500
                };
            }

            // نشان‌دادن خطا به عنوان مدیریت‌شده
            context.ExceptionHandled = true;
        }
    }
}