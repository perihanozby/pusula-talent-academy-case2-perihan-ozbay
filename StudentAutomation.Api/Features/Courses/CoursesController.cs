using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentAutomation.Api.Domain;
using StudentAutomation.Api.Infrastructure;

namespace StudentAutomation.Api.Features.Courses;

[ApiController]
[Route("api/[controller]")]
public class CoursesController(AppDbContext db) : ControllerBase
{
    private async Task<Teacher?> GetCurrentTeacherAsync()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(sub))
            return null;
        var userId = int.Parse(sub);
        return await db.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
    }

    // ADMIN: kurs oluştur
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateCourseDto dto)
    {
        var teacher = await db.Teachers.FindAsync(dto.TeacherId);
        if (teacher is null)
            return BadRequest("Teacher not found");

        var c = new Course
        {
            Name = dto.Name,
            Code = dto.Code,
            Description = dto.Description,
            TeacherId = dto.TeacherId,
            Status = CourseStatus.Planned,
        };
        db.Courses.Add(c);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = c.Id }, new { c.Id });
    }

    // (opsiyonel) genel detay – herkes görebilir
    [Authorize(Roles = "Admin,Teacher,Student")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var c = await db
            .Courses.Include(x => x.Teacher)
            .ThenInclude(t => t.User)
            .FirstOrDefaultAsync(x => x.Id == id);
        return c is null
            ? NotFound()
            : Ok(
                new
                {
                    c.Id,
                    c.Name,
                    c.Code,
                    c.Status,
                    Teacher = c.Teacher.User.FullName,
                }
            );
    }

    // TEACHER: kendi dersleri
    [Authorize(Roles = "Teacher")]
    [HttpGet("mine")]
    public async Task<IActionResult> Mine()
    {
        var me = await GetCurrentTeacherAsync();
        if (me is null)
            return Forbid();

        var data = await db
            .Courses.Where(c => c.TeacherId == me.Id)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Code,
                c.Status,
            })
            .ToListAsync();

        return Ok(data);
    }

    // TEACHER: durum güncelle (yalnızca kendi dersi)
    [Authorize(Roles = "Teacher")]
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateCourseStatusDto dto)
    {
        var me = await GetCurrentTeacherAsync();
        if (me is null)
            return Forbid();

        var c = await db.Courses.FirstOrDefaultAsync(x => x.Id == id);
        if (c is null)
            return NotFound();
        if (c.TeacherId != me.Id)
            return Forbid();

        if (!Enum.TryParse<CourseStatus>(dto.Status, ignoreCase: true, out var status))
            return BadRequest("Status must be Planned, Started or Finished");

        c.Status = status;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> ListAll([FromQuery] string? q)
    {
        var query = db.Courses.Include(c => c.Teacher).ThenInclude(t => t.User).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(c => c.Name.Contains(q) || c.Code.Contains(q));

        var data = await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new
            {
                c.Id,
                c.Code,
                c.Name,
                c.Status,
                Teacher = c.Teacher.User.FullName,
            })
            .ToListAsync();

        return Ok(data);
    }
}
