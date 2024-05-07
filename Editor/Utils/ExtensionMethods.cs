using System;
using System.Collections.Generic;
using Editor;

namespace Sandbox.Utils;

public static class ExtensionMethods
{
	public static List<AssetType> GetRelatedAssetTypes( this Type type )
	{
		var result = new List<AssetType>();
		
		if ( AssetType.FromType( type ) is {} t )
			result.Add( t );
		
		foreach (AssetType assetType in AssetType.All)
		{
			if ( assetType.ResourceType == null )
				continue;
			if ( assetType.ResourceType.IsAssignableTo( type ) )
			{
				result.Add(assetType);
			}
		}
		return result;
	}
}
