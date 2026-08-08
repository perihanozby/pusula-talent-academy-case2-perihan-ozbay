namespace StudentAutomation.Api.Features.Teachers;

public record TeacherCreateDto(string FullName, string Email, string Password, string Title, string? Bio);

public record TeacherUpdateDto(string? FullName, string? Title, string? Bio);
