using System;
using System.Collections.Generic;
using System.Reflection;

namespace Sandbox.ScriptableObjectArchitecture;

internal class SerializedPropertyWrapper : SerializedProperty
{
	public override string Name => PropertyType.Name;
	public override Type PropertyType => _property.PropertyType;
	public override SerializedObject Parent { get; }

	private PropertyInfo _property;
	private object _value;

	internal SerializedPropertyWrapper( PropertyInfo property, object value , SerializedObject? parent = null)
	{
		_property = property;
		_value = value;
		Parent = parent ?? value.GetSerialized();
	}

	public override void SetValue<T>( T value )
	{
		if ( PropertyType.IsInstanceOfType( value ) )
		{
			_property.SetValue( _value, value );
			NoteChanged();
			return;
		}

		if ( PropertyType.IsEnum )
		{
			_property.SetValue( _value, Enum.ToObject( PropertyType, value ) );
			NoteChanged();
			return;
		}
		
		//if T is a child class of the property type, we can just set it
		if ( PropertyType.IsAssignableFrom( typeof(T) ) )
		{
			_property.SetValue( _value, value );
			NoteChanged();
			return;
		}
		
		_property.SetValue( _value, Convert.ChangeType( value, PropertyType ) );
		NoteChanged();
	}

	public override T GetValue<T>( T defaultValue = default! )
	{
		var value = _property.GetValue( _value );

		if ( value == null )
		{
			return defaultValue;
		}

		if ( value is T genericValue ) // Already correct type
		{
			return genericValue;
		}

		var returnTypeDescriptor = EditorTypeLibrary.GetType<T>();
		if ( returnTypeDescriptor.IsEnum ) // Convert to enum if needed
		{
			if ( value is int intValue )
			{
				return (T)Enum.ToObject( returnTypeDescriptor.GetType(), intValue );
			}

			return (T)Enum.Parse( returnTypeDescriptor.GetType(), value.ToString() ?? string.Empty );
		}

		return (T)Convert.ChangeType( value, typeof(T) ); // Convert to correct type
	}

	public override bool TryGetAsObject( out SerializedObject obj )
	{
		obj = _property.GetValue( _value ).GetSerialized();
		return obj != null;
	}

	public override IEnumerable<Attribute> GetAttributes()
	{
		return _property.GetCustomAttributes();
	}
}
