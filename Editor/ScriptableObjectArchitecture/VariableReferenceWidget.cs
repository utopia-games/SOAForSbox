using System;
using System.Reflection;
using Editor;
using Sandbox.ScriptableObjectArchitecture.Variables;
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
			AddPropertyToLayout( valueToUseEnumProperty, _currentValue, SerializedObject, Layout, 80, null);
		}
		
		VariableType valueToUse = (VariableType) valueToUseEnumProperty?.GetValue( _currentValue )!;
		
		// Control panel of VariableGameResource
		PropertyInfo? variableProperty = type.GetProperty( "Variable" );
		if ( valueToUse == Sandbox.Variables.VariableType.Variable && variableProperty != null )
		{
			var serializedPropertyInfo = new SerializedPropertyWrapper( variableProperty, _currentValue, SerializedObject );
			var e = new VariableGameResourceWidget( serializedPropertyInfo, TypeDescriptorUnderlaying );
			e.Prime();
			_inner.Add( e );
		}
		
		// Control panel of Value
		PropertyInfo? valueProperty = type.GetProperty( "Value" );
		if ( IsPropertyVisible(valueProperty, valueToUse) )
		{
			AddPropertyToLayout( valueProperty, _currentValue, SerializedObject, _inner, null, 100);
		}
	}
	
	private void AddPropertyToLayout(PropertyInfo? propertyInfo, object currentValue, SerializedObject? serializedObject, Layout layout, float? maxwidth = 80, float? minwidth = 80)
	{
		if (propertyInfo == null)
			return;
		
		var serializedPropertyInfo = new SerializedPropertyWrapper(propertyInfo, currentValue, serializedObject);
		var e = Create(serializedPropertyInfo);
		if(maxwidth != null)
			e.MaximumWidth = (float)maxwidth;
		if(minwidth != null)
			e.MinimumWidth = (float)minwidth;
		layout.Add(e);
	}

	private bool IsPropertyVisible(PropertyInfo? property, VariableType valueToUse)
	{
		return property != null && (valueToUse == Sandbox.Variables.VariableType.Constant || (valueToUse == Sandbox.Variables.VariableType.Variable && property.GetValue(_currentValue) != default));
	}

	protected override void OnPaint() 
	{
		// nothing
	}

}
