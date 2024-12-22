namespace AuthApi.Data
{
    using MailKit;
    using MailKit.Net.Smtp;
    using MimeKit;

    public class EmailService
    {
        private readonly string _smtpServer = "smtp.yandex.ru";
        private readonly int _smtpPort = 587; 
        private readonly string _email = "OctarineCore42@yandex.ru"; 
        private readonly string _password = "sjuhqjvcdcbhodqy"; 

        public async Task SendEmailAsync(string recipientEmail, string subject, string message)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Octarine Core", _email));
            email.To.Add(new MailboxAddress("", recipientEmail));
            email.Subject = subject;
            email.Body = new TextPart("plain")
            {
                Text = message
            };
            using (var smtpClient = new SmtpClient(new ProtocolLogger(Console.OpenStandardOutput())))

            {
                await smtpClient.ConnectAsync(_smtpServer, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(_email, _password);
                await smtpClient.SendAsync(email);
                await smtpClient.DisconnectAsync(true);
            }
        }
    }

}
