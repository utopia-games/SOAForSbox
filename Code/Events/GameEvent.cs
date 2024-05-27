using System;
using System.Collections.Generic;
using Sandbox.Events;
using Sandbox.Utils;

namespace Sandbox;

public interface IGameEvent
{
	void Raise( object value );

	public void RegisterListener( IGameEventListener listener );

	public void UnregisterListener( IGameEventListener listener );
}

public class GameEvent<T> : GameResource, IGameEvent, IGenericGameResource
{
	public GameResource Resource { get; }

	[Property]
	public string testToSeeIfThisIsIncluded { get; set; } = "This is a test";
	
	private List<IGameEventListener> listeners = new();

	protected GameEvent()
	{
		Resource = this;
	}

	public void Raise(T? value)
	{
		for (var i = listeners.Count - 1; i >= 0; --i)
		{
			listeners[i].OnEventReceived( value );
		}
	}

	public void Raise( object value )
	{
		if( value is T e )
		{
			Raise( e );
		}
		else
		{
			throw new InvalidCastException( $"Invalid cast from {value.GetType()} to {typeof(T)}" );
		}
	}

	public void RegisterListener( IGameEventListener listener ) 
	{
		listeners.Add( listener );
	}

	public void UnregisterListener( IGameEventListener listener )
	{
		listeners.Remove( listener );
	}

}

