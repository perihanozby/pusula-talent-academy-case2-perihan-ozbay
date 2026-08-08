using System.Security.Claims;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentAutomation.Api.Domain;
using StudentAutomation.Api.Infrastructure;

namespace StudentAutomation.Api.Features.Students;

[ApiController]
[Route("api/[controller]")]
public class StudentsController(AppDbContext db) : ControllerBase
{
    [Authorize(Roles = "Admin,Teacher")]
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? department,
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20
    )
    {
        var query = db.Students.Include(s => s.User).AsQueryable();
        if (!string.IsNullOrWhiteSpace(department))
            query = query.Where(s => s.Department == department);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(s => s.User.FullName.Contains(q));
        var data = await query
            .OrderBy(s => s.User.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new
            {
                s.Id,
                s.User.FullName,
                s.Number,
                s.Department,
                s.BirthDate,
            })
            .ToListAsync();
        return Ok(data);
    }

    [Authorize(Roles = "Admin,Teacher")]
    [HttpPost]
    public async Task<IActionResult> Create(StudentCreateDto dto)
    {
        if (await db.Users.AnyAsync(u => u.Email == dto.Email))
            return Conflict("Email already exists");
        var user = new User
        {
            Email = dto.Email.Trim().ToLowerInvariant(),
            FullName = dto.FullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("ChangeMe123!"),
            Role = UserRole.Student,
        };
        var student = new Student
        {
            User = user,
            Number = dto.Number,
            Department = dto.Department,
            BirthDate = dto.BirthDate.HasValue
                ? DateTime.SpecifyKind(dto.BirthDate.Value, DateTimeKind.Utc)
                : null,
        };
        db.Students.Add(student);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = student.Id }, new { student.Id });
    }

    [Authorize(Roles = "Admin,Teacher")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var s = await db.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
        return s is null
            ? NotFound()
            : Ok(
                new
                {
                    s.Id,
                    s.User.FullName,
                    s.Department,
                    s.BirthDate,
                }
            );
    }

    [Authorize(Roles = "Admin,Teacher")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, StudentUpdateDto dto)
    {
        var s = await db.Students.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
        if (s is null)
            return NotFound();
        if (!string.IsNullOrWhiteSpace(dto.FullName))
            s.User.FullName = dto.FullName;
        if (!string.IsNullOrWhiteSpace(dto.Number))
            s.Number = dto.Number;
        if (!string.IsNullOrWhiteSpace(dto.Department))
            s.Department = dto.Department;
        if (dto.BirthDate.HasValue)
            s.BirthDate = DateTime.SpecifyKind(dto.BirthDate.Value, DateTimeKind.Utc);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Student")]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = int.Parse(
            User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")!
        );
        var s = await db.Students.Include(x => x.User).FirstOrDefaultAsync(x => x.UserId == userId);
        return s is null
            ? NotFound()
            : Ok(
                new
                {
                    s.Id,
                    s.User.FullName,
                    s.Number,
                    s.Department,
                    s.BirthDate,
                }
            );
    }
}
