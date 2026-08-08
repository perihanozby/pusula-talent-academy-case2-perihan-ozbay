namespace StudentAutomation.Api.Domain;

public class Attendance
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course Course { get; set; } = default!;
    public int StudentId { get; set; }
    public Student Student { get; set; } = default!;
    public DateTime Date { get; set; } = DateTime.UtcNow.Date;
    public bool IsPresent { get; set; }
    public string? Note { get; set; }
}
