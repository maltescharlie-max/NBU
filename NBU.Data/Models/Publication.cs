namespace NBU.Data.Models;

public sealed class Publication
{
    public int Id { get; set; }
    public string Type { get; set; } = "News";
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EventDate { get; set; }

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
