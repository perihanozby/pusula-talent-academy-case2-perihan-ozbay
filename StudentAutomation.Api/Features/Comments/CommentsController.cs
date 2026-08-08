using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentAutomation.Api.Infrastructure;

namespace StudentAutomation.Api.Features.Comments;

[ApiController]
[Route("api/[controller]")]
public class CommentsController(AppDbContext db) : ControllerBase
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
    public async Task<IActionResult> Add(AddCommentDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Text))
            return BadRequest("Text required");

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

        db.Comments.Add(
            new Domain.Comment
            {
                CourseId = dto.CourseId,
                StudentId = dto.StudentId,
                TeacherId = teacherId.Value,
                Text = dto.Text,
            }
        );
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCourseComments), new { courseId = dto.CourseId }, null);
    }

    [Authorize(Roles = "Teacher")]
    [HttpGet("course/{courseId:int}")]
    public async Task<IActionResult> GetCourseComments(int courseId)
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
            .Comments.Where(c => c.CourseId == courseId)
            .Include(c => c.Student)
            .ThenInclude(s => s.User)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new
            {
                c.Id,
                c.StudentId,
                c.Student.User.FullName,
                c.Text,
                c.CreatedAt,
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
            .Comments.Where(c => c.StudentId == studentId.Value)
            .Include(c => c.Course)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new
            {
                c.CourseId,
                Course = c.Course.Name,
                c.Text,
                c.CreatedAt,
            })
            .ToListAsync();

        return Ok(data);
    }
}
