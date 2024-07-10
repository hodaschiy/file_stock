using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Web.Http.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Server.Controllers;
using Server.Models;
using Server.Data;
using Server.Tests.TestServices;
using Microsoft.Extensions.Logging;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace Server.Tests
{
    [TestClass]
    public class TestsAuthController
    {
        private Mock<ServerContext> _mockContext;
        private AuthController _authController;

        [TestInitialize]
        public void InitializeTests()
        {
            var users = new List<User>()
            {
                new User()
                {
                    Id = 1,
                    Name = "Login",
                    Password = Encoding.UTF8.GetBytes("Password"),
                    Token = Convert.ToBase64String(  Encoding.UTF8.GetBytes(  DateTime.UtcNow.Date.ToString() )   ),
                    PublicKey = "good"
                }
            }.AsQueryable();

            var files = new List<FileModel>()
            { 
                new FileModel()
                {
                    Id = 1,
                    Data = Encoding.UTF8.GetBytes("Сегодня хорошая погода"),
                    Name = "fl1.str",
                    UserId = 1
                },
                new FileModel()
                {
                    Id = 2,
                    Data = Encoding.UTF8.GetBytes("Погода и правда не плохая"),
                    Name = "fl2.str",
                    UserId = 1
                }
            }.AsQueryable();

            var mockUsr = new Mock<DbSet<User>>();
            var mockFlMdl = new Mock<DbSet<FileModel>>();

            mockUsr.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
            mockUsr.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
            mockUsr.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
            mockUsr.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(() => users.GetEnumerator());

            mockFlMdl.As<IQueryable<FileModel>>().Setup(m => m.Provider).Returns(files.Provider);
            mockFlMdl.As<IQueryable<FileModel>>().Setup(m => m.Expression).Returns(files.Expression);
            mockFlMdl.As<IQueryable<FileModel>>().Setup(m => m.ElementType).Returns(files.ElementType);
            mockFlMdl.As<IQueryable<FileModel>>().Setup(m => m.GetEnumerator()).Returns(() => files.GetEnumerator());


            _mockContext = new Mock<ServerContext>(new DbContextOptions<ServerContext>());
            _mockContext.Setup(m => m.User).Returns(mockUsr.Object);
            _mockContext.Setup(m => m.FileModel).Returns(mockFlMdl.Object);

            _authController = new AuthController(_mockContext.Object, new MockCryptService(), Mock.Of<ILogger<AuthController>>());
        }

        [TestMethod]
        public void TestTestContext1() 
        {
            var usr = _mockContext.Object.User.Where(x => x.Name == "Login").First();
            usr.PublicKey = "not_good";
            _mockContext.Object.Update(usr);
            _mockContext.Object.SaveChanges();

            Assert.AreEqual("not_good", _mockContext.Object.User.Where(x => x.Name == "Login").Select(x => x.PublicKey).First());
        }
        [TestMethod]
        public void TestTestContext2()
        {
            string? Password = Convert.ToBase64String(Encoding.UTF8.GetBytes("Password"));
            Assert.AreEqual(Password, Convert.ToBase64String(_mockContext.Object.User.Select(x => x.Password).FirstOrDefault()));
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [TestMethod]
        public void TestAuthOptimistic()
        {
            string Token = Convert.ToBase64String(  Encoding.UTF8.GetBytes(  DateTime.UtcNow.Date.ToString() )   );
            string Password = Convert.ToBase64String(Encoding.UTF8.GetBytes("Password"));
            Assert.AreEqual(Token, _authController.Auth(new AuthController.AuthRequest() { Login = "Login", Password = Password }));
        }

        [TestMethod]
        public void TestAuthPessimistic1()
        {
            string Password = Convert.ToBase64String(Encoding.UTF8.GetBytes("Wrong_Password"));
            Assert.AreEqual(null, _authController.Auth(new AuthController.AuthRequest() { Login = "Login", Password = Password }));
        }
        [TestMethod]
        public void TestAuthPessimistic2()
        {
            string Password = Convert.ToBase64String(Encoding.UTF8.GetBytes("Password"));
            Assert.AreEqual(null, _authController.Auth(new AuthController.AuthRequest() { Login = "Wrong_Login", Password = Password }));
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////
        [TestMethod]
        public void TestRegisterOptimistic()
        {
            string Token = Convert.ToBase64String(Encoding.Default.GetBytes(DateTime.UtcNow.Date.ToString()));
            Assert.AreEqual(Token, _authController.Register(new AuthController.RegisterRequest() { Login = "Login1", Password = "Password", PublicKey = "good" }));
        }

        [TestMethod]
        public void TestRegisterPessimistic()
        {
            string Token = Convert.ToBase64String(Encoding.Default.GetBytes(DateTime.UtcNow.Date.ToString()));
            Assert.AreEqual(null, _authController.Register(new AuthController.RegisterRequest() { Login = "Login", Password = "Password", PublicKey = "good" }));
        }
    }
}