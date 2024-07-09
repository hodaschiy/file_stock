using System.CodeDom.Compiler;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using Swashbuckle.AspNetCore.SwaggerGen;
using NLog;

namespace Server.Services
{
    public interface ICryptService
    {
        public byte[]? EncryptData(byte[] data, string? PublicKey = null, bool userKey = true);
        public byte[]? DecryptData(byte[] data, string? PublicKey = null, bool userKey = false);
        public string GetKey(string clientPublicKey);
    }

    public interface IKeyProvider
    {
        public RSACryptoServiceProvider GetKey(KeyType Type);
    }
    
    public class CryptService : ICryptService
    {
        private readonly IKeyProvider _keyProvider;
        private readonly ILogger<CryptService> _logger;

        private RSACryptoServiceProvider DatabaseKey { get => _keyProvider.GetKey(KeyType.DB); }
        private Dictionary<RSACryptoServiceProvider, RSACryptoServiceProvider> UserKeys = new Dictionary<RSACryptoServiceProvider, RSACryptoServiceProvider>(); //user generated key - server generated key pair

        public CryptService(IKeyProvider keyProvider, ILogger<CryptService> logger) { _keyProvider = keyProvider; _logger = logger; /*UserKeys.Add(_keyProvider.GetKey(KeyType.Client));*/ }
        public byte[]? EncryptData(byte[] data, string? PublicKey = null, bool userKey = false) 
        {
            if (PublicKey == null)
            {
                return DatabaseKey.Encrypt(data, false);
            }
            var keyPair = UserKeys.Where(k => k.Key.ToXmlString(false) == PublicKey).FirstOrDefault();
            RSACryptoServiceProvider key;
            if (userKey)
            {
                key = keyPair.Key;
            }
            else
            {
                key = keyPair.Value;
            }

            if (key == null)
            {
                throw new KeyIsNotExists();
            }

            if (key != null)
            {
                return key.Encrypt(data, false);
            }

            return null;
        }
        public byte[]? DecryptData(byte[] data, string? PublicKey = null, bool userKey = false)
        {
            try
            {
                if (PublicKey == null)
                {
                    return DatabaseKey.Decrypt(data, false);
                }
            }
            catch
            {
                throw new IsNotCorrectKey("Ключ базы данных не подходит"); // ключ базы не подходит
            }
            var keyPair = UserKeys.Where(k => k.Key.ToXmlString(false) == PublicKey).FirstOrDefault();
            RSACryptoServiceProvider key;
            if (userKey)
            {
                key = keyPair.Key;
            }
            else
            {
                key = keyPair.Value;
            }

            if (key == null)
            {
                throw new KeyIsNotExists();
            }

            if (key != null) 
            {
                try
                {
                    return key.Decrypt(data, false);
                }
                catch (Exception ex)
                {
                    if (!key.PublicOnly) 
                        throw new IsNotCorrectKey(ex.Message);
                    else
                        throw new KeyIsNotExists(ex.Message);
                }
            }
            
            return null;
        }
        public string GetKey(string clientPublicKey) 
        {
            RSACryptoServiceProvider userKey = new RSACryptoServiceProvider();
            try
            {
                userKey.FromXmlString(clientPublicKey);
            }
            catch (Exception ex)
            {
                throw new IsNotCorrectKey(ex.Message);
            }

            if (!UserKeys.Select(x => x.Key).ToList().Contains(userKey))
            {
                var key = _keyProvider.GetKey(KeyType.Client);
                UserKeys.Add(userKey, key);
            }

            return UserKeys[userKey].ToXmlString(false);
        }
    }

    public class InMemoryKeyProvider : IKeyProvider // реализация статических ключей
    {
        private RSACryptoServiceProvider dbkey = new RSACryptoServiceProvider(); 
        private List<RSACryptoServiceProvider> keys = new List<RSACryptoServiceProvider>();
        public RSACryptoServiceProvider GetKey(KeyType Type)
        {
            if (Type == KeyType.DB)
            {
                return dbkey;
            }
            else 
            {
                return keys.First();
            }
        }

