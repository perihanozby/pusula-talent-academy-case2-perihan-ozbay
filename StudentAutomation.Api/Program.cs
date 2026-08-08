using System.Text;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudentAutomation.Api.Domain;
using StudentAutomation.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(opt =>
{
    opt.AddPolicy(
        "blazor",
        p =>
            p.AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials()
                .WithOrigins(
                    "http://localhost:5173",
                    "http://localhost:5002",
                    "http://localhost:5001",
                    "http://localhost:5280",
                    "http://localhost:5232"
                )
    );
});

var jwtSection = builder.Configuration.GetSection("jwt");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = key,
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", p => p.RequireRole("Admin"));
    options.AddPolicy("Teacher", p => p.RequireRole("Teacher"));
    options.AddPolicy("Student", p => p.RequireRole("Student"));
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("blazor");
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    if (app.Environment.IsDevelopment())
{
    var adminEmail = app.Configuration["SeedAdmin:Email"];
    var adminPassword = app.Configuration["SeedAdmin:Password"];

    if (
        !string.IsNullOrWhiteSpace(adminEmail)
        && !string.IsNullOrWhiteSpace(adminPassword)
        && !await db.Users.AnyAsync(u => u.Role == UserRole.Admin)
    )
    {
        var admin = new User
        {
            Email = adminEmail.Trim().ToLowerInvariant(),
            FullName = "Local Admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            Role = UserRole.Admin,
        };

        db.Users.Add(admin);
        await db.SaveChangesAsync();
    }
}
}

app.MapControllers();

app.Run();
