namespace StudentAutomation.Api.Features.Students;

public record StudentCreateDto(
    string FullName,
    string Email,
    string Number,
    DateTime? BirthDate,
    string Department
);

public record StudentUpdateDto(
    string FullName,
    string? Number,
    DateTime? BirthDate,
    string? Department
);
