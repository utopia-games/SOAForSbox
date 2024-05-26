using System;
using System.Text.Json.Serialization;

namespace Sandbox.Variables;

public class VariableReference<T>
{
	public VariableType ValueToUse { get; set; }
	public T? ConstantValue { get; set; } = default;
	public VariableGameResource? Variable { get; set; } = default;

	[JsonIgnore]
	public T? Value
	{
		get =>
			ValueToUse switch
			{
				VariableType.Constant => ConstantValue,
				VariableType.Variable when Variable != null => Variable.GetValue<T>(),
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
					Variable.SetValue( value );
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
