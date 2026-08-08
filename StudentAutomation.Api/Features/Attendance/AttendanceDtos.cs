namespace StudentAutomation.Api.Features.Attendance;

public record MarkAttendanceDto(
    int CourseId,
    int StudentId,
    DateTime Date,
    bool IsPresent,
    string? Note
);
