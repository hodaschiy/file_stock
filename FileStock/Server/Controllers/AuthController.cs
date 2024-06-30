using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Server.Data;
using Server.Models;
using Server.Services;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace Server.Controllers
{
    [Route("[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ServerContext _context;
        private readonly ICryptService _crypt;

        public AuthController(ServerContext context, ICryptService crypt)
        {
            _context = context;
            _crypt = crypt;
        }

        [HttpGet]
        public string Handshake(string? Login = null) //сделать нормальный проктокол, после авторизации по паролю TODO : клиент генерит ключ => передает серверу PublicKey => сервер генерит токен, шифрует полученным ключем => отправляет клиенту
        {
            var key = _crypt.GetKey();
            var usr = _context.User.Where(usr => usr.Name == Login).FirstOrDefault();

            if (usr != null)
            {
                usr.PublicKey = key;
                _context.Update(usr);
                _context.SaveChanges();
            }

            return key;
        }

        [HttpPost]
        public bool Auth(AuthRequest request) 
        {
            User usr = _context.User.Where(usr => usr.Name == request.Login).FirstOrDefault();
            if (usr == null)
            { 
                return false; 
            }

            byte[] pw = Convert.FromBase64String(request.Password);
            if (Encoding.UTF8.GetString(_crypt.DecryptData(pw, usr.PublicKey)) == Encoding.UTF8.GetString(_crypt.DecryptData(usr.Password))) 
            {
                usr.Token = Encoding.UTF8.GetString(Encoding.Default.GetBytes(DateTime.UtcNow.Date.ToString()));
                _context.Update(usr);
                _context.SaveChanges();
                return true;
            }
            else 
            {
                return false;
            }; 
        }

        [HttpPost]
        public bool Register(RegisterRequest request)
        {
            User? usr = _context.User.Where(usr => usr.Name == request.Login).FirstOrDefault();

            if (usr != null)
            {
                return false;
            }
            byte[] pw = Convert.FromBase64String(request.Password);

            _context.User.Add(new Models.User()
            {
                Name = request.Login,
                Password = _crypt.EncryptData(Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(_crypt.DecryptData(pw, request.PublicKey)))),
                PublicKey = request.PublicKey,
                Token = Encoding.UTF8.GetString(Encoding.Default.GetBytes(DateTime.UtcNow.Date.ToString()))
            });
            _context.SaveChanges();
            return true;
        }

        public class AuthRequest 
        {
            public string Login { get; set; }
            public string Password { get; set; }
        }
        public class RegisterRequest
        {
            public string Login { get; set; }
            public string Password { get; set; }
            public string PublicKey { get; set; }
        }
    }
}
