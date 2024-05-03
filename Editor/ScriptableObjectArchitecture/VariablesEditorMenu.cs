using System;
using System.Collections.Generic;
using System.Linq;
using Editor;
using Sandbox.UI;
using Button = Editor.Button;
using Label = Editor.Label;

namespace Sandbox.ScriptableObjectArchitecture;

public static class VariablesEditorMenu
{
	public static void OpenCreationDialogForSpecificType( string fileNameText, string extension, Action<Asset?>? onAssetCreated = null )
	{
		Action<string, string> createNew = ( string s, string ext ) =>
		{
			Asset? asset = AssetSystem.CreateResource( ext, s );
			MainAssetBrowser.Instance?.UpdateAssetList();
			onAssetCreated?.Invoke( asset );
		};
		
		if(string.IsNullOrWhiteSpace( fileNameText ) )
			fileNameText = "MyVariable";
		
		if ( Project.Current is {} project )
		{
			var fd = new FileDialog( null )
			{
				Title = $"Create new variable", Directory = project.GetAssetsPath(), DefaultSuffix = $".{extension}", WindowTitle = "Create new variable"
				
			};
			fd.SelectFile( $"{fileNameText}.{extension}" );
			fd.SetFindFile();
			fd.SetModeSave();
			fd.SetNameFilter( $"Variable File (*.{extension})"  );
			if ( !fd.Execute() )
				return;
			
			createNew( fd.SelectedFile, extension );
		}
	}

	public static void OpenCreationDialogForSpecificTypes( List<AssetType> typeList, Action<Asset?>? onAssetCreated = null)
	{
		var window = new Dialog
		{
			WindowTitle = "New Variable", DeleteOnClose = true, Window = { Size = new Vector2(256, 100) }, Layout = Layout.Column()
		};

		window.Layout.Margin = 16;
		window.Layout.Spacing = 8;

		var label = new Label( "Select Variable type" );
		window.Layout.Add( label );

		
		var comboBox = new ComboBox();
		var currentlySelected = typeList.FirstOrDefault();
		
		foreach (AssetType variableType in typeList)
		{
			comboBox.AddItem( $"{variableType.FriendlyName} (*.{variableType.FileExtension})", onSelected: () => currentlySelected = variableType );
		}
		window.Layout.Add( comboBox );
		
		
		var nameFile = window.Layout.AddRow();
		nameFile.Add( new Label( "FileName" ) );
		var fileName = new LineEdit();
		nameFile.Add( fileName );
		nameFile.Spacing = 4;
		fileName.PlaceholderText = "MyVariable";

		var row = window.Layout.AddRow();
		row.AddStretchCell();
		row.Add( new Button( "Abort" ) { MouseClick = window.Close } );
		row.AddSpacingCell( 4f );
		row.Add( new Button.Primary( "Create" ) { MouseClick = () =>
			{
				if ( currentlySelected != null )
				{
					OpenCreationDialogForSpecificType( fileName.Text, currentlySelected.FileExtension, onAssetCreated);
				}
				window.Close();
			}
		} );

		window.AdjustSize();
		window.Show();
		window.ConstrainToScreen();
	}


	[Menu( "Editor", "Variables/New ..." )]
	static void OpenTypeChoice()
	{
		var availableTypes = GetVariableTypes();
		OpenCreationDialogForSpecificTypes( availableTypes, asset =>
		{
			MainAssetBrowser.Instance?.FocusOnAsset( asset );
			EditorUtility.InspectorObject = asset;
		} );
	}
	
	public static List<AssetType> GetVariableTypes()
	{
		var genericType = typeof(Variable<>);

		bool Predicate( AssetType x ) => x.ResourceType != null && x.ResourceType.IsBasedOnGenericType( genericType ) && x.FileExtension != "errored";
		
		var linkAssetTypes = AssetType.All
			.Where(Predicate)
			.OrderBy(x => x.FriendlyName)
			.ToList();
		return linkAssetTypes;
	}
}
