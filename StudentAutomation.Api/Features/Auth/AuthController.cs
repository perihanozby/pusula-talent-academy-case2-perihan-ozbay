using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudentAutomation.Api.Domain;
using StudentAutomation.Api.Infrastructure;

namespace StudentAutomation.Api.Features.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext db, IConfiguration cfg) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        if (!new[] { "Teacher", "Student" }.Contains(req.Role))
            return BadRequest("Role must be Teacher or Student");

        var exists = await db.Users.AnyAsync(u => u.Email == req.Email);
        if (exists)
            return Conflict("Email already exists");

        var user = new User
        {
            Email = req.Email.Trim().ToLowerInvariant(),
            FullName = req.FullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role = Enum.Parse<UserRole>(req.Role),
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        if (user.Role == UserRole.Student)
            db.Students.Add(
                new Student
                {
                    UserId = user.Id,
                    Number = $"S{user.Id:D5}",
                    Department = "General",
                }
            );
        else
            db.Teachers.Add(
                new Teacher
                {
                    UserId = user.Id,
                    Title = "Teacher",
                    Bio = "",
                }
            );
        await db.SaveChangesAsync();
        return CreatedAtAction(
            nameof(Register),
            new { user.Id },
            new
            {
                user.Id,
                user.Email,
                user.Role,
            }
        );
    }

    /////////////////////////////////////////////////////////
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == req.Email);
        if (user is null)
            return Unauthorized();
        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized();

        var jwt = BuildToken(user, cfg);
        return new AuthResponse(jwt, user.FullName, user.Role.ToString());
    }

    private static string BuildToken(User user, IConfiguration cfg)
    {
        var jwt = cfg.GetSection("jwt");
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("fullName", user.FullName),
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
