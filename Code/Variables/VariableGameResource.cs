using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sandbox.Variables;

public interface IVariable
{
	string InternalTypeName { get; }
	
	object? RawValue { get; set; }
}

public class VariableData<T> : IVariable, IJsonConvert
{
	
	// Default constructor
	public VariableData() {}
	
	// Constructor with initial value
	public VariableData(T initValue) { SetValue( initValue );}
	
	[JsonIgnore]
	public T? Value
	{
		get => GetValue();
		set => SetValue( value ) ;
	}

	public string InternalTypeName => TypeLibrary.GetType<T>().ToString();

	public object? RawValue
	{
		get => GetValue();
		set
		{
			if ( value is T v )
				SetValue( v );
			else
				throw new InvalidCastException( $"Invalid cast from {value?.GetType()} to {typeof(T)}" );
		}
	}

	private T? _cachedValue = default;
	public void SetValue( T? value ) 
	{
		_cachedValue = value;
	}
	public T? GetValue()
	{
		return _cachedValue;
	}

	static JsonSerializerOptions SerializerOptions => new JsonSerializerOptions
	{
		WriteIndented = true,
		PropertyNameCaseInsensitive = true,
		NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
		ReadCommentHandling = JsonCommentHandling.Skip,
	};
	
	
	
	public static void JsonWrite( object value, Utf8JsonWriter writer )
	{
		VariableData<T> variable = (VariableData<T>) value;
		writer.WriteStartObject();
		writer.WriteString( "InternalType", variable.InternalTypeName );
		if ( variable.RawValue != null )
		{
			switch (variable.RawValue)
			{
				case Resource resource:
					writer.WriteString( "ValuePath", resource.ResourcePath );
					break;
				default:
					writer.WritePropertyName( "RawValue" );
					JsonSerializer.Serialize( writer, variable.RawValue);
					break;
			}
		}
		writer.WriteEndObject();
	}

	public static object? JsonRead( ref Utf8JsonReader reader, Type typeToConvert )
	{
		if (reader.TokenType == JsonTokenType.String)
			return JsonSerializer.Deserialize<T>( reader.GetString() ?? string.Empty );
		
		reader.Read();
		if ( reader.TokenType != JsonTokenType.StartObject )
			return default;
		
		string? internalType = null;
		T? value = default(T);
		while ( reader.Read() )
		{
			if ( reader.TokenType == JsonTokenType.EndObject )
				break;
			if ( reader.TokenType != JsonTokenType.PropertyName )
				continue;
			string? propertyName = reader.GetString();
			reader.Read();
			switch ( propertyName )
			{
				case "InternalType":
					internalType = reader.GetString();
					break;
				case "RawValue":
					value = JsonSerializer.Deserialize<T>( ref reader );
					break;
				case "ValuePath":
					if ( ResourceLibrary.TryGet( reader.GetString(), out GameResource resource ) )
					{
						value = (T?) (object?) resource;
					}
					break;
			}
		}

		if ( internalType == null )
			return null;
		var newObj = TypeLibrary.GetType( internalType ).CreateGeneric<IVariable>();
		(newObj as VariableData<T>)?.SetValue( (T)value! );
		return newObj;
	}
}
 
[GameResource( "Variable", "var", "A Game Variable.", Category = "Variables", Icon = "casino")]
public class VariableGameResource : GameResource
{
	[Property] public IVariable? Variable { get; set; }
	
	[JsonIgnore] public Type VariableType
	{
		set => SetVariableType(value);
		get => TypeLibrary.GetType(Variable?.InternalTypeName).TargetType;
	}
	
	private void SetVariableType( Type value )
	{
		if (  value.Name != Variable?.InternalTypeName )
		{
			Type[] genericArguments = { value };
			var newObj = TypeLibrary.GetType( typeof(VariableData<>) ).CreateGeneric<IVariable>( genericArguments ); 
			Variable = newObj;
		}
	}
	public T? GetValue<T>()
	{
		return Variable?.RawValue is T value ? value : default;
	}

	public void SetValue<T>( T value )
	{
		Variable ??= new VariableData<T>(value);
		
		if ( Variable.InternalTypeName != TypeLibrary.GetType<T>().ToString() )
			Variable = new VariableData<T>(value);

		Variable.RawValue = value;
	}
}
