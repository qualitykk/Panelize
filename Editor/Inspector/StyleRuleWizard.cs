using Sandbox.UI;
using System.Linq;
using Button = Editor.Button;
using Label = Editor.Label;

namespace Panelize;

public class StyleRuleWizard : Window
{
	public static StyleRuleWizard OpenWithSession(Panel panel, Action onFinished = null, StyleBlock block = null )
	{
		StyleSheet sheet = PanelEditorSession.Current?.SelectedPanelSheets?.FirstOrDefault();
		if(sheet == null)
			sheet = panel.AllStyleSheets.FirstOrDefault();

		return new StyleRuleWizard( panel, sheet, onFinished, block );
	}
	public Action OnFinished;
	Panel panel;
	string rule;
	List<string> parts = new();
	LineEdit rulePreview;
	Button addButton;
	Layout editor;
	StyleSheet sheet;
	StyleBlock block;
	public StyleRuleWizard( Panel panel, StyleSheet sheet, Action onFinished = null, StyleBlock block = null )
	{
		ArgumentNullException.ThrowIfNull( panel );
		ArgumentNullException.ThrowIfNull( sheet );
		WindowTitle = "Add Rule...";

		this.panel = panel;
		this.sheet = sheet;
		this.block = block ?? new()
		{
			Styles = new()
		};
		OnFinished = onFinished;

		MinimumWidth = 720f;
		MinimumHeight = 720f;

		Canvas = new ScrollArea( this );
		Canvas.Layout = Layout.Column();
		editor = Canvas.Layout;
		editor.Margin = new( 8f );

		Rebuild();
		Show();
	}

	public void Rebuild()
	{
		editor.Clear( true );

		editor.Add( new Label.Title( "Add Style Rule" ) );

		rulePreview = new()
		{
			PlaceholderText = "Style Rule"
		};
		rulePreview.EditingFinished += UpdateRuleFromText;
		editor.Add( rulePreview );

		editor.Add(CreateSection( "Panel" ), 0);
		editor.Add( CreateElementEditor( panel ) );
		if(!string.IsNullOrEmpty(panel.Id))
		{
			editor.Add(CreateIdEditor( panel.Id ), 0);
		}

		foreach(string c in panel.Class)
		{
			editor.Add(CreateClassEditor(c), 0);
		}

		Panel parent = panel.Parent;
		while ( parent.IsValid() )
		{
			if ( parent is RootPanel ) break;

			editor.Add( CreateSection( $"Parent ({parent.ElementName})" ) );
			editor.Add( CreateElementEditor( parent ));

			if(!string.IsNullOrEmpty(parent.Id))
				editor.Add( CreateIdEditor( parent.Id) );

			foreach (var c in parent.Class)
			{
				editor.Add( CreateClassEditor( c ), 0 );
			}

			parent = parent.Parent;
		}

		editor.Add( CreateSection( "Events" ) );
		editor.Add( CreateRule( ":hover", "Hover" ), 0 );
		editor.Add( CreateRule( ":active", "Active" ), 0 );
		editor.Add( CreateRule( ":intro", "Intro" ), 0 );
		editor.Add( CreateRule( ":outro", "Outro" ), 0 );

		editor.Add( CreateSection( "Layout" ) );
		editor.Add( CreateRule( ":empty", "No Children" ) );
		editor.Add( CreateRule( ":first-child", "Is First Child" ) );
		editor.Add( CreateRule( ":last-child", "Is Last Child" ) );
		editor.Add( CreateRule( ":only-child", "Is Only Child" ) );

		addButton = new( "Add", "add" )
		{
			Enabled = false
		};
		addButton.Clicked += Finish;
		editor.AddStretchCell();
		editor.Add( addButton );

		AdjustSize();
	}

	private void Finish()
	{
		block.SetSelector( rule );
		sheet.Nodes.Add( block );
		PanelUtils.DirtyStyles(panel);

		OnFinished?.Invoke();
		Destroy();
	}

	private void UpdateRuleFromText()
	{
		rule = rulePreview.Text;
		parts = rule.Split( ' ' ).ToList();

		UpdateControls();
	}

	private void UpdateRuleFromParts()
	{
		rule = string.Join( ' ', parts );
		rulePreview.Text = rule;

		UpdateControls();
	}

	private void UpdateControls()
	{
		addButton.Enabled = parts.Count > 0;
	}

	private void AddRule( string value )
	{
		if(!parts.Contains( value ) )
		{
			parts.Add( value );
			UpdateRuleFromParts();
		}
	}

	private void RemoveRule( string value )
	{
		if(parts.Contains(value))
		{
			parts.Remove( value );
			UpdateRuleFromParts();
		}
	}

	private Widget CreateSection(string text)
	{
		Label header = new Label( text );
		header.SetStyles( "font-size: 18px; font-family: Roboto; font-weight: 600;" );
		return header;
	}
	private StyleRuleRow CreateRule(string rule, string label)
	{
		StyleRuleRow row = new( rule, label );
		row.RuleSelected += ( value ) =>
		{
			if ( value )
			{
				AddRule( rule );
			}
			else
			{
				RemoveRule( rule );
			}
		};

		return row;
	}
	private Widget CreateIdEditor(string value)
	{
		StyleRuleRow row = CreateRule( $"#{value}", $"Id {value}" );
		return row;
	}

	private Widget CreateClassEditor(string value)
	{
		string rule = $".{value}";
		StyleRuleRow row = CreateRule( rule, $"Class {value}" );
		return row;
	}

	private Widget CreateElementEditor(Panel panel)
	{
		string rule = panel.ElementName;
		StyleRuleRow row = CreateRule( rule, $"Element {rule}" );
		return row;
	}
}

public class StyleRuleRow : Widget
{
	public Action<bool> RuleSelected;
	string text;
	string rule;

	public StyleRuleRow(string value, string label) 
	{
		text = label;
		rule = value;

		Layout = Layout.Row();
		Rebuild();
	}
	protected override void OnPaint()
	{
		
	}
	public void Rebuild()
	{
		Layout.Clear( true );
		Label label = new( text )
		{
			ToolTip = rule
		};
		Layout.Add( label, 1 );

		Checkbox control = new( this );
		control.OnEdited += OnRuleSelected;
		Layout.Add( control, 0 );
	}

	private void OnRuleSelected(bool value)
	{
		RuleSelected?.Invoke( value );
	}
}
