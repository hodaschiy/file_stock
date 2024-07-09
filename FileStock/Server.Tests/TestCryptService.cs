using Microsoft.Extensions.Logging;
using Moq;
using Server.Data;
using Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Server.Tests
{
    [TestClass]
    public class TestsCryptService
    {
        private ICryptService _cryptService;
        private IKeyProvider _keyProvider;


        [TestInitialize]
        public void InitializeTests()
        {
            _keyProvider = new InMemoryKeyProvider();
            _cryptService = new CryptService(_keyProvider, Mock.Of<ILogger<CryptService>>());
        }
        [TestMethod]
        public void GetKeyOptimistic()
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            string strKey = rsa.ToXmlString(false);
            RSACryptoServiceProvider result = new RSACryptoServiceProvider();
            result.FromXmlString(_cryptService.GetKey(strKey));
            Assert.IsTrue(result.PublicOnly);
        }
        [TestMethod]
        public void GetKeyPessimistic()
        {
            string strKey = "incorrect_string";
            Assert.ThrowsException<IsNotCorrectKey>(delegate{ _cryptService.GetKey(strKey); });
        }

        [TestMethod]
        public void EncrypDecryptDataServerKeyOptimistic()
        {
            string subject = "сегодня пишем тесты для криптографического сервиса";
            byte[] subjectByteForm = Encoding.UTF8.GetBytes(subject);
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            string publicUserKey = rsa.ToXmlString(false);

            string publicServerKey = _cryptService.GetKey(publicUserKey);

            byte[] encrypted = _cryptService.EncryptData(subjectByteForm, publicUserKey, false);
            byte[] decrypted = _cryptService.DecryptData(encrypted, publicUserKey, false);

            Assert.AreEqual(subject, Encoding.UTF8.GetString(decrypted));
        }
        [TestMethod]
        public void EncrypDecryptDataUserKeyOptimistic()
        {
            string subject = "сегодня пишем тесты для криптографического сервиса";
            byte[] subjectByteForm = Encoding.UTF8.GetBytes(subject);
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            string publicUserKey = rsa.ToXmlString(false);

            string publicServerKey = _cryptService.GetKey(publicUserKey);

            byte[] encrypted = _cryptService.EncryptData(subjectByteForm, publicUserKey, true);
            byte[] decrypted = rsa.Decrypt(encrypted, false);

            Assert.AreEqual(subject, Encoding.UTF8.GetString(decrypted));
        }
        [TestMethod]
        public void EncrypDecryptDataDBKeyOptimistic()
        {
            string subject = "сегодня пишем тесты для криптографического сервиса";
            byte[] subjectByteForm = Encoding.UTF8.GetBytes(subject);

            byte[] encrypted = _cryptService.EncryptData(subjectByteForm);
            byte[] decrypted = _cryptService.DecryptData(encrypted);

            Assert.AreEqual(subject, Encoding.UTF8.GetString(decrypted));
        }
        [TestMethod]
        public void EncryptDataServerKeyPessimistic()
        {
            string subject = "сегодня пишем тесты для криптографического сервиса";
            byte[] subjectByteForm = Encoding.UTF8.GetBytes(subject);
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            string publicUserKey = rsa.ToXmlString(false);

            Assert.ThrowsException<KeyIsNotExists>(delegate { _cryptService.EncryptData(subjectByteForm, publicUserKey, false); });
        }
        [TestMethod]
        public void EncryptDataUserKeyPessimistic()
        {
            string subject = "сегодня пишем тесты для криптографического сервиса";
            byte[] subjectByteForm = Encoding.UTF8.GetBytes(subject);
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            string publicUserKey = rsa.ToXmlString(false);

            Assert.ThrowsException<KeyIsNotExists>(delegate { _cryptService.EncryptData(subjectByteForm, publicUserKey, true); });
        }
        [TestMethod]
        public void DecryptDataServerKeyPessimistic1()
        {
            string subject = "сегодня пишем тесты для криптографического сервиса";
            byte[] subjectByteForm = Encoding.UTF8.GetBytes(subject);
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            string publicUserKey = rsa.ToXmlString(false);

            byte[] encrypted = rsa.Encrypt(subjectByteForm, false);
            Assert.ThrowsException<KeyIsNotExists>(delegate {_cryptService.DecryptData(encrypted, publicUserKey, false); });
        }
        [TestMethod]
        public void DecryptDataServerKeyPessimistic2()
        {
            string subject = "сегодня пишем тесты для криптографического сервиса";
            byte[] subjectByteForm = Encoding.UTF8.GetBytes(subject);
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            string publicUserKey = rsa.ToXmlString(false);

            string publicServerKey = _cryptService.GetKey(publicUserKey);

            Assert.ThrowsException<IsNotCorrectKey>(delegate { _cryptService.DecryptData(subjectByteForm, publicUserKey, false); });
        }
        [TestMethod]
        public void DecryptDataUserKeyPessimistic1()
        {
            string subject = "сегодня пишем тесты для криптографического сервиса";
            byte[] subjectByteForm = Encoding.UTF8.GetBytes(subject);
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            string publicUserKey = rsa.ToXmlString(false);

            byte[] encrypted = rsa.Encrypt(subjectByteForm, false);
            Assert.ThrowsException<KeyIsNotExists>(delegate { _cryptService.DecryptData(encrypted, publicUserKey, true); });
        }
        [TestMethod]
        public void DecryptDataUserKeyPessimistic2()
        {
            string subject = "сегодня пишем тесты для криптографического сервиса";
            byte[] subjectByteForm = Encoding.UTF8.GetBytes(subject);
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            string publicUserKey = rsa.ToXmlString(false);

            string publicServerKey = _cryptService.GetKey(publicUserKey);

            Assert.ThrowsException<KeyIsNotExists>(delegate { _cryptService.DecryptData(subjectByteForm, publicUserKey, true); });
        }
    }
}
