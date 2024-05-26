using Editor;
using Sandbox.Variables;

namespace Sandbox.ScriptableObjectArchitecture.Variables;


// TODO make a custom editor for VariableGameResource that will be very similar to GameResourceWidget

[CustomEditor( typeof( IVariable ) )]
public class IVariableWidget : ControlWidget
{
	private TypeDescription? InternalVariableType() => TypeLibrary.GetType(SerializedProperty.GetValue<IVariable>()?.InternalTypeName);
	
	private IVariable? Value => SerializedProperty.GetValue<IVariable>();

	public IVariableWidget(SerializedProperty property) : base(property)
	{
		Layout = Layout.Row();
		Layout.Spacing = 3;
		
		BuildUI();
	}

	private void BuildUI()
	{
		var value = SerializedProperty.GetValue<IVariable>();

		if ( value == null )
		{
			SerializedProperty.SetValue( new VariableData<float>(42) );
		}
		else
		{
			var typeProperty = value.GetType().GetProperty( nameof(VariableData<dynamic>.Value) );
			if(typeProperty == null)
				typeProperty = value.GetType().GetProperty( nameof(IVariable.RawValue) );
			
			if (typeProperty != null)
			{
				SerializedPropertyWrapper serializedPropertyWrapper = new SerializedPropertyWrapper( typeProperty, value, SerializedProperty.Parent);
				var e = Create( serializedPropertyWrapper );
				Layout.Add( e );
			}
		}
	}

	protected override void OnDragStart()
	{
		Log.Info( "start");

		base.OnDragStart();
	}

	public override void OnDragDrop( DragEvent ev )
	{
		Log.Info( "Droped");
		base.OnDragDrop( ev );
	}
}
