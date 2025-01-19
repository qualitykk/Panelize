using Editor;
using Sandbox;
using Sandbox.UI;

namespace Panelize;

public class Properties : Widget
{
	public static object SelectedObject { get; set; }
	private static object currentSelectedObject { get; set; }
	Layout editor;
	public Properties(MainWindow mainWindow) : base(mainWindow)
	{
		WindowTitle = "Properties";
		Layout = Layout.Column();
		editor = Layout.AddRow( 1 );

		SelectedObject = null;
		currentSelectedObject = null;
	}

	[EditorEvent.Frame]
	public void Frame()
	{
		//Log.Info( $"Select {SelectedObject}" );

		if ( SelectedObject != currentSelectedObject )
		{
			Select( SelectedObject );
		}
	}

	private void Select(object obj )
	{
		currentSelectedObject = obj;

		editor.Clear(true);

		var so = obj.GetSerialized();
		Widget inspector = null;
		if(obj is Panel p )
		{
			inspector = new PanelInspector( this, p, PanelEditorSession.Current );
		}
		else
		{
			inspector = InspectorWidget.Create( so );
		}

		if ( inspector.IsValid() )
		{
			editor.Add( inspector, 1 );
		}
		else
		{
			// Try CanEdit still..
			// todo: Everything that should be an inspector should be an InspectorWidget
			inspector = CanEditAttribute.CreateEditorForObject( obj );

			if ( inspector.IsValid() )
			{
				editor.Add( inspector, 1 );
			}
			else
			{
				try
				{
					var sheet = new ControlSheet();
					sheet.AddObject( so );

					var scroller = new ScrollArea( this );
					scroller.Canvas = new Widget();
					scroller.Canvas.Layout = Layout.Column();
					scroller.Canvas.VerticalSizeMode = SizeMode.CanGrow;
					scroller.Canvas.HorizontalSizeMode = SizeMode.Flexible;

					scroller.Canvas.Layout.Add( sheet );
					scroller.Canvas.Layout.AddStretchCell();

					editor.Add( scroller );
				}
				catch { }

			}
		}
	}
}
