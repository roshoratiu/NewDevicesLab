namespace NewDevicesLab.Domain.Entities;

public class OrderSheet
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    public Guid CreatedByUserId { get; set; }

    public AppUser CreatedByUser { get; set; } = null!;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? SubmittedAtUtc { get; set; }

    public ICollection<OrderSheetItem> Items { get; set; } = new List<OrderSheetItem>();
}
