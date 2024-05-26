using Sandbox.Variables;

namespace Sandbox.ScriptableObjectArchitecture.Variables;

[CustomEditor( typeof( VariableGameResource ) )]

public class VariableGameResourceWidget : GenericGameResourceWidget
{
	public VariableGameResourceWidget(SerializedProperty property) : base(property)
	{
	}
}
