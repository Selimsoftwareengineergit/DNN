namespace DNN.Services
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string FromName { get; set; }
        public string FromEmail { get; set; }
        public string EmailPassword { get; set; }
    }
}
