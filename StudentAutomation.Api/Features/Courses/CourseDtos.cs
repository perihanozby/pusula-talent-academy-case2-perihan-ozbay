namespace StudentAutomation.Api.Features.Courses;

public record CreateCourseDto(string Name, string Code, string? Description, int TeacherId);

public record UpdateCourseStatusDto(string Status);
