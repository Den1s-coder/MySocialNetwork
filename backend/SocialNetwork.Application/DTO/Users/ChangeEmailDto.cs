namespace SocialNetwork.Application.DTO.Users
{
    public record ChangeEmailDto
    {
        public string NewEmail { get; init; }
        public string Password { get; init; }
    }
}
