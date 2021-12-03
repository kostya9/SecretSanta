namespace SecretSanta.Domain.State;

public class UserSantaEvents
{
    public event EventHandler? SantaEventsChanged;

    private readonly List<SecretSantaEvent> _events;

    public IEnumerable<SecretSantaEvent> Events => _events;

    public UserSantaEvents()
    {
        _events = new List<SecretSantaEvent>();
    }

    public void AddEvent(SecretSantaEvent santaEvent)
    {
        _events.Add(santaEvent);
        SantaEventsChanged?.Invoke(this, new EventArgs());
    }

    public void RemoveEvent(SecretSantaEvent santaEvent)
    {
        _events.Remove(santaEvent);
        SantaEventsChanged?.Invoke(this, new EventArgs());
    }

    public void Clear()
    {
        _events.Clear();
        SantaEventsChanged?.Invoke(this, new EventArgs());
    }
}
