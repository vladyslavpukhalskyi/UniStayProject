namespace Domain.Chats;

public record ChatMessageId(Guid Value)
{
    public static ChatMessageId New() => new(Guid.NewGuid());
    public static ChatMessageId Empty => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}
