namespace StudentAutomation.Api.Domain;

public class Comment
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course Course { get; set; } = default!;
    public int StudentId { get; set; }
    public Student Student { get; set; } = default!;
    public int TeacherId { get; set; }
    public Teacher Teacher { get; set; } = default!;
    public string Text { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
