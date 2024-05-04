using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.ScriptableObjectArchitecture;
using Sandbox.Utils;
using Sandbox.Variables;

namespace Editor;

// Notice: Works for now, just copied what was done for GameResources and changed it a bit to work for Variables
// May need to be refactored later as it must contain a lot of duplicated and unnecessary code

[CustomEditor( typeof( Variable<> ) )]
public class VariableWidget : ControlWidget
{
	List<AssetType> _assetTypes = new();

	IconButton? _previewButton = null;

	public override bool IsControlButton => true;
	public override bool SupportsMultiEdit => true;
	
	private TypeDescription _underlayingType;

	public VariableWidget( SerializedProperty property ) : base( property )
	{
		if ( AssetType.FromType( property.PropertyType ) is {} type )
			_assetTypes.Add( type );
		
		foreach (AssetType assetType in AssetType.All)
		{
			if ( assetType.ResourceType == null )
				continue;
			if( assetType.ResourceType.BaseType == property.PropertyType)
				_assetTypes.Add(assetType);
		}

		HorizontalSizeMode = SizeMode.CanGrow | SizeMode.Expand;
		Cursor = CursorShape.Finger;
		MouseTracking = true;
		AcceptDrops = true;
		IsDraggable = true;
		_underlayingType = EditorTypeLibrary.GetType( SerializedProperty.PropertyType.GenericTypeArguments[0] );
	}

	protected override void DoLayout()
	{
		base.DoLayout();

		if ( _previewButton is null )
			return;

		_previewButton.FixedSize = Height - 2;
		_previewButton.Position = new Vector2( Width - Height + 1, 1 );
	}

