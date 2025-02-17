
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Api.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NotificationService.Services
{
    public class SmsService
    {
        private readonly string _serviceUrl = "https://api.payamak-panel.com/post/Send.asmx?op=SendSms";
        private readonly string _creditServiceUrl = "https://api.payamak-panel.com/post/Send.asmx?op=GetCredit";
        private readonly string _username;
        private readonly string _password;
        private readonly string _senderNumber;
        private readonly List<string> _managerPhoneNumbers; // لیست شماره‌های تلفن مدیر
        private readonly List<string> _managerEmails; // لیست ایمیل‌های مدیر
        private readonly EmailService _emailService; // سرویس ایمیل
        private readonly ILogger<SmsService> _logger; // لاگر
        private bool _isWarningSent = false; // فلگ برای پیگیری ارسال هشدار

        public SmsService(IConfiguration configuration, EmailService emailService, ILogger<SmsService> logger)
        {
            // خواندن تنظیمات از فایل appsettings.json
            var smsSettings = configuration.GetSection("SmsSettings");
            _username = smsSettings["Username"];
            _password = smsSettings["Password"];
            _senderNumber = smsSettings["SenderNumber"];

            // خواندن تنظیمات مدیر به‌صورت لیست
            var managerSettings = configuration.GetSection("ManagerSettings");
            _managerPhoneNumbers = managerSettings.GetSection("PhoneNumbers").Get<List<string>>();
            _managerEmails = managerSettings.GetSection("Emails").Get<List<string>>();

            // تزریق سرویس ایمیل
            _emailService = emailService;

            // تزریق لاگر
            _logger = logger;

            // بررسی صحت اطلاعات مدیر
            if (_managerPhoneNumbers == null || _managerPhoneNumbers.Count == 0 || _managerEmails == null || _managerEmails.Count == 0)
            {
                throw new InvalidOperationException("اطلاعات مدیر (شماره تلفن یا ایمیل) تنظیم نشده است.");
            }
        }

        // متد برای ارسال پیامک
        public async Task<int> SendSms(string phoneNumber, string message)
        {
            try
            {
                // بررسی موجودی حساب
                var credit = await GetCredit();
                if (credit <= 0)
                {
                    _logger.LogWarning("موجودی کافی نیست.");
                    return -1; // کد خطا برای موجودی ناکافی
                }

                // اگر موجودی کمتر از 2,000 پیامک بود و هشدار قبلاً ارسال نشده است
                if (credit < 2000 && !_isWarningSent)
                {
                    _isWarningSent = true; // فلگ را به true تغییر دهید
                    await SendWarningToManager(credit); // هشدار را ارسال کنید
                }
                else if (credit >= 2000) // اگر موجودی به 2,000 یا بیشتر رسید، فلگ را بازنشانی کنید
                {
                    _isWarningSent = false; // بازنشانی فلگ
                    _logger.LogInformation("موجودی شارژ شد. فلگ هشدار بازنشانی شد.");
                }

                // ساخت بدنه SOAP برای ارسال پیامک
                var soapBody = $@"<?xml version=""1.0"" encoding=""utf-8""?>
                <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
    xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
    xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                  <soap:Body>
                    <SendSms xmlns=""http://tempuri.org/"">
                      <username>{_username}</username>
                      <password>{_password}</password>
                      <to>
                        <string>{phoneNumber}</string>
                      </to>
                      <from>{_senderNumber}</from>
                      <text>{message}</text>
                      <isflash>false</isflash>
                      <udh></udh>
                      <recId></recId>
                      <status></status>
                    </SendSms>
                  </soap:Body>
                </soap:Envelope>";

                // ارسال درخواست به سرویس ملی پیامک
                using var httpClient = new HttpClient();
                var httpContent = new StringContent(soapBody, Encoding.UTF8, "text/xml");
                httpContent.Headers.Add("SOAPAction", "http://tempuri.org/SendSms");

                var response = await httpClient.PostAsync(_serviceUrl, httpContent);

                // بررسی پاسخ
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var sendResult = ParseSendSmsResult(responseContent);
                    if (sendResult == 0)
                    {
                        _logger.LogInformation($"پیامک با موفقیت به {phoneNumber} ارسال شد.");
                    }
                    else
                    {
                        _logger.LogWarning($"ارسال پیامک به {phoneNumber} با خطا مواجه شد. کد خطا: {sendResult}");
                    }
                }
                else
                {
                    _logger.LogError($"خطا: {response.StatusCode}");
                    return -1; // کد خطا
                }

                return 0; // موفقیت
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"خطا در ارسال پیامک به {phoneNumber}.");
                throw; // خطا به کنترل‌کننده منتقل می‌شود
            }
        }

        // متد برای بررسی موجودی حساب
        public async Task<decimal> GetCredit()
        {
            try
            {
                var soapBody = $@"<?xml version=""1.0"" encoding=""utf-8""?>
                <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                  <soap:Body>
                    <GetCredit xmlns=""http://tempuri.org/"">
                      <username>{_username}</username>
                      <password>{_password}</password>
                    </GetCredit>
                  </soap:Body>
                </soap:Envelope>";

                using var httpClient = new HttpClient();
                var httpContent = new StringContent(soapBody, Encoding.UTF8, "text/xml");
                httpContent.Headers.Add("SOAPAction", "http://tempuri.org/GetCredit");

                var response = await httpClient.PostAsync(_creditServiceUrl, httpContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var credit = ParseCreditResult(responseContent);
                    return credit;
                }
                else
                {
                    _logger.LogError($"خطا: {response.StatusCode}");
                    return -1; // خطا در بررسی موجودی
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بررسی موجودی حساب.");
                return -2; // خطا در بررسی موجودی
            }
        }

        // متد برای تجزیه موجودی حساب
        private decimal ParseCreditResult(string responseContent)
        {
            var startTag = "<GetCreditResult>";
            var endTag = "</GetCreditResult>";
            var startIndex = responseContent.IndexOf(startTag) + startTag.Length;
            var endIndex = responseContent.IndexOf(endTag);

            if (startIndex > 0 && endIndex > startIndex)
            {
                var result = responseContent.Substring(startIndex, endIndex - startIndex);
                if (decimal.TryParse(result, out decimal credit))
                {
                    return credit;
                }
            }

            return -1; // خطا در تجزیه پاسخ
        }

        // متد برای ارسال هشدار به مدیر
        private async Task SendWarningToManager(decimal credit)
        {
            try
            {
                _logger.LogInformation("بررسی موجودی و ارسال هشدار...");

                // ارسال ایمیل به تمام ایمیل‌های مدیر
                foreach (var email in _managerEmails)
                {
                    await SendEmailToManager(email, credit);
                }

                // ارسال پیامک به تمام شماره‌های تلفن مدیر
                foreach (var phoneNumber in _managerPhoneNumbers)
                {
                    await SendSmsToManager(phoneNumber, $"هشدار: موجودی پیامک شما کمتر از 2,000 عدد است. موجودی فعلی: {credit}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال هشدار به مدیر.");
            }
        }

        // متد برای ارسال ایمیل به مدیر
        private async Task SendEmailToManager(string email, decimal credit)
        {
            try
            {
                _logger.LogInformation($"ارسال ایمیل به {email}...");
                var emailMessage = new EmailMessage
                {
                    SystemName = "crm",
                    To = email,
                    Subject = "هشدار کمبود موجودی پیامک",
                    Body = $"موجودی پیامک شما کمتر از 2,000 عدد است. موجودی فعلی: {credit}"
                };
                _emailService.SendEmail(emailMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"خطا در ارسال ایمیل به {email}.");
            }
        }

        // متد برای ارسال پیامک به مدیر
        private async Task SendSmsToManager(string phoneNumber, string message)
        {
            try
            {
                _logger.LogInformation($"ارسال پیامک به {phoneNumber}...");
                await SendSms(phoneNumber, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"خطا در ارسال پیامک به {phoneNumber}.");
            }
        }

        // متد برای تجزیه کد وضعیت ارسال پیامک
        private int ParseSendSmsResult(string responseContent)
        {
            var startTag = "<SendSmsResult>";
            var endTag = "</SendSmsResult>";
            var startIndex = responseContent.IndexOf(startTag) + startTag.Length;
            var endIndex = responseContent.IndexOf(endTag);

            if (startIndex > 0 && endIndex > startIndex)
            {
                var result = responseContent.Substring(startIndex, endIndex - startIndex);
                if (int.TryParse(result, out int sendResult))
                {
                    return sendResult;
                }
            }

            return -3; // خطا در تجزیه پاسخ
        }
    }
}