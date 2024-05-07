namespace Sandbox.Events;

public interface IGameEventListener
{
	void OnEventReceived<T>( T? value );
} 
