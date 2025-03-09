using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthApi.Data;
using AuthApi.Models;
using AuthApi.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        public AuthController(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest loginRequest)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.email == loginRequest.email);

            if (user == null)
            {
                return Unauthorized(new { message = "Пользователя с таким Email не существует" });
            }
            else if (user.password_hash != loginRequest.password)
            {
                return Unauthorized (new { message = "Неверный email или пароль" });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("ТетраГидроКанабиноловаяЯмаЖаждаДозы");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
            new Claim("users_id", user.user_id.ToString()),
            new Claim(ClaimTypes.Email, user.email),
            new Claim("users_name", user.username)                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { token = tokenString });
        }
        [HttpPost("registration")]
        public async Task<IActionResult> Registration([FromBody] User newUser)
        {
            var user = await _context.Users
        .FirstOrDefaultAsync(u => u.email == newUser.email);
            if (user != null)
            {
                return Unauthorized(new { message = "Пользователь с таким Email уже существует" });
            }
            newUser.confirmationcode = GenerateConfirmationCode();
            newUser.createdat = DateTime.UtcNow.AddHours(1);
            newUser.isconfirmed = false;
            newUser.avatar_url = $"http://127.0.0.1:9000/avatars/IconUser2.png";
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            EmailAndCode email = new EmailAndCode();
            email.Email = newUser.email;
            await SendCodeMail(email);
            return Ok(new { message = $"Для подтверждения регистрации, введите код высланный на {newUser.email}" });
        }
        [HttpPost("send_code")]
        public async Task<IActionResult> SendCodeMail([FromBody] EmailAndCode email)
        {
            var user = await _context.Users
        .FirstOrDefaultAsync(u => u.email == email.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Пользователя с таким Email не существует" });
            }
            user.confirmationcode = GenerateConfirmationCode();
            user.createdat = DateTime.UtcNow.AddHours(1);
            _context.Update(user);
            await _context.SaveChangesAsync();
            await SendConfirmationEmail(user.email, user.confirmationcode);
            return Ok(new { message = $"Код подтвежденя выслан на {user.email}" });
        }
        public string GenerateConfirmationCode(int length = 6)
        {
            var random = new Random();
            var code = new char[length];
            for (int i = 0; i < length; i++)
            {
                code[i] = (char)('0' + random.Next(0, 10)); 
            }
            return new string(code);
        }

        public async Task SendConfirmationEmail(string userEmail, string confirmationCode)
        {
            string htmlMessage = GenerateHtmlEmail(confirmationCode);
            await _emailService.SendEmailAsync(userEmail, "Подтверждение регистрации", htmlMessage); 
        }
        string GenerateHtmlEmail(string confirmationCode)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            margin: 0;
            padding: 20px;
            background-color: #f4f4f4;
        }}
        h1 {{
            color: #4CAF50;
        }}
        p {{
            font-size: 16px;
            color: #333;
        }}
        .confirmation-code {{
            font-weight: bold;
            font-size: 18px;
            color: #4CAF50;
        }}
        .footer {{
            font-size: 12px;
            color: #777;
        }}
    </style>
</head>
<body>
    <h1>Добро пожаловать в Octarine Core!</h1>
    <p>Ваш код подтверждения: <span class='confirmation-code'>{confirmationCode}</span></p>
    <p>Введите его, чтобы завершить регистрацию.</p>
    <p class='footer'>Если вы не регистрировались, проигнорируйте это письмо.</p>
</body>
</html>";
        }

        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmRequest confirmRequest)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.email == confirmRequest.Email);

            if (user == null)
            {
                return Unauthorized(new { message = "Пользователь не найден" });
            }
            if (user.createdat < DateTime.UtcNow)
            {
                return Unauthorized(new { message = "Код подтверждения истек" });
            }
            if (user.confirmationcode != confirmRequest.Code)
            {
                return Unauthorized(new { message = "Неверный код подтверждения" });
            }
            user.isconfirmed = true;
            user.confirmationcode = null;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Почта успешно подтверждена" });
        }


    }
}
