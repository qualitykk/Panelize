using Sandbox.UI;
using Label = Editor.Label;

namespace Panelize;

public class PanelInspector : Widget
{
	public static bool RawEdit { get; set; }
	public Panel Panel { get; set; }
	public PanelEditorSession Session { get; set; }
	PanelPropertyEditor propEditor;
	PanelStyleEditor styleEditor;
	public PanelInspector( Widget parent, Panel target, PanelEditorSession session ) : base( parent )
	{
		if ( !target.IsValid() )
			return;

		ArgumentNullException.ThrowIfNull( session );

		Panel = target;
		Session = session;
		Layout = Layout.Column();

		Rebuild();
	}

	void Rebuild()
	{
		Layout.Clear(true);

		PanelHeader header = new( this, this, Panel );
		Layout.Add( header );

		var tabs = new TabWidget( this );

		propEditor = new( this, Panel, Session );
		styleEditor = new( this, Panel, Session );
		tabs.AddPage( "Properties", "edit", propEditor );
		tabs.AddPage( "Styles", "style", styleEditor );
		tabs.StateCookie = "PanelEditorPanelInspector";

		Layout.Add( tabs, 1 );
	}
}

file class PanelHeader : Widget
{
	Panel panel;
	PanelInspector inspector;
	Layout Control;
	Layout Editor;
	public PanelHeader(Widget parent, PanelInspector inspector, Panel target) : base(parent)
	{
		ArgumentNullException.ThrowIfNull( target );
		panel = target;
		this.inspector = inspector;

		Layout = Layout.Row();
		MinimumHeight = 120f;

		Editor = Layout.AddColumn();
		Editor.Margin = new( 4f );
		Editor.Spacing = 2f;

		Layout.AddSpacingCell( 10f );

		Control = Layout.AddColumn();
		Control.Margin = new(2f, 4f);
		Control.Spacing = 4f;
		Control.Alignment = TextFlag.CenterTop;

		Build();
	}

	void Build()
	{
		Control.Clear(true);
		Editor.Clear(true);
		Type panelType = panel.GetType();

		// Add panel controls

		Checkbox visibleCheckbox = new( this, panel.IsVisibleSelf, "visibility", "visibility_off", 32f )
		{
			ToolTip = "Show Panel"
		};
		visibleCheckbox.OnEdited += ( value ) => panel.Style.Display = value ? DisplayMode.Flex : DisplayMode.None;
		Control.Add( visibleCheckbox );

		Checkbox rawValuesCheckbox = new( this, PanelInspector.RawEdit, "text_fields", "tune", 32f )
		{
			ToolTip = "Edit Raw Values"
		};
		rawValuesCheckbox.OnEdited += ( value ) => PanelInspector.RawEdit = value;
		Control.Add( rawValuesCheckbox );

		Checkbox simulateHoverCheckbox = new( this, panel.HasHovered, "highlight_alt", "check_box_outline_blank", 32f )
		{
			ToolTip = "Simulate Hover (Broken)"
		};
		simulateHoverCheckbox.OnEdited += ( value ) =>
		{
			if ( value )
				panel.PseudoClass |= PseudoClass.Hover;
			else
				panel.PseudoClass &= ~PseudoClass.Hover;
		};
		Control.Add(simulateHoverCheckbox );

		// Add well-defined properties manually.

		LineEdit idEditor = new( this )
		{
			Text = panel.Id
		};
		idEditor.EditingFinished += () => panel.Id = idEditor.Text;
		MakeRow( "Id" ).Add( idEditor );

		TagEdit classEditor = new( this )
		{
			ValueTags = panel.Classes
		};
		classEditor.OnEdited += () => panel.Classes = classEditor.ValueTags;
		MakeRow( "Class" ).Add( classEditor );

		LineEdit typeEditor = new( this )
		{
			Text = panel.ElementName
		};
		typeEditor.EditingFinished += () => panel.ElementName = typeEditor.Text;
		MakeRow( "Type" ).Add( typeEditor );
	}

	private Layout MakeRow(string label)
	{
		var row = Editor.AddRow();
		row.Add( new Label( label ) );
		row.AddSpacingCell( 8f );

		return row;
	}
}
