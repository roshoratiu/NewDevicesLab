namespace NewDevicesLab.Domain.Entities;

public class OrderSheetItem
{
    public Guid Id { get; set; }

    public Guid OrderSheetId { get; set; }

    public OrderSheet OrderSheet { get; set; } = null!;

    public string SiteName { get; set; } = string.Empty;

    public string ComponentName { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public string Link { get; set; } = string.Empty;

    public decimal PriceEuro { get; set; }
}
