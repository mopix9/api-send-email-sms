namespace Api.Models
{

    public class EmailMessage
    {    
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        //public string From { get; set; } // Optional: Allows overriding the "From" address
        public string SystemName { get; set; } // New field for identifying the sender system
    }

}