namespace AntdUI;

public interface IEventListener
{
	void HandleEvent(EventType id, object? tag);
}
