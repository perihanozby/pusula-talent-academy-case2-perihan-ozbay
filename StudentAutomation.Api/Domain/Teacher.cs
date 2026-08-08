namespace StudentAutomation.Api.Domain;

public class Teacher
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = default!;
    public string Title { get; set; } = "";
    public string Bio { get; set; } = "";

    public ICollection<Course> Courses { get; set; } = new List<Course>();
    public ICollection<Comment> CommentsWritten { get; set; } = new List<Comment>();
}
