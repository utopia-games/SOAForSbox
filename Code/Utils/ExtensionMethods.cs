using System.Collections.Generic;

namespace Sandbox.Utils;

// Useful extension methods for various types
public static class ExtensionMethods
{
	public static bool IsEmpty<T>(this ICollection<T> c) => c.Count == 0;
	
	public static bool IsEmptyOrNull<T>(this ICollection<T>? c) => c == null || c.Count == 0;
}


public interface IGenericGameResource
{
	public GameResource Resource { get; }
}
