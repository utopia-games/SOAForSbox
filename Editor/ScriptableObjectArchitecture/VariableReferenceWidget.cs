using System;
using System.Reflection;
using Editor;
using Sandbox.Variables;

namespace Sandbox.ScriptableObjectArchitecture;

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
				// if enum property or variable property change, rebuild UI
				if ( prop.PropertyType == typeof(VariableType) || prop.PropertyType == typeof(VariableGameResource) )
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
		
		
		// Control panel of VariableType
		PropertyInfo? valueToUseEnumProperty = type.GetProperty( "ValueToUse" );
		if ( valueToUseEnumProperty != null )
		{
			var serializedPropertyInfo = new SerializedPropertyWrapper( valueToUseEnumProperty, _currentValue, SerializedObject );
			var e = Create( serializedPropertyInfo );
			e.MaximumWidth = 80;
			Layout.Add( e ); 
		}
		VariableType valueToUse = (VariableType) valueToUseEnumProperty?.GetValue( _currentValue )!;


		
		// Control panel of VariableGameResource
		PropertyInfo? variableProperty = type.GetProperty( "Variable" );
		if ( valueToUse == Sandbox.Variables.VariableType.Variable && variableProperty != null )
		{
			var serializedPropertyInfo = new SerializedPropertyWrapper( variableProperty, _currentValue, SerializedObject );
			var e = new GenericGameResourceWidget( serializedPropertyInfo );
			_inner.Add( e );
		}
		
		
		
		// Control panel of Value
		PropertyInfo? valueProperty = type.GetProperty( "Value" );
		if ( valueProperty != null && (valueToUse == Sandbox.Variables.VariableType.Constant || valueToUse == Sandbox.Variables.VariableType.Variable && variableProperty?.GetValue( _currentValue ) != default))
		{
			var serializedPropertyInfo = new SerializedPropertyWrapper( valueProperty, _currentValue, SerializedObject );
			var e = Create( serializedPropertyInfo );
			e.MinimumWidth = 100;
			_inner.Add( e );
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
