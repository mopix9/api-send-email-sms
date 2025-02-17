namespace Api.Models
{

    public class SmsMessage
    {
        public string To { get; set; }
        public string Message { get; set; }
        public string SystemName { get; set; } // برای تعیین سیستم فرستنده (Afkar یا CRM)

    }

}
