using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentAutomation.Api.Infrastructure;

namespace StudentAutomation.Api.Features.Attendance;

[ApiController]
[Route("api/[controller]")]
public class AttendanceController(AppDbContext db) : ControllerBase
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
    [HttpPost("mark")]
    public async Task<IActionResult> Mark(MarkAttendanceDto dto)
    {
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

        var day = dto.Date.Date;
        var rec = await db.Attendances.FirstOrDefaultAsync(a =>
            a.CourseId == dto.CourseId && a.StudentId == dto.StudentId && a.Date == day
        );

        if (rec is null)
        {
            db.Attendances.Add(
                new Domain.Attendance
                {
                    CourseId = dto.CourseId,
                    StudentId = dto.StudentId,
                    Date = day,
                    IsPresent = dto.IsPresent,
                    Note = dto.Note,
                }
            );
            await db.SaveChangesAsync();
            return CreatedAtAction(
                nameof(GetCourseAttendance),
                new { courseId = dto.CourseId },
                null
            );
        }
        else
        {
            rec.IsPresent = dto.IsPresent;
            rec.Note = dto.Note;
            await db.SaveChangesAsync();
            return NoContent();
        }
    }

    [Authorize(Roles = "Teacher")]
    [HttpGet("course/{courseId:int}")]
    public async Task<IActionResult> GetCourseAttendance(
        int courseId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to
    )
    {
        var teacherId = await GetCurrentTeacherIdAsync();
        if (teacherId is null)
            return Forbid();

        var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
        if (course is null)
            return NotFound("Course not found");
        if (course.TeacherId != teacherId.Value)
            return Forbid();

        var q = db
            .Attendances.Where(a => a.CourseId == courseId)
            .Include(a => a.Student)
            .ThenInclude(s => s.User)
            .AsQueryable();

        if (from.HasValue)
            q = q.Where(a => a.Date >= from.Value.Date);
        if (to.HasValue)
            q = q.Where(a => a.Date <= to.Value.Date);

        var data = await q.OrderByDescending(a => a.Date)
            .Select(a => new
            {
                a.StudentId,
                a.Student.User.FullName,
                a.Date,
                a.IsPresent,
                a.Note,
            })
            .ToListAsync();

        return Ok(data);
    }

    [Authorize(Roles = "Student")]
    [HttpGet("me")]
    public async Task<IActionResult> Me([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var studentId = await GetCurrentStudentIdAsync();
        if (studentId is null)
            return Forbid();

        var q = db
            .Attendances.Where(a => a.StudentId == studentId.Value)
            .Include(a => a.Course)
            .AsQueryable();

        if (from.HasValue)
            q = q.Where(a => a.Date >= from.Value.Date);
        if (to.HasValue)
            q = q.Where(a => a.Date <= to.Value.Date);

        var data = await q.OrderByDescending(a => a.Date)
            .Select(a => new
            {
                a.CourseId,
                Course = a.Course.Name,
                a.Date,
                a.IsPresent,
                a.Note,
            })
            .ToListAsync();

        return Ok(data);
    }
}
