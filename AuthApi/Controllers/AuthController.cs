using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthApi.Data;
using AuthApi.Models;
using Microsoft.AspNetCore.Identity.Data;
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
                .FirstOrDefaultAsync(u => u.users_email == loginRequest.email);

            if (user == null)
            {
                return Unauthorized(new { message = "Пользователя с таким Email не существует" });
            }
            else if (user.users_password != loginRequest.password)
            {
                return Unauthorized (new { message = "Неверный email или пароль" });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("ТетраГидроКанабиноловаяЯмаЖаждаДозы");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.user_id.ToString()),
            new Claim(ClaimTypes.Email, user.users_email)
                }),
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
        .FirstOrDefaultAsync(u => u.users_email == newUser.users_email);
            if (user != null)
            {
                return Unauthorized(new { message = "Пользователь с таким Email уже существует" });
            }
            newUser.confirmationcode = GenerateConfirmationCode();
            newUser.createdat = DateTime.UtcNow.AddHours(1);
            newUser.isconfirmed = false;

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            Console.WriteLine("В базу внеслось, Письмо создаётся.");
            await SendConfirmationEmail(newUser.users_email, newUser.confirmationcode);
            return Ok(new { message = $"Для подтверждения регистрации, введите код высланный на {user.users_email}" });
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
            /* Твой CSS */
        </style>
    </head>
    <body>
        <h1>Добро пожаловать в Octarine Core!</h1>
        <p>Ваш код подтверждения: <b>{confirmationCode}</b></p>
        <p>Введите его, чтобы завершить регистрацию (тут был ChatGPT).</p>
    </body>
    </html>";
        }
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmRequest confirmRequest)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.users_email == confirmRequest.Email);

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
            user.createdat = null;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Почта успешно подтверждена" });
        }



    }
}
