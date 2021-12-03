namespace SecretSanta.Domain.State;

public record TelegramLogin(string Login);

public record TelegramId(long Id);

public class TelegramAuth
{
    public event EventHandler? AuthChanged;

    public bool IsLoggedIn => Login != null && Id != null;

    public TelegramLogin? Login { get; private set; }

    public TelegramId? Id { get; private set; }

    public void UpdateLogin(TelegramId id, TelegramLogin login)
    {
        Login = login;
        Id = id;
        AuthChanged?.Invoke(this, new EventArgs());
    }

    public void Logout()
    {
        Id = null;
        Login = null;
        AuthChanged?.Invoke(this, new EventArgs());
    }
}
