namespace BanklyEmailNotificationApplication.Model
{
    public class EmailSettings
    {
        public string Mail { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool UseSsl { get; set; }
        public string SmtpServer { get; set; }
        public int Port { get; set; }
    }

   
}