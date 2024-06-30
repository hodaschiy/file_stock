using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Client.Controls
{
    public class AuthControl // отрефакторить спагетти
    {
        private RSACryptoServiceProvider _rsa = new RSACryptoServiceProvider();

        public async Task<bool> Authorize(string Login, string Password)
        {
            Handshake(Login);

            JsonContent jsonContent = JsonContent.Create(new { Login = Login, Password = Convert.ToBase64String(_rsa.Encrypt(Encoding.UTF8.GetBytes(Password), false)) });

            var response = await App.http.PostAsync("/Auth", jsonContent);//TODO обработать исключение "ключ не существует"
            var IsAuthorized = await response.Content.ReadAsStringAsync();

            if (Convert.ToBoolean(IsAuthorized))
            {
                App.usr.Name = Login;
                App.usr.Token = Convert.ToBase64String(_rsa.Encrypt(Encoding.Default.GetBytes(DateTime.UtcNow.Date.ToString()), false));

                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> Register(string Login, string Password)
        {
            Handshake(null);

            JsonContent jsonContent = JsonContent.Create(new { Login = Login, Password = Convert.ToBase64String(_rsa.Encrypt(Encoding.UTF8.GetBytes(Password), false)) });

            var response = await App.http.PostAsync("/Register", jsonContent);//TODO обработать исключение "ключ не существует"
            var IsRegistered = await response.Content.ReadAsStringAsync();

            if (Convert.ToBoolean(IsRegistered))
            {
                App.usr.Name = Login;
                App.usr.Token = Convert.ToBase64String(_rsa.Encrypt(Encoding.Default.GetBytes(DateTime.UtcNow.Date.ToString()), false));

                return true;
            }
            else
            {
                return false;
            }
        }

        private void Handshake(string Login)
        {
            string PublicKey;
            if (Login == null)
            {
                PublicKey = App.http.GetStringAsync($"/Handshake?Login={Login}").Result;
            }
            else 
            {
                PublicKey = App.http.GetStringAsync($"/Handshake").Result;
            }

            _rsa.FromXmlString(PublicKey);
        }
    }
}
