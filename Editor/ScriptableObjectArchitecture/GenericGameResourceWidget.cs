using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Editor;
using Sandbox.Utils;

namespace Sandbox.ScriptableObjectArchitecture;

[CustomEditor( typeof( IGenericGameResource ) )]
public class GenericGameResourceWidget: ControlWidget
{
	private readonly List<AssetType> _assetTypes;

	readonly IconButton? _previewButton;

	public override bool IsControlButton => true;
	public override bool SupportsMultiEdit => true;

	public GenericGameResourceWidget( SerializedProperty property ) : base( property )
	{
		_assetTypes = property.PropertyType.GetRelatedAssetTypes();

		HorizontalSizeMode = SizeMode.CanGrow | SizeMode.Expand;
		Cursor = CursorShape.Finger;
		MouseTracking = true;
		AcceptDrops = true;
		IsDraggable = true;

		if ( _assetTypes.Contains( AssetType.SoundFile ) )
		{
			_previewButton = new PreviewButton( property );
			_previewButton.Background = Theme.Blue;
			_previewButton.Foreground = Theme.White;
			_previewButton.Icon = "volume_up";
			_previewButton.Parent = this;
			_previewButton.OnClick = () => EditorUtility.PlayAssetSound( property.GetValue<SoundFile>() );
		}
	}

	protected override void DoLayout()
	{
		base.DoLayout();

		if ( _previewButton is not null )
		{
			_previewButton.FixedSize = Height - 2;
			_previewButton.Position = new Vector2( Width - Height + 1, 1 );
		}
	}

	protected override void PaintControl()
	{
		var resource = SerializedProperty.GetValue<Resource>();
		var asset = resource != null ? AssetSystem.FindByPath( resource.ResourcePath ) : null;

		var rect = new Rect( 0, Size );

		var iconRect = rect.Shrink( 2 );
		iconRect.Width = iconRect.Height;

		rect.Left = iconRect.Right + 10;

		Paint.ClearPen();
		Paint.SetBrush( Theme.Grey.WithAlpha( 0.2f ) );
		Paint.DrawRect( iconRect, 2 );

		var pickerName = DisplayInfo.ForType( SerializedProperty.PropertyType ).Name;
		if ( !_assetTypes.IsEmptyOrNull() )
		{
			pickerName = string.Join( ", ", _assetTypes.Select( x => x.FriendlyName ) );
		}

		Pixmap? icon = _assetTypes.Select( e => e.Icon64 ).FirstOrDefault();

		if ( SerializedProperty.IsMultipleDifferentValues )
		{
			var textRect = rect.Shrink( 0, 3 );
			if ( icon != null ) Paint.Draw( iconRect, icon );

			Paint.SetDefaultFont();
			Paint.SetPen( Theme.MultipleValues );
			Paint.DrawText( textRect, $"Multiple Values", TextFlag.LeftCenter );
		}
		else if ( asset is not null && !asset.IsDeleted )
		{
			Paint.Draw( iconRect, asset.GetAssetThumb() );

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
			var textRect = rect.Shrink( 0, 3 );

			if ( icon != null ) Paint.Draw( iconRect, icon );
			Paint.SetPen( Color.White.WithAlpha( 0.9f ) );
			Paint.SetFont( "Poppins", 8, 450 );
			var t = Paint.DrawText( textRect, $"Unknown {pickerName}", TextFlag.LeftTop );

			textRect.Left = t.Right + 6;
			Paint.SetDefaultFont( 7 );
			Theme.DrawFilename( textRect, resource.ResourcePath, TextFlag.LeftCenter, Color.White.WithAlpha( 0.5f ) );
		}
		else
		{
			var textRect = rect.Shrink( 0, 3 );
			if ( icon != null ) Paint.Draw( iconRect, icon );

			Paint.SetDefaultFont( italic: true );
			Paint.SetPen( Color.White.WithAlpha( 0.2f ) );
			Paint.DrawText( textRect, $"{pickerName}", TextFlag.LeftCenter );
		}
	}

	protected override void OnContextMenu( ContextMenuEvent e )
	{
		var m = new Editor.Menu();

		var resource = SerializedProperty.GetValue<Resource>();
		var asset = (resource != null) ? AssetSystem.FindByPath( resource.ResourcePath ) : null;

		m.AddOption( "Open in Editor", "edit", () => asset?.OpenInEditor() ).Enabled = asset != null;
		m.AddSeparator();
		m.AddOption( "Copy", "file_copy", action: Copy ).Enabled = asset != null;
		m.AddOption( "Paste", "content_paste", action: Paste );
		m.AddSeparator();
		m.AddOption( "Clear", "backspace", action: Clear ).Enabled = resource != null;

		m.OpenAtCursor();
	}

	protected override void OnMouseClick( MouseEvent e )
	{
		base.OnMouseClick( e );

		if ( ReadOnly ) return;

		var resource = SerializedProperty.GetValue<Resource>();
		var asset = resource != null ? AssetSystem.FindByPath( resource.ResourcePath ) : null;

		var pickerName = DisplayInfo.ForType( SerializedProperty.PropertyType ).Name;

		var picker = new AssetPicker( this, _assetTypes )
		{
			Window =
			{
				StateCookie = "ResourceProperty",
				Title = $"Select {pickerName}"
			}
		};
		
		picker.Window.RestoreFromStateCookie();
		if(asset != null)
		{
			picker.Assets = new List<Asset>() { asset };
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
		// This might be a cloud asset
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

		if ( asset is null || !_assetTypes.Contains( asset.AssetType ))
			return;

		UpdateFromAsset( asset );
		ev.Action = DropAction.Link;
	}

	async Task DroppedUrl( string identUrl )
	{
		var asset = await AssetSystem.InstallAsync( identUrl );

		if ( asset is null || !_assetTypes.Contains( asset.AssetType ) )
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
		SerializedProperty.SetValue( (Resource)null! );
	}
}

file class PreviewButton : IconButton
{
	private SerializedProperty _property;

	public PreviewButton( SerializedProperty property ) : base( "people" )
	{
		this._property = property;
	}
}
