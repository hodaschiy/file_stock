using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Models;
using Server.Services;

namespace Server.Data
{
    public class ServerContext : DbContext
    {
        public ServerContext (DbContextOptions<ServerContext> options)
            : base(options)
        {
        }
        public DbSet<User> User { get; set; } = default!;
        public DbSet<FileModel> FileModel { get; set; } = default!;
    }
}
