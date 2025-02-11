using Domain.Users;

namespace Domain.Messages
{
    public class Message
    {
        public MessageId Id { get; }
        public UserId SenderId { get; private set; }
        
        public User? Sender { get; }
        public UserId ReceiverId { get; private set; }
        
        public User? Receiver { get; }
        public string Text { get; private set; }
        public DateTime SendDate { get; private set; }

        private Message(MessageId id, UserId senderId, UserId receiverId, string text, DateTime sendDate)
        {
            Id = id;
            SenderId = senderId;
            ReceiverId = receiverId;
            Text = text;
            SendDate = sendDate;
        }

        public static Message New(MessageId id, UserId senderId, UserId receiverId, string text)
            => new(id, senderId, receiverId, text, DateTime.UtcNow);

        public void UpdateText(string text)
        {
            Text = text;
            SendDate = DateTime.UtcNow;
        }
    }
}