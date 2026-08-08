namespace StudentAutomation.Api.Domain;

public class Grade
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course Course { get; set; } = default!;
    public int StudentId { get; set; }
    public Student Student { get; set; } = default!;
    public int Score { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
