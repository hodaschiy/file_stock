using Server.Services;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public byte[]? Password { get; set; }
        public string? Token { get; set; }
        public string PublicKey { get; set; }

        public User() { }
        public User(ICryptService cryptService, string userPublicKey) { PublicKey = cryptService.GetKey(userPublicKey); }
    }

    public class UserAlreadyRegister : Exception { }
}
