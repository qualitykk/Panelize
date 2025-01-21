using Editor;
using Sandbox;
using Sandbox.Internal;
using Sandbox.UI;
using System;
using System.Linq;

namespace Panelize;

public class PanelList : Widget
{
	TreeView tree;
	Panel selectedPanel;
	LineEdit filter;
	object hoveredPanel;
	int lastHash;
	public PanelList()
	{
		WindowTitle = "Panels";

		Layout = Layout.Column();
		Layout.Spacing = 4;

		CreateToolBar();

		tree = new TreeView( this );
		tree.ExpandForSelection = true;

		Layout.Add( tree, 1 );

		tree.SelectionOverride = () => Properties.SelectedObject;
		tree.ItemSelected += OnItemSelected;

		tree.ItemHoverEnter += OnHoverNode;
		tree.ItemHoverLeave += o => OnHoverNode( null );
	}

	[EditorEvent.Frame]
	public void Frame()
	{
		if ( !Visible )
			return;

		var session = PanelEditorSession.Current;
		if( session == null)
		{
			lastHash = 0;
			tree.Clear();
			return;
		}

		int hash = session.GetHashCode();
		if ( hash == lastHash )
			return;

		lastHash = hash;
		tree.Clear();
		tree.AddItem( new PanelTreeNode( session.SelectedPanel, session ) );
	}

	private void CreateToolBar()
	{
		return;

		var toolbar = new ToolBar( this );
		toolbar.SetIconSize( 18 );

		filter = new LineEdit( this );
		filter.PlaceholderText = "Filter Panels..";
		filter.TextEdited += OnFilterEdited;
		toolbar.AddWidget( filter );

		Layout.Add( toolbar );
	}

	private void OnFilterEdited( string text )
	{
		tree.Dirty();
	}

	private void OnItemSelected( object obj )
	{
		if ( obj is Panel panel )
		{
			selectedPanel = panel;
			Properties.SelectedObject = panel;
		}
	}
	private void OnHoverNode( object target )
	{
		hoveredPanel = null;

		if ( target is PanelTreeNode node )
			hoveredPanel = node.Value;
	}
}

public class PanelTreeNode : TreeNode<Panel>
{
	private PanelEditorSession session;
	private bool isEditorPanel = true;
	public PanelTreeNode( Panel panel, PanelEditorSession session )
	{
		Value = panel;
		this.session = session;
		//CanEdit = session.IsEditorPanel( panel );
	}
	public override int ValueHash
	{
		get
		{
			if ( !Value.IsValid() ) return 0;

			return HashCode.Combine( Value.Classes, Value.IsVisible, Value.PseudoClass, Value.Children.Sum( x => x.GetHashCode() ) );
		}
	}

	protected override void BuildChildren()
	{
		SetChildren( Value.Children, x => new PanelTreeNode( x, session ) );
	}

	protected override bool HasDescendant( object obj )
	{
		if ( obj is not IPanel iPanel ) return false;
		return iPanel.IsAncestor( Value );
	}

