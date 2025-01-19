using Sandbox;
using Sandbox.UI;
using System;
using System.Linq;
using System.Reflection;

namespace Panelize;

public class PanelPropertyEditor : Widget
{
	Panel panel;
	PanelEditorSession session;
	PropertySheet sheet;
	public PanelPropertyEditor( Widget parent, Panel panel, PanelEditorSession session ) : base( parent )
	{
		ArgumentNullException.ThrowIfNull( panel );
		Layout = Layout.Column();

		this.panel = panel;
		this.session = session;

		Rebuild();
	}

	public void Rebuild()
	{
		Layout.Clear( true );

		sheet?.Clear( true );
		sheet = new();
		sheet.PropertyOrder = ( prop ) => prop.DisplayName;

		var scroller = Layout.Add(new ScrollArea( this ));
		scroller.Canvas = new Widget();
		scroller.Canvas.Layout = Layout.Column();
		scroller.Canvas.Layout.Add( sheet );
		scroller.Canvas.Layout.AddStretchCell(1);

		BuildGenericProperties( panel );
		sheet.Rebuild();
	}

	[EditorEvent.Frame]
	public void Frame()
	{
		if ( SetContentHash( HashCode.Combine( panel ), 0.1f ) )
		{
			Rebuild();
		}
	}

	
	private void BuildGenericProperties( Panel panel)
	{
		Type panelType = panel.GetType();
		var properties = panelType.GetProperties( BindingFlags.Instance | BindingFlags.Public )
							.Select( prop => BuildProperty(prop, panel) )
							.OrderBy( prop => prop.Name );

		foreach ( SerializedProperty prop in properties )
		{
			if ( !CanEdit(prop)) continue;
			sheet.AddProperty( prop );
		}
	}

	private CustomSerializedProperty BuildProperty(PropertyInfo prop, Panel panel)
	{
		var sp = new CustomSerializedProperty( prop, panel );
		sp.CustomGroup = prop.DeclaringType.Name;
		sp.OnSetValue += (val) => PropertyEdited(panel, prop, val);
		return sp;
	}

	private void PropertyEdited( Panel panel, PropertyInfo prop, object val )
	{
		//Log.Info( $"Property edited: {panel} [{prop.Name} = {val}]" );
		var state = session.GetPanelState( panel );
		state.SetProperty( prop.Name, val );
		session.SetPanelState( panel, state );
	}

	private bool CanEdit(SerializedProperty prop)
	{
		// Ignore these properties because
		// a) we already have an editor for them elsewhere or
		// b) there is no reason for a user to edit them in this tool.
		string[] ignoredProperties = [
			"Id",
			"Classes",
			"Scene",
			"ChildContent",
			"PseudoClass",
			"ElementName"
		];

		return prop.IsEditable && !ignoredProperties.Contains( prop.Name );
	}
}
