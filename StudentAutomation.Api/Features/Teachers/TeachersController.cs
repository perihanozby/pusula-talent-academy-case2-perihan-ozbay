using System.Security.Claims;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentAutomation.Api.Domain;
using StudentAutomation.Api.Infrastructure;

namespace StudentAutomation.Api.Features.Teachers;

[ApiController]
[Route("api/[controller]")]
public class TeachersController(AppDbContext db) : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? q)
    {
        var query = db.Teachers.Include(t => t.User).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(t => t.User.FullName.Contains(q));
        var data = await query
            .OrderBy(t => t.User.FullName)
            .Select(t => new
            {
                t.Id,
                t.User.FullName,
                t.User.Email,
                t.Title,
                t.Bio,
            })
            .ToListAsync();
        return Ok(data);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(TeacherCreateDto dto)
    {
        if (await db.Users.AnyAsync(u => u.Email == dto.Email))
            return Conflict("Email already exists");

        var u = new User
        {
            Email = dto.Email.Trim().ToLowerInvariant(),
            FullName = dto.FullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("ChangeMe123!"),
            Role = UserRole.Teacher,
        };
        var t = new Teacher
        {
            User = u,
            Title = dto.Title,
            Bio = dto.Bio ?? "",
        };

        db.Teachers.Add(t);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = t.Id }, new { t.Id });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var t = await db.Teachers.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
        return t is null
            ? NotFound()
            : Ok(
                new
                {
                    t.Id,
                    t.User.FullName,
                    t.User.Email,
                    t.Title,
                    t.Bio,
                }
            );
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, TeacherUpdateDto dto)
    {
        var t = await db.Teachers.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
        if (t is null)
            return NotFound();
        if (!string.IsNullOrWhiteSpace(dto.FullName))
            t.User.FullName = dto.FullName;
        if (!string.IsNullOrWhiteSpace(dto.Title))
            t.Title = dto.Title;
        if (!string.IsNullOrWhiteSpace(dto.Bio))
            t.Bio = dto.Bio;

        await db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var t = await db.Teachers.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
        if (t is null)
            return NotFound();

        db.Teachers.Remove(t);
        db.Users.Remove(t.User);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
