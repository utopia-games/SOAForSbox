using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Editor;
using Sandbox.Variables;

namespace Sandbox.CustomInterface;

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

[CustomEditor( typeof(VariableReference<>) )]
public class VariableReferenceWidget : ControlWidget
{
	private Type VariableType => SerializedProperty.PropertyType.GetGenericArguments()[0];
	private TypeDescription TypeDescriptorUnderlaying;
	private SerializedObject? SerializedObject { get;  set; }

	Layout? _inner;
	readonly object? _currentValue;
	
	public VariableReferenceWidget( SerializedProperty property ) : base( property )
	{
		if( property.TryGetAsObject( out var obj ) )
		{
			SerializedObject = obj;
		}
		else
		{
			Log.Warning( "Couldn't get SerializedObject from VariableReference" );
		}
		
		TypeDescriptorUnderlaying = EditorTypeLibrary.GetType( VariableType );
		Layout = Layout.Row();
		Layout.Spacing = 3;

		_currentValue = property.GetValue<object>();

		if ( SerializedObject != null )
		{
			SerializedObject.OnPropertyChanged += ( prop ) =>
			{
				if ( !prop.TryGetAttribute( out TagAttribute tag ) )
					return;

				if ( tag.Value.Any( e => e.ToLowerInvariant() == "refresh" ) )
				{
					BuildUI();
				}
			};
		}
		BuildUI();
	}

	private void BuildUI()
	{
		var type = _currentValue?.GetType();
		if ( _currentValue == null || type == null)
		{
			Log.Warning( "Couldn't get type of VariableReference" );
			return;
		}
		Layout.Clear( true );
		_inner = Layout.AddRow();
		_inner.Spacing = 3;
		foreach ( PropertyInfo propertyInfo in type.GetProperties() )
		{
			var serializedPropertyInfo = new SerializedPropertyWrapper( propertyInfo, _currentValue, SerializedObject);
			if ( !NeedToShowProperty( serializedPropertyInfo, type, _currentValue ) )
				continue;

			var displayInfo = DisplayInfo.ForMember( propertyInfo );
			if ( displayInfo.HasTag( "enum" ) )
			{
				var e = Create( serializedPropertyInfo );
				e.MaximumWidth = 80;
				Layout.Add( e );
			}
			else
			{
				var e = Create( serializedPropertyInfo );
				e.MinimumWidth = 80;
				e.ReadOnly = displayInfo.ReadOnly;
				_inner.Add( e );
			}
		}
	}

	private bool NeedToShowProperty( SerializedPropertyWrapper serializedPropertyInfo, Type type, object current )
	{
		var visibilityAttributes = serializedPropertyInfo.GetAttributes();
		var visible = true;
		try
		{
			foreach ( var visibilityAttribute in visibilityAttributes )
			{
				if( visibilityAttribute is HideAttribute )
				{
					visible = false;
					break;
				}
				if ( visibilityAttribute is HideIfAttribute conditionalVisibilityAttribute )
				{
					var mypropProperty = type.GetProperty( conditionalVisibilityAttribute.PropertyName );
					if ( mypropProperty == null )
						continue;

					var value = mypropProperty.GetValue( current );

					if ( visibilityAttribute is ShowIfAttribute showIfAttribute )
					{
						if(value == null)
						{
							visible = showIfAttribute.Value == value;
						}
						else
						{
							visible = value.Equals( showIfAttribute.Value );
						}
						if ( !visible )
							break;
					}
					else
					{
						if ( value == null  )
						{
							visible = conditionalVisibilityAttribute.Value != null;
						}
						else
						{
							visible = !value.Equals( conditionalVisibilityAttribute.Value );
						}
						
						if ( !visible )
							break;
					}
				}
			}
		}
		catch ( Exception e )
		{
			Log.Warning( e.Message );
		}

		return visible;
	}

	protected override void OnPaint()
	{
		// nothing
	}

}
