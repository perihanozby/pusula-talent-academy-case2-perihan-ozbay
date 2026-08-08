namespace StudentAutomation.Api.Features.Grades;

public record AddGradeDto(int CourseId, int StudentId, int Score, string? Note);
