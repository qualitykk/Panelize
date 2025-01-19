using Editor.NodeEditor;
using Sandbox.UI;
using System.Linq;
using Button = Editor.Button;

namespace Panelize;

public class PanelStyleEditor : Widget
{
	Panel panel;
	PanelEditorSession session;
	Layout editor;
	Button addRuleButton;
	public PanelStyleEditor(Widget parent, Panel panel, PanelEditorSession session ) : base(parent)
	{
		ArgumentNullException.ThrowIfNull( panel );
		ArgumentNullException.ThrowIfNull( session );

		Layout = Layout.Column();
		Layout.Spacing = 8f;
		this.panel = panel;
		this.session = session;
	}

	public void Rebuild()
	{
		Layout.Clear( true );

		var controlLayout = Layout.AddRow();

		addRuleButton = new( "Add Rule", "add" );
		addRuleButton.Clicked += AddBlockPopup;
		controlLayout.Add( addRuleButton, 0 );

		//Layout.AddSpacingCell( 4f );

		var scroller = new ScrollArea( this );
		scroller.Canvas = new Widget();
		scroller.Canvas.Layout = Layout.Column();
		editor = scroller.Canvas.Layout.AddColumn();
		editor.Spacing = 4f;
		scroller.Canvas.Layout.AddStretchCell();
		Layout.Add( scroller );

		foreach (var styleBlock in session.GetStyleBlocks(panel) )
		{
			PanelStyleBlockEditor block = new( panel, styleBlock, session, PanelInspector.RawEdit );
			editor.Add( block );
		}
	}

	[EditorEvent.Frame]
	public void Frame()
	{
		if(SetContentHash( HashCode.Combine( panel, PanelInspector.RawEdit ), 0.1f ))
		{
			Rebuild();
		}
	}
	private void AddBlockPopup()
	{
		StyleRuleWizard.OpenWithSession( panel, Rebuild );
	}
}