	protected override void PaintControl()
	{
		var resource = SerializedProperty.GetValue<Resource>();
		var asset = resource != null ? AssetSystem.FindByPath( resource.ResourcePath ) : null;

		bool hovered = IsUnderMouse;
		if ( !Enabled ) hovered = false;

		Paint.Antialiasing = true;
		Paint.TextAntialiasing = true;

		var rect = LocalRect.Shrink( 6, 0 );
		var h = Size.y;

		
		Paint.ClearPen();
		Paint.SetBrush( Theme.ControlBackground );
		Paint.DrawRect( LocalRect, Theme.ControlRadius );

		// icon box
		Paint.ClearPen();
		Paint.SetBrush( Theme.Yellow.Darken( hovered ? 0.7f : 0.8f ).Desaturate( 0.8f ) );
		Paint.DrawRect( new Rect( 0, 0, h, h ).Grow( -1 ), Theme.ControlRadius - 1.0f);

		Paint.SetPen( Theme.Yellow.Darken( hovered ? 0.0f : 0.1f ).Desaturate( hovered ? 0.0f : 0.2f ) );
		

		var pickerName = DisplayInfo.ForType( SerializedProperty.PropertyType ).Name;
		if ( !_assetTypes.IsEmpty() )
		{
			pickerName = string.Join( ", ", _assetTypes.Select( x => x.FriendlyName ) );
		}
		rect.Left += 22;

		if ( SerializedProperty.IsMultipleDifferentValues )
		{
			var textRect = rect.Shrink( 0, 3 );
			//if ( icon != null ) Paint.Draw( iconRect, icon );

			Paint.SetDefaultFont();
			Paint.SetPen( Theme.MultipleValues );
			rect.Left += 22;
			Paint.DrawText( textRect, $"Multiple Values", TextFlag.LeftCenter );
		}
		else if ( asset is not null && !asset.IsDeleted )
		{
			
			Paint.SetPen( Theme.Yellow );
			Paint.SetFont( "Poppins", 9, 450 );
			Paint.DrawText( new Rect( 1, h - 1 ), _underlayingType.Title[0].ToString(), TextFlag.Center );
			
			var textRect = rect.Shrink( 0, 3 );

			Paint.SetPen( Color.White.WithAlpha( 0.9f ) );
			Paint.SetFont( "Poppins", 8, 450 );
			var t = Paint.DrawText( textRect, $"{asset.Name}", TextFlag.LeftTop );
			textRect.Left = t.Right + 6;
			Paint.SetDefaultFont( 7 );
			Theme.DrawFilename( textRect, asset.RelativePath, TextFlag.LeftCenter, Color.White.WithAlpha( 0.5f ) );
		}
		else if ( resource != null )
		{
			Paint.SetPen( Theme.Yellow );
			Paint.SetFont( "Poppins", 9, 450 );
			Paint.DrawText( new Rect( 1, h - 1 ), _underlayingType.Title[0].ToString(), TextFlag.Center );
			
			var textRect = rect.Shrink( 0, 3 );

			Paint.SetPen( Color.White.WithAlpha( 0.9f ) );
			Paint.SetFont( "Poppins", 8, 450 );
			var t = Paint.DrawText( textRect, $"Unknown {pickerName}", TextFlag.LeftTop );

			textRect.Left = t.Right + 6;
			Paint.SetDefaultFont( 7 );
			Theme.DrawFilename( textRect, resource.ResourcePath, TextFlag.LeftCenter, Color.White.WithAlpha( 0.5f ) );
		}
		else if(!_assetTypes.IsEmpty())
		{
			Paint.SetPen( Theme.Grey.Darken( hovered ? 0.0f : 0.1f ).Desaturate( hovered ? 0.0f : 0.2f ) );
			Paint.SetFont( "Poppins", 9, 450 );
			Paint.DrawText( new Rect( 1, h - 1 ), _underlayingType.Title[0].ToString(), TextFlag.Center );
			
			var textRect = rect.Shrink( 0, 3 );
			
			Paint.SetDefaultFont( italic: true );
			Paint.SetPen( Color.White.WithAlpha( 0.2f ) );
			Paint.DrawText( textRect, $"{pickerName}", TextFlag.LeftCenter );
		}
		else
		{
			Paint.SetPen( Theme.Red.Desaturate( hovered ? 0.0f : 0.2f ) );
			Paint.SetFont( "Poppins", 9, 450 );
			Paint.DrawText( new Rect( 1, h - 1 ), _underlayingType.Title[0].ToString(), TextFlag.Center );
			
			var textRect = rect.Shrink( 0, 3 );
			
			Paint.SetDefaultFont();
			Paint.SetPen( Color.Red.WithAlpha( 0.5f ) );
			Paint.DrawText( textRect, $"There is no Variable Asset type for this type: '{_underlayingType.Title}'", TextFlag.LeftCenter );
		}
	}

	protected override void OnContextMenu( ContextMenuEvent e )
	{

		var resource = SerializedProperty.GetValue<Resource>();
		var asset = (resource != null) ? AssetSystem.FindByPath( resource.ResourcePath ) : null;

		if ( _assetTypes.IsEmpty() )
		{
			DisplayWarningNoTypeAvailable();
		}
		else
		{
			var m = new Menu();
			m.AddOption( "Open in Editor", "edit", () => asset?.OpenInEditor() ).Enabled = asset != null;
			m.AddSeparator();
			m.AddOption( "Copy", "file_copy", action: Copy ).Enabled = asset != null;
			m.AddOption( "Paste", "content_paste", action: Paste );
			m.AddSeparator();
			m.AddOption( "Clear", "backspace", action: Clear ).Enabled = resource != null;
			m.AddSeparator();
			m.AddOption( "Create Variable asset","add", action: CreateNewAsset).Enabled = !ReadOnly && _assetTypes.Count > 0;
			m.AddOption( "Find exising variable", "search", () => OnMouseClick( new MouseEvent() ) );
			m.OpenAtCursor();
		}
	}

	private void CreateNewAsset()
	{
		void OnAssetCreated( Asset? asset )
		{
			UpdateFromAsset( asset );
			SignalValuesChanged();
		}

		if ( _assetTypes.Count == 1 )
		{
			AssetType assetType = _assetTypes.First();
			VariablesEditorMenu.OpenCreationDialogForSpecificType( assetType.FriendlyName, assetType.FileExtension, OnAssetCreated);
		}
		else
		{

			VariablesEditorMenu.OpenCreationDialogForSpecificTypes( _assetTypes, OnAssetCreated);
		}
	}
	
