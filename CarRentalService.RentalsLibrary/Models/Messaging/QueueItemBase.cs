namespace CarRentalService.RentalsLibrary.Models.Messaging;
public class QueueItemBase<TMessage> where TMessage : QueueMessage
{
    public Guid MessageId { get; set; }
    public QueueItemHost Host { get; set; } = default!;
    public TMessage Message { get; set; } = default!;
}

public abstract class QueueMessage;

public record QueueItemHost(string MachineName, string ProcessName, int ProcessId);
