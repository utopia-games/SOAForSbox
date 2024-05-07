using System.Collections;

namespace Sandbox.Utils;

// Useful extension methods for various types
public static class ExtensionMethods
{
	public static bool IsEmpty(this ICollection c) => c.Count == 0;
	
	public static bool IsEmptyOrNull(this ICollection? c) => c == null || c.Count == 0;
}


public interface IGenericGameResource
{
	public GameResource Resource { get; }
}