	private void DisplayWarningNoTypeAvailable()
	{
		EditorUtility.DisplayDialog( "Warning", $"There is no Variable Asset type available for this type: '{_underlayingType.Title}'. \nYou must likely just need to implement a class inheriting from Variable<{_underlayingType.ClassName}>\nwith the GameRessource attribute" );
	}

	protected override void OnMouseClick( MouseEvent e )
	{
		base.OnMouseClick( e );

		if ( ReadOnly ) return;

		if ( _assetTypes.IsEmpty() )
		{
			DisplayWarningNoTypeAvailable();
			return;
		}

		var resource = SerializedProperty.GetValue<Resource>();
		Asset? asset = resource != null ? AssetSystem.FindByPath( resource.ResourcePath ) : null; 

		var pickerName = DisplayInfo.ForType( SerializedProperty.PropertyType ).Name;

		var picker = new AssetPicker( this, _assetTypes )
		{
			Window =
			{
				StateCookie = "ResourceProperty",
				Title = $"Select {pickerName} ({_underlayingType.Title})"
			}
		};
		picker.Window.RestoreFromStateCookie();
		if ( asset != null )
		{
			picker.Assets = new List<Asset> { asset };
		}
		picker.OnAssetHighlighted = ( o ) => UpdateFromAsset( o.FirstOrDefault() );
		picker.OnAssetPicked = ( o ) => UpdateFromAsset( o.FirstOrDefault() );
		picker.Window.Show();
	}

	private void UpdateFromAsset( Asset? asset )
	{
		if ( asset is null ) return;

		var resource = asset.LoadResource( SerializedProperty.PropertyType );
		SerializedProperty.SetValue( resource );
	}

	public override void OnDragHover( DragEvent ev )
	{
		// This might be a cloud asset - EDIT: No? 
		if ( ev.Data.Url?.Scheme == "https" )
		{
			ev.Action = DropAction.Link;
			return;
		}

		if ( !ev.Data.HasFileOrFolder )
			return;

		var asset = AssetSystem.FindByPath( ev.Data.FileOrFolder );

		if ( asset == null || !_assetTypes.Contains( asset.AssetType ) )
			return;

		ev.Action = DropAction.Link;
	}

	public override void OnDragDrop( DragEvent ev )
	{
		if ( ev.Data.Url?.Scheme == "https" )
		{
			_ = DroppedUrl( ev.Data.Text );
			ev.Action = DropAction.Link;
			return;
		}

		if ( !ev.Data.HasFileOrFolder )
			return;

		var asset = AssetSystem.FindByPath( ev.Data.FileOrFolder );

		if ( asset is null || !_assetTypes.Contains( asset.AssetType) )
			return;

		UpdateFromAsset( asset );
		ev.Action = DropAction.Link;
	}

	async Task DroppedUrl( string identUrl )
	{
		var asset = await AssetSystem.InstallAsync( identUrl );

		if ( asset is null || _assetTypes.Contains(asset.AssetType) )
			return;

		UpdateFromAsset( asset );
	}

	protected override void OnDragStart()
	{
		var resource = SerializedProperty.GetValue<Resource>();
		var asset = resource != null ? AssetSystem.FindByPath( resource.ResourcePath ) : null;

		if ( asset == null )
			return;

		var drag = new Drag( this );
		drag.Data.Url = new System.Uri( $"file://{asset.AbsolutePath}" );
		drag.Execute();
	}

	void Copy()
	{
		var resource = SerializedProperty.GetValue<Resource>();
		if ( resource == null ) return;

		var asset = AssetSystem.FindByPath( resource.ResourcePath );
		if ( asset == null ) return;

		EditorUtility.Clipboard.Copy( asset.Path );
	}

	void Paste()
	{
		var path = EditorUtility.Clipboard.Paste();
		var asset = AssetSystem.FindByPath( path );
		UpdateFromAsset( asset );
	}

	void Clear()
	{
		SerializedProperty.SetValue<Resource>( null! );
	}
	
	
}
