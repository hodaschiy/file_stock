using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class User
    {
        public string Name { get; set; }
        public string Token { get; set; }
        public RSACryptoServiceProvider serverKey { get; set; }
        public RSACryptoServiceProvider clientKey { get; set; }
        public User() { clientKey = new RSACryptoServiceProvider(); serverKey = new RSACryptoServiceProvider(); }
    }
}
