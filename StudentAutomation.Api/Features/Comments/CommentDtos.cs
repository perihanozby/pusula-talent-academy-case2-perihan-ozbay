namespace StudentAutomation.Api.Features.Comments;

public record AddCommentDto(int CourseId, int StudentId, string Text);
