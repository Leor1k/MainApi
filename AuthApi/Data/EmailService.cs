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
            Console.WriteLine("Создал письмо.");

            email.From.Add(new MailboxAddress("Octarine Core", _email));
            Console.WriteLine("Поставил отправителя");

            email.To.Add(new MailboxAddress("", recipientEmail));
            Console.WriteLine("Поставил получателя");
            email.Subject = subject;

            Console.WriteLine("Поставил ему");
            email.Body = new TextPart("plain")
            {
                Text = message
            };
            Console.WriteLine(" Создал тему");
            using (var smtpClient = new SmtpClient(new ProtocolLogger(Console.OpenStandardOutput())))

            {
                await smtpClient.ConnectAsync(_smtpServer, _smtpPort, MailKit.Security.SecureSocketOptions.SslOnConnect);
                Console.WriteLine("Создал конект");
                await smtpClient.AuthenticateAsync(_email, _password);
                Console.WriteLine("Аутендефецировал");
                await smtpClient.SendAsync(email);
                Console.WriteLine("Отпавил");
                await smtpClient.DisconnectAsync(true);
                Console.WriteLine("Дисконект");
            }
        }
    }

}
