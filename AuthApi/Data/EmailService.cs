namespace AuthApi.Data
{
    using MailKit.Net.Smtp;
    using MimeKit;

    public class EmailService
    {
        private readonly string _smtpServer = "smtp.yandex.com"; // Замените на свой SMTP-сервер
        private readonly int _smtpPort = 587; // Порт (обычно 587 для TLS)
        private readonly string _email = "OctarineCore42@yandex.ru"; // Ваш email
        private readonly string _password = "c/!{QOv\\\\L85qsT"; // Пароль от почты

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
