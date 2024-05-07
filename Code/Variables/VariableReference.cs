using System;
using System.Text.Json.Serialization;

namespace Sandbox.Variables;

public class VariableReference<T>
{
	[Tag( "Enum", "Refresh" )] public VariableType ValueToUse { get; set; }

	[Hide] public T? ConstantValue { get; set; } = default;

	[ShowIf( nameof(ValueToUse), VariableType.Variable ), Tag( "Refresh" )]
	public IVariable? Variable { get; set; } = default;

	[Hide, JsonIgnore]
	public bool CanEditValue =>
		(ValueToUse == VariableType.Variable && Variable != null) || ValueToUse == VariableType.Constant;

	[JsonIgnore, HideIf( nameof(CanEditValue), false )]
	public T? Value
	{
		get =>
			ValueToUse switch
			{
				VariableType.Constant => ConstantValue,
				VariableType.Variable when Variable != null => (T?) Variable.RawValue,
				VariableType.Variable when Variable == null => default,
				_ => throw new ArgumentOutOfRangeException( nameof(ValueToUse), ValueToUse, "Invalid ValueToUse." )
			};
		set
		{
			switch ( ValueToUse )
			{
				case VariableType.Constant:
					ConstantValue = value;
					break;
				case VariableType.Variable when Variable != null:
					Variable.RawValue = value;
					break;
				case VariableType.Variable when Variable == null:
					throw new NullReferenceException(
						"Variable is null. Cannot set value. Please assign a variable, or use a constant value." );
				default:
					throw new ArgumentOutOfRangeException( nameof(ValueToUse), ValueToUse, "Invalid ValueToUse." );
			}
		}
	}
}


[Serializable]
public enum VariableType
{
	Constant,
	Variable
}
