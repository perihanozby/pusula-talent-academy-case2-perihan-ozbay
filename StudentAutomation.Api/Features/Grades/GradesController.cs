using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentAutomation.Api.Infrastructure;

namespace StudentAutomation.Api.Features.Grades;

[ApiController]
[Route("api/[controller]")]
public class GradesController(AppDbContext db) : ControllerBase
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

    private async Task<int?> GetCurrentStudentIdAsync()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(sub))
            return null;
        var userId = int.Parse(sub);
        var s = await db.Students.FirstOrDefaultAsync(s => s.UserId == userId);
        return s?.Id;
    }

    [Authorize(Roles = "Teacher")]
    [HttpPost]
    public async Task<IActionResult> Add(AddGradeDto dto)
    {
        if (dto.Score < 0 || dto.Score > 100)
            return BadRequest("Score must be 0-100");

        var teacherId = await GetCurrentTeacherIdAsync();
        if (teacherId is null)
            return Forbid();

        var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == dto.CourseId);
        if (course is null)
            return NotFound("Course not found");
        if (course.TeacherId != teacherId.Value)
            return Forbid();

        var enrolled = await db.Enrollments.AnyAsync(e =>
            e.CourseId == dto.CourseId && e.StudentId == dto.StudentId
        );
        if (!enrolled)
            return BadRequest("Student is not enrolled in this course");

        db.Grades.Add(
            new Domain.Grade
            {
                CourseId = dto.CourseId,
                StudentId = dto.StudentId,
                Score = dto.Score,
                Note = dto.Note,
            }
        );
        await db.SaveChangesAsync();
        return CreatedAtAction(
            nameof(GetCourseGrades),
            new { courseId = dto.CourseId },
            new { dto.CourseId, dto.StudentId }
        );
    }

    [Authorize(Roles = "Teacher")]
    [HttpGet("course/{courseId:int}")]
    public async Task<IActionResult> GetCourseGrades(int courseId)
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
            .Grades.Where(g => g.CourseId == courseId)
            .Include(g => g.Student)
            .ThenInclude(s => s.User)
            .OrderBy(g => g.Student.User.FullName)
            .Select(g => new
            {
                g.StudentId,
                g.Student.User.FullName,
                g.Score,
                g.Note,
                g.CreatedAt,
            })
            .ToListAsync();

        return Ok(data);
    }

    [Authorize(Roles = "Student")]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var studentId = await GetCurrentStudentIdAsync();
        if (studentId is null)
            return Forbid();

        var data = await db
            .Grades.Where(g => g.StudentId == studentId.Value)
            .Include(g => g.Course)
            .OrderByDescending(g => g.CreatedAt)
            .Select(g => new
            {
                g.CourseId,
                Course = g.Course.Name,
                g.Score,
                g.Note,
                g.CreatedAt,
            })
            .ToListAsync();

        return Ok(data);
    }
}
