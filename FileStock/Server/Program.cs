using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Server.Services;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ServerContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ServerContext") ?? throw new InvalidOperationException("Connection string 'ServerContext' not found.")));

// Add services to the container.

builder.Services.AddControllers();
/*builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,
        options => builder.Configuration.Bind("JwtSettings", options));*/

builder.Services.AddSingleton<IKeyProvider, InMemoryKeyProvider>().AddSingleton<ICryptService, CryptService>();
builder.Services.AddTransient<IArchiverService, ArchiverService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    SeedData.Initialize(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

//app.UseAuthentication();

app.MapControllers();



app.Run();
