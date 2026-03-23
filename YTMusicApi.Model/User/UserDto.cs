namespace YTMusicApi.Model.User
{
    public class UserDto
    {
        public UserDto(Guid id, string userName, string passwordHash, string email, bool isEmailVerified, string? emailVerificationToken, DateTime? emailVerificationTokenExpires)
        {
            Id = id;
            UserName = userName;
            PasswordHash = passwordHash;
            Email = email;
            IsEmailVerified = isEmailVerified;
            EmailVerificationToken = emailVerificationToken;
            EmailVerificationTokenExpires = emailVerificationTokenExpires;
        }
        public Guid Id { get; private set; }
        public string UserName { get; private set; }
        public string PasswordHash { get; private set; }
        public string Email { get; private set; }
        public bool IsEmailVerified { get; private set; }
        public string? EmailVerificationToken { get; private set; }
        public DateTime? EmailVerificationTokenExpires { get; private set; }

        public static UserDto Create(string userName, string passwordHash, string email, string? verificationToken = null, DateTime? tokenExpires = null)
        {
            return new UserDto(Guid.NewGuid(), userName, passwordHash, email, false, verificationToken, tokenExpires);
        }
    }
}
