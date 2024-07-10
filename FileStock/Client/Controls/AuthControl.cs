using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
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
    public class AuthControl
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public async Task<bool> Authorize(string Login, string Password)
        {
            Handshake(Login, App.usr.clientKey.ToXmlString(false));

            JsonContent jsonContent = JsonContent.Create(new { Login = Login, Password = Convert.ToBase64String(App.usr.serverKey.Encrypt(Encoding.UTF8.GetBytes(Password), false)) });

            var response = await App.http.PostAsync("/Auth", jsonContent);//TODO обработать исключение "ключ не существует"
            var Token = await response.Content.ReadAsStringAsync();

            if (Token != null)
            {
                App.usr.Name = Login;
                var f_step = Convert.FromBase64String(Token);
                var s_step = App.usr.clientKey.Decrypt(f_step, false);
                var t_step = App.usr.serverKey.Encrypt(s_step, false);
                App.usr.Token = Convert.ToBase64String(t_step);

                App.http.DefaultRequestHeaders.Add("Authorization", "basic " + App.usr.Name + ":" + App.usr.Token);
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<string> Register(string Login, string Password)
        {
            Handshake(null, App.usr.clientKey.ToXmlString(false));

            JsonContent jsonContent = JsonContent.Create(new { Login = Login, Password = Convert.ToBase64String(App.usr.serverKey.Encrypt(Encoding.UTF8.GetBytes(Password), false)), PublicKey = App.usr.clientKey.ToXmlString(false) });

            var response = await App.http.PostAsync("/Register", jsonContent);//TODO обработать исключение "ключ не существует"
            var Token = await response.Content.ReadAsStringAsync();

            if (Token != String.Empty)
            {
                try
                {
                    App.usr.Name = Login;
                    App.usr.Token = Convert.ToBase64String(App.usr.serverKey.Encrypt(App.usr.clientKey.Decrypt(Convert.FromBase64String(Token), false), false));
                }
                catch 
                { 
                    return Token; 
                }
                App.http.DefaultRequestHeaders.Add("Authorization", "basic " + App.usr.Name + ":" + App.usr.Token);
                return "true";
            }
            else
            {
                return "Пользователь уже существует";
            }
        }

        private void Handshake(string Login, string clientPublicKey)
        {
            string PublicKey;
            if (Login == null)
            {
                JsonContent jsonContent = JsonContent.Create(new { clientPublicKey });
                PublicKey = App.http.PostAsync($"/Handshake", jsonContent).Result.Content.ReadAsStringAsync().Result;
            }
            else 
            {
                JsonContent jsonContent = JsonContent.Create(new { Login, clientPublicKey });
                PublicKey = App.http.PostAsync($"/Handshake", jsonContent).Result.Content.ReadAsStringAsync().Result;
            }

            App.usr.serverKey.FromXmlString(PublicKey);
        }

        public void Exit() 
        {
            App.usr = new User();
            App.http.DefaultRequestHeaders.Remove("Authorization");
        }
    }
}
