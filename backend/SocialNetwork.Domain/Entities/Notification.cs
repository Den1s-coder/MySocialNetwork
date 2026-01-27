namespace SocialNetwork.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public string Message { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public bool IsRead { get; set; } = false;

    }
}
