
using Microsoft.EntityFrameworkCore;
using UserPermissions.Application.Interfaces;
using UserPermissions.Application;
using UserPermissions.Infrastructure.Persistence;
using UserPermissions.Infrastructure.Repositories;
using UserPermissions.Infrastructure.Security;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using System.Reflection;


var builder = WebApplication.CreateBuilder(args);

// EF InMemory
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("users_db"));

// Dependency Injection for the Repositories and Services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "User Permissions", Version = "v1" });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI(opt => { opt.SwaggerEndpoint("/swagger/v1/swagger.json", "User Permissions API V1"); } );
app.UseRouting();

app.MapControllers();

app.Run();

public partial class Program { }

