namespace SecretSanta.Domain.State
{
    public class UserState
    {
        public UserState()
        {
            Auth = new TelegramAuth();
            SantaEvents = new UserSantaEvents();
        }

        public TelegramAuth Auth { get; }

        public UserSantaEvents SantaEvents { get; }
    }
}