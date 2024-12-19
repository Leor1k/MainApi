namespace AuthApi.Data
{
    using MailKit.Net.Smtp;
    using MimeKit;

    public class EmailService
    {
        private readonly string _smtpServer = "smtp.yandex.ru";

        private readonly int _smtpPort = 587; 
        private readonly string _email = "OctarineCore42@yandex.ru"; 
        private readonly string _password = "jhNz=t@DQGli8z"; 

        public async Task SendEmailAsync(string recipientEmail, string subject, string message)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Octarine Core", _email)); // Имя отправителя
            email.To.Add(new MailboxAddress("", recipientEmail)); // Получатель
            email.Subject = subject;

            email.Body = new TextPart("plain")
            {
                Text = message
            };

            using (var smtpClient = new SmtpClient())
            {
                await smtpClient.ConnectAsync(_smtpServer, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(_email, _password);
                await smtpClient.SendAsync(email);
                await smtpClient.DisconnectAsync(true);
            }
        }
    }

}
