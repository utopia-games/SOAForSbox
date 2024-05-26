using Sandbox.Variables;

namespace Sandbox.ScriptableObjectArchitecture.Variables;

[CustomEditor( typeof( VariableGameResource ) )]

public class VariableGameResourceWidget : GenericGameResourceWidget
{
	TypeDescription? _forcedType;
	public VariableGameResourceWidget(SerializedProperty property) : base(property)
	{
	}
	
	public VariableGameResourceWidget(SerializedProperty property, TypeDescription _forcedType) : base(property)
	{
		this._forcedType = _forcedType;
	}
}