	public override void OnPaint( VirtualWidget item )
	{
		PaintSelection( item );

		var hovered = Value.PseudoClass.HasFlag( PseudoClass.Hover );
		var a = Value.IsVisible ? 1.0f : 0.5f;
		var rect = item.Rect;

		void Write( Color color, string text, ref Rect r )
		{
			Paint.SetPen( color.WithAlphaMultiplied( a ) );
			var size = Paint.DrawText( r, text, TextFlag.LeftCenter );
			r.Left += size.Width;
		}

		{
			//	Paint.SetPen( Theme.Yellow.WithAlpha( alpha ) );
			//	Paint.DrawIcon( r, info.Icon ?? "window", 18, TextFlag.LeftCenter );
			//r.Left += Theme.RowHeight;
		}

		var brackets = Theme.Yellow.WithAlpha( 0.7f );
		var element = Theme.White.WithAlpha( 0.9f );
		var keyword = Theme.White.WithAlpha( 0.7f );
		var valueColor = Theme.Blue;

		if(!isEditorPanel )
		{
			brackets = Theme.Grey.WithAlpha( 0.7f );
			element = Theme.Grey.WithAlpha( 0.9f );
			keyword = Theme.Grey.WithAlpha( 0.7f );
			valueColor = Theme.White;
		}
		else if ( hovered )
		{
			element = Theme.Green.WithAlpha( 0.9f );
			keyword = Theme.Green.WithAlpha( 0.6f );
		}

		Paint.SetDefaultFont();

		{
			Write( brackets, $"<", ref rect );
			Paint.SetDefaultFont( 8, 500 );
			Write( element, $"{Value.ElementName}", ref rect );
			Paint.SetDefaultFont();
		}

		if ( !string.IsNullOrEmpty( Value?.Id ) )
		{
			Write( keyword, $" id=\"", ref rect );
			//Paint.SetDefaultFont( 8, 500 );
			Write( valueColor, Value?.Id, ref rect );
			Paint.SetDefaultFont();
			Write( keyword, $"\"", ref rect );
		}

		if ( !string.IsNullOrEmpty( Value?.Classes ) )
		{
			Write( keyword, $" class=\"", ref rect );
			Write( valueColor, Value?.Classes, ref rect );
			Write( keyword, $"\"", ref rect );
		}

		Write( brackets, $">", ref rect );

		if ( !string.IsNullOrEmpty( Value.SourceFile ) )
		{
			var localFile = System.IO.Path.GetFileName( Value.SourceFile );
			Write( Theme.Green.WithAlpha( 0.5f ), $" {localFile}:{Value.SourceLine} ", ref rect );
		}
	}

	
	public override bool OnContextMenu()
	{
		if ( !isEditorPanel ) return false;

		var menu = new ContextMenu( null );

		menu.AddOption( "Create Child", "add", OpenCreateChildMenu );

		menu.AddOption( "Delete", "delete", DeletePanel );
		var o = menu.AddOption( "Go To Source", action: () => CodeEditor.OpenFile( Value.SourceFile, Value.SourceLine ) );
		o.Enabled = !string.IsNullOrWhiteSpace( Value.SourceFile );
		
		menu.OpenAtCursor();
		return true;
	}

	const float NAME_POPUP_WIDTH = 200f;
	const float NAME_POPUP_HEIGHT = 60f;
	const float NAME_HEADER_HEIGHT = 20f;
	const float NAME_ENTRY_HEIGHT = 20f;
	private void OpenCreateChildMenu()
	{
		float width = NAME_POPUP_WIDTH;

		PopupWidget namePopup = new( TreeView );
		namePopup.Layout = Layout.Column();
		namePopup.FixedWidth = width;
		namePopup.FixedHeight = NAME_POPUP_HEIGHT;

		ScrollArea scrollArea = namePopup.Layout.Add( new ScrollArea( TreeView ), 1 );
		scrollArea.Canvas = new Widget( scrollArea )
		{
			Layout = Layout.Column(),
			HorizontalSizeMode = SizeMode.Expand | SizeMode.CanGrow
		};
		scrollArea.Canvas.Layout.Margin = 8f;
		scrollArea.Canvas.Layout.Spacing = 4f;

		Editor.Label header = new( "Enter panel type:", scrollArea.Canvas )
		{
			FixedHeight = NAME_HEADER_HEIGHT
		};

		LineEdit textInput = new( scrollArea )
		{
			FixedHeight = NAME_ENTRY_HEIGHT
		};
		textInput.PlaceholderText = "div";
		textInput.EditingFinished += namePopup.Close;
		textInput.ReturnPressed += () => CreateChild( textInput.Text );

		scrollArea.Canvas.Layout.Add( header );
		scrollArea.Canvas.Layout.Add( textInput );

		namePopup.Visible = true;
		namePopup.AdjustSize();
		namePopup.ConstrainToScreen();
		namePopup.OpenAtCursor();
		textInput.Focus();
	}

	private void CreateChild( string tag )
	{
		if ( string.IsNullOrEmpty( tag ) )
			tag = "div";

		Panel panel = null;
		if ( tag == "div" || tag == "p" || tag == "span" ) panel ??= new Panel();
		else panel = EditorTypeLibrary.Create<Panel>( tag, false );
		panel ??= new Panel();
		session.AddEditorPanel(panel);
		panel.ElementName = tag;
		panel.Parent = Value;

		TreeView.Open( this );
		//Log.Info( $"Create child with type {tag} for {Value}" );
	}

	private void DeletePanel()
	{
		Value.Delete();
	}
}
