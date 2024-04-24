namespace EFCoreExtras.Tests;

public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int Quantity { get; set; } = 10;
}

public class ItemString
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = null!;
    public int Quantity { get; set; } = 10;
}
