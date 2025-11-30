namespace Domain.Chats;

public record ChatMemberId(Guid Value)
{
    public static ChatMemberId New() => new(Guid.NewGuid());
    public static ChatMemberId Empty => new(Guid.Empty);
    public override string ToString() => Value.ToString();
}
