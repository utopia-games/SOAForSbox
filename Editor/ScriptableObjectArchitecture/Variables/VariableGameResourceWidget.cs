using Editor;
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

	protected override void UpdateFromAsset( Asset? asset )
	{
		if ( asset is null ) return;

		VariableGameResource? resource = asset.LoadResource<VariableGameResource>();
		if ( _forcedType == null || resource.VariableType == _forcedType.TargetType )
		{
			SerializedProperty.SetValue( resource );
		}
	}
}
