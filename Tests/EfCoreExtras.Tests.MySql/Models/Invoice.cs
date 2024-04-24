namespace EFCoreExtras.Tests;

public class Invoice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Invoice(Guid id)
    {
        Id = id;
    }

    public Invoice()
    {

    }
}