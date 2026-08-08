using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentAutomation.Api.Infrastructure;

namespace StudentAutomation.Api.Features.Enrollments;

[ApiController]
[Route("api/[controller]")]
public class EnrollmentsController(AppDbContext db) : ControllerBase
{
    private async Task<int?> GetCurrentTeacherIdAsync()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(sub))
            return null;
        var userId = int.Parse(sub);
        var t = await db.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
        return t?.Id;
    }

    // TEACHER: derse öğrenci ekle
    [Authorize(Roles = "Teacher")]
    [HttpPost]
    public async Task<IActionResult> Add(EnrollmentAddDto dto)
    {
        var teacherId = await GetCurrentTeacherIdAsync();
        if (teacherId is null)
            return Forbid();

        var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == dto.CourseId);
        if (course is null)
            return NotFound("Course not found");
        if (course.TeacherId != teacherId.Value)
            return Forbid();

        var studentExists = await db.Students.AnyAsync(s => s.Id == dto.StudentId);
        if (!studentExists)
            return NotFound("Student not found");

        var exists = await db.Enrollments.AnyAsync(e =>
            e.CourseId == dto.CourseId && e.StudentId == dto.StudentId
        );
        if (exists)
            return Conflict("Already enrolled");

        db.Enrollments.Add(
            new Domain.Enrollment { CourseId = dto.CourseId, StudentId = dto.StudentId }
        );
        await db.SaveChangesAsync();
        return CreatedAtAction(
            nameof(ListStudents),
            new { courseId = dto.CourseId },
            new { dto.CourseId, dto.StudentId }
        );
    }

    // TEACHER: dersten öğrenci çıkar
    [Authorize(Roles = "Teacher")]
    [HttpDelete]
    public async Task<IActionResult> Remove([FromQuery] int courseId, [FromQuery] int studentId)
    {
        var teacherId = await GetCurrentTeacherIdAsync();
        if (teacherId is null)
            return Forbid();

        var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
        if (course is null)
            return NotFound("Course not found");
        if (course.TeacherId != teacherId.Value)
            return Forbid();

        var enr = await db.Enrollments.FirstOrDefaultAsync(e =>
            e.CourseId == courseId && e.StudentId == studentId
        );
        if (enr is null)
            return NotFound("Enrollment not found");

        db.Enrollments.Remove(enr);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // TEACHER: dersin öğrencileri
    [Authorize(Roles = "Teacher")]
    [HttpGet("{courseId:int}/students")]
    public async Task<IActionResult> ListStudents(int courseId)
    {
        var teacherId = await GetCurrentTeacherIdAsync();
        if (teacherId is null)
            return Forbid();

        var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
        if (course is null)
            return NotFound("Course not found");
        if (course.TeacherId != teacherId.Value)
            return Forbid();

        var data = await db
            .Enrollments.Where(e => e.CourseId == courseId)
            .Include(e => e.Student)
            .ThenInclude(s => s.User)
            .OrderBy(e => e.Student.User.FullName)
            .Select(e => new
            {
                e.StudentId,
                e.Student.User.FullName,
                e.Student.Number,
                e.Student.Department,
            })
            .ToListAsync();

        return Ok(data);
    }
}