        private void GenerateKeys()
        {
            dbkey.FromXmlString("<RSAKeyValue><Modulus>73dVOQx6QybQy+FlHobkEga/1aQuWyJiS2dB639vX+/KEOVjwgkDYTNlc1AbUeB8VnyTWA0GkPfsj5UewjS1+zl5oNCK/chS8ZTz8e9Mj16KbqBpfR/7fgEKte7JVWeK3LgUyuTaoP9SUhbFtCB0IySuqa/j5padG83oDHthC9JDWaYVynNGrDowzvgQH7poYrxINs0WpQ227jQbbBSo1VbtU/OUhNvxoxA7hLFy0ShBfJX/Bd18WMnfF8LbSjs4qYV8nmt7imc+N3gJzv1ywkLfbQfczwmZO8+/emMo1YoB5NE9rqZU0/zyJ4MUOI6m8uHxjnT8QPZ312YFD8yOhQ==</Modulus><Exponent>AQAB</Exponent><P>+2ijhIX10lsDpezP6E2psWq1Bk618LTTy1yy5dFZqFfSR8dIRciPfVdQXIk7KmVqh7skyuSkJFYaI2Vr87Seg2QmNRKgU1JC8EyHAykGSSoVL3cvlMXY3BX4qLyJVBLnJqKh0i/o9PZ6IMOdeK7kI0+yk9SlImTT28Gb1LtXWgc=</P><Q>89bcfeYMbEhXBu9MOviuTF1fNiyGZvIRxN2ZjAgW/TF4uZtCNCYS5fKUUI0qgo97P9o6N/8csJFXyme1YP2qDLzgHzxAY7oBiu6mItVx86OiJA4JlWlaXhHxPsIbamhRXyFxSZvTuvNjSqT8a/JkA4WsIxBwKwyOOgkBMdZwIBM=</Q><DP>X9SaB8jcU+uXb2beNSltVpBbImVcWr1CwhC6rHlpWI7pO60NsnPpphhjLHKu19FCkyxVsPUIUEV1vypIxOODTIgfPDm2XSxNEbXJ/P1lRPxCiQ5AV9A2gmXpm8AKBL8T1xlE346dmMpe8SA/ZSIdGgymAE2dCIgwbWLWWHd0q38=</DP><DQ>YuuomZ6pLYiPXa3YtBfMxggfJyAbCkpr8PyhXRCvGtpmCo405mkHw6eMib0rILpKmtXSRoNFRVBBWEiIaaeew/ofxiwwRwTfuVglp/4Fc3zPu2bCdo297mnC/93JN3rIgBpWFjGUTMSOZGrYSIaWEmO5OKRjjONUE7ExfeFrrWc=</DQ><InverseQ>IFvrlpilyuvXx0UzFGNFiRMGPVSoOEG2EyEowWRVksJuSTF8g4/ZRqjmyDXwhGqs9sxSRYkVmyRojPmj5V6V8ccAAEZMp8lLfcLcD3i7ipyBc+pALPu/RcWWYVzuhPUioEXy+P+frfRT+ixjYYyvk9A6zt7XUWWyDCJe/0Udqkc=</InverseQ><D>rhn0vCV2PpxZ8eFNw4QPDYwarRlRGrFFBc1s/+sG7plIMECW5tvmuSTE/bGgK0crHzYzajEONRgLxdiFJG1uYdjHtUPpydl7dGj3c06oOxRhUU0PZQl7r4DSaLPTbWR6cT7k3x7EVYlMMKKd2vg9d8+TrYtUBrI6R5zMuUZ6qDwu6VQFgQRs8waOyGNdMC99kjPbU+NI+T/345TqlNLqEsAYKsxfqBjwhaRFSauNjzZFqvUvGx36H03xAHGviYnDPno0tKAPfCWJ3iQUveYb9FAAx+hfthYFylGxVjZ63G+h76xMts1oOU/yh5rCHxoCy3I9lCcc0Y7wOptna/gFgQ==</D></RSAKeyValue>");
            keys.Add(new RSACryptoServiceProvider());
            keys.First().FromXmlString("<RSAKeyValue><Modulus>trAcH3VKYEe+I1uKCnBSE3FIYxzUH9TJ6Ka0CgVTXzQFaFNBlSvXXUVwyCByTCLOtys1kkh2BZ2URqKuyoaEV0jdi7ReS0oFDnV2zX2UCVO7RnixrruNftjLSoPS7bYeNYV6xqz6pEiNjjz0M9BRysCmlpWnAv5U/8RK2X4rRzB8AnmUaEA2eoZNJDFbo104Y2+60noa+O3woT1qNgc48LQcJw+bi+rECbjIgTMHgbqnQZ7on/WzTLAyiTuGWfpaJ2tknpdpuBJo2CzMKu/n0aFGEHN6FmL07tsddv5MkYLjokGAZhOjsTblWAX9+HFMKLQVpFmdOvSueHRktHalaQ==</Modulus><Exponent>AQAB</Exponent><P>5/NrndyoGsgQxADy9vSHmh5k/zG2zCbRRlRmKYVMQPGolhd9wtOxKSFSk9JfNbDl9nTB3O/3BHlJlrTbl2ePWQmBHIjyLg8W/LeVbRMGqyoDMgJLyjPZBfWMEkIDLdQ1iumV3Zl7CuMDjgXCAMABVxV896Oy0g15g5l0CSEFZ3s=</P><Q>yaEff1CI50Z2H4oyl5dwHse5t/pmxsShPwbcCTE4kmO2Q9JS67ZhT/jZfYKyJuXHDfS0fIE/TnXVTQnphOvju/c3PEiLRnpyHACwYA9aEr/mxOZ1oHX5y+9qSG6tIIHBNbft4TF0z22Qe0qIu4jMqUtnVerSor17hHc+/Ikdn2s=</Q><DP>tdMds1v78zKN4fgUff5mJixZY6rm7tYnCwiyJS42/lnxm+bCUO19XQ6gGvy4YfBP9IjGR5lcfAdP5nHUCbXGqzdGHMZFglGV5XlMC3r0qUx/wL4IB3PpVkCMEuvobg6CAmjlcBcWWwxmBvkAgXICGu/fTIjobrzdWon6agWWpO0=</DP><DQ>A6YYzdMFRzotNRDpLXkeHFB8+elcJsi0KlHTdY8gePW4/K03tvBL/oiAVxcgZjfOTGBbS82C+caEH6rIQRGLw4ELzNl7O8FHg1430uU0Ohai9Hk/M7Iu3RPuFnV6SJZQispTUbn1ZTOUY8bLhqlJGt80dYeUXEGVlKYby7IniRU=</DQ><InverseQ>dnWznZOW5oSnlNLbKtW6nKLH800EabEzyjlhjFPEY9wb4hHbS+PAeyUILQQh0+8Bn7AQhGMdmE/cEgYv7jvYmUQw3mtGhjdOUSCDDMrFXY9gX0woScFMXdYauZNE7iOm+aVq9wJL2AldnVAv5zohB0qrFu0pFy/OfG3j1ii+PGY=</InverseQ><D>N1JhUc1s9ncDRyXDSaH12NlnOExUmEcR1ZgV0SsHsj7InG0J4ShjkWUj4BajRhcMTj+Re3jh9JVhf3poQAA4N05jeJjoLRh/K3+6uEOc/q4Seia2d9ln4c+40BnXWBDcWQzQvzVGuVUXOMmQkdg1zJZGXDd5Wdi22LUSjYY9iNLMjjm2dXYHQ+UWjzgZpUohL0vHNSu0L5D+TTTj+4L7TeyloHJbx0E+gAg2t/CCF6gLdjXiFh/yCKOmHn15Yfy4nh1+J2AC9VKzptWGyYrA/+Bs4fJz5UZItghGS1L2+AVZ3DGaXWaa5t6qjtTP8WYb3qg3SRT0IeZi9+yA4VmtGQ==</D></RSAKeyValue>");
        }

        public InMemoryKeyProvider() { GenerateKeys(); }
    }

    public class KeyProvider : IKeyProvider // TODO: должен хранить ключи в базе доступной только в локальнрой сети
    {
        public RSACryptoServiceProvider GetKey(KeyType Type) { return new RSACryptoServiceProvider(); }
    }

    public enum KeyType
    {
        Client = 0,
        DB = 1
    }

    public class KeyIsNotExists : Exception { public KeyIsNotExists() : base() { } public KeyIsNotExists(string? message) : base(message) { } }
    public class IsNotCorrectKey : Exception { public IsNotCorrectKey() : base() { } public IsNotCorrectKey(string? message) : base(message) { } }
    public class NotImplimented : Exception { public NotImplimented() : base() { } public NotImplimented(string? message) : base(message) { } }
}
