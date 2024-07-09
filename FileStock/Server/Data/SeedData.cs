using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Server.Data;
using Server.Services;
using System;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Security.Cryptography;

namespace Server.Models;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        var context = new ServerContext(
            serviceProvider.GetRequiredService<
                DbContextOptions<ServerContext>>());
        var crypt = serviceProvider.GetService<ICryptService>(); 
        
        // Look for any movies.
        if (context.User.Any() || context.FileModel.Any())
        {
            return;   // DB has been seeded
        }
        context.User.AddRange(
            new User(crypt, new RSACryptoServiceProvider().ToXmlString(false))
            {
                Name = "admin"
            }
        );
        context.SaveChanges();
        User usr = context.User.First();
        usr.Password = crypt.EncryptData(Encoding.UTF8.GetBytes("admin"));
        context.Update(usr);
        context.SaveChanges();

        int userID = context.User.First().Id;

        context.FileModel.AddRange(
            new FileModel()
            {
                Name = "test.txt",
                UserID = userID,
                Data = File.ReadAllBytes(@"D:\MyProjects\test.txt")
            },
            new FileModel()
            {
                Name = "test.png",
                UserID = userID,
                Data = File.ReadAllBytes(@"D:\MyProjects\test.png")
            });
        context.SaveChanges();
    }
}

