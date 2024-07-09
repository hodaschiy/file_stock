using Microsoft.AspNetCore.Authorization;
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
    [AllowAnonymous]
    [Route("[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ServerContext _context;
        private readonly ICryptService _crypt;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ServerContext context, ICryptService crypt, ILogger<AuthController> logger)
        {
            _context = context;
            _crypt = crypt;
            _logger = logger;
        }

        [HttpPost]
        public string Handshake(HandshakeRequest req)
        {
            string clientPublicKey = req.clientPublicKey;
            string? Login = req.Login;


            var key = _crypt.GetKey(clientPublicKey);
            var usr = _context.User.Where(usr => usr.Name == Login).FirstOrDefault();

            if (usr != null)
            {
                usr.PublicKey = clientPublicKey;
                _context.Update(usr);
                _context.SaveChanges();
            }
            //_logger.LogDebug(_context.User.Where(usr => usr.Name == Login).Select(x => x.PublicKey).FirstOrDefault());
            return key;
        }

        [HttpPost]
        public string Auth(AuthRequest request) 
        {
            User usr = _context.User.Where(usr => usr.Name == request.Login).FirstOrDefault();
            if (usr == null)
            { 
                return null; 
            }

            byte[] pw = Convert.FromBase64String(request.Password);
            if (Encoding.UTF8.GetString(_crypt.DecryptData(pw, usr.PublicKey)) == Encoding.UTF8.GetString(_crypt.DecryptData(usr.Password))) 
            {
                byte[] Token = Encoding.UTF8.GetBytes(DateTime.UtcNow.Date.ToString());
                usr.Token = Convert.ToBase64String(_crypt.EncryptData(Token));
                _context.Update(usr);
                _context.SaveChanges();
                return Convert.ToBase64String(_crypt.EncryptData(Token, usr.PublicKey));
            }
            else 
            {
                return null;
            }; 
        }

        [HttpPost]
        public string Register(RegisterRequest request)
        {
            User? usr = _context.User.Where(usr => usr.Name == request.Login).FirstOrDefault();

            if (usr != null)
            {
                return null;
            }
            byte[] pw = Convert.FromBase64String(request.Password);
            byte[] Token = Encoding.UTF8.GetBytes(DateTime.UtcNow.Date.ToString());
            _context.User.Add(new User()
            {
                Name = request.Login,
                Password = _crypt.EncryptData(Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(_crypt.DecryptData(pw, request.PublicKey)))),
                PublicKey = request.PublicKey,
                Token = Convert.ToBase64String(_crypt.EncryptData(Token))
            });
            _context.SaveChanges();
            return Convert.ToBase64String(_crypt.EncryptData(Token, request.PublicKey));
        }

        public class HandshakeRequest
        {
            public string? Login { get; set; }
            public string clientPublicKey { get; set; }
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
