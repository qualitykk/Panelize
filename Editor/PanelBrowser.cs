using Sandbox;
using Sandbox.Diagnostics;
using Sandbox.UI;
using System.Linq;

namespace Panelize;

public class PanelBrowser : Widget
{
	public float EntrySize { get; set; } = 200f;
	public int Columns { get; set; } = 2;
	GridLayout grid;
    public PanelBrowser()
    {
		WindowTitle = "Panel List";
		Layout = Layout.Column();

		grid = Layout.AddLayout(Layout.Grid(), 10);
		grid.Spacing = 5f;
		BuildList();

    }

	private void BuildList()
	{
		grid.Clear( true );
		var builderTypes = EditorTypeLibrary.GetTypes<PanelBuilder>();

		int x = 0;
		int y = 0;
		foreach (var type in builderTypes)
		{
			if ( type.IsAbstract || type.IsGenericType ) continue;

			if (x > Columns)
			{
				x = 0;
				y++;
			}

			PanelPreviewWidget preview = new PanelPreviewWidget( type.Create<PanelBuilder>() )
			{
				FixedWidth = EntrySize,
				FixedHeight = EntrySize
			};
			grid.AddCell(x, y, preview);
			x++;
		}
	}
}

public class PanelPreviewWidget : Widget
{
	PanelBuilder builder;
	Color bgColor = Color.Gray;
	//Drag drag;
    public PanelPreviewWidget( PanelBuilder builder )
    {
		Assert.NotNull( builder );

		this.builder = builder;
		IsDraggable = true;
    }
	protected override void OnPaint()
	{
		Rect size = LocalRect;
		Paint.SetBrushAndPen( bgColor );
		Paint.DrawRect( size.Shrink(5f), 1f );

		Paint.SetPen( Color.White );
		Paint.SetFont( "Roboto", 24f, 800 );
		Paint.DrawText( size, builder.Title );
	}

	bool dragging = false;
	protected override void OnDragStart()
	{
		if ( dragging ) return;

		dragging = true;
		Log.Info( $"Drag Start!" );
	}

	protected override void OnMouseReleased( MouseEvent e )
	{
		if(e.LeftMouseButton && dragging )
		{
			Vector2 pos = e.WindowPosition;
			
			Log.Info( $"Drag stop!" );
			PanelEditorSession.Current.EditorWidget?.OnBuilderDrop( builder, e );
			dragging = false;
		}
	}
}
