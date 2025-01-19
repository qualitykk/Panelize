using Editor.NodeEditor;
using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Button = Editor.Button;
using Label = Editor.Label;

namespace Panelize;
/// <summary>
/// Display a CSS rule block
/// </summary>
public class PanelStyleBlockEditor : Widget
{
	Panel panel;
	IStyleBlock block;
	PanelEditorSession session;
	bool rawEdit;
	public PanelStyleBlockEditor(Panel panel, IStyleBlock block, PanelEditorSession session, bool rawEdit = false)
	{
		//MinimumHeight = 100f;

		Layout = Layout.Column();
		Layout.Margin = 4;
		Layout.Spacing = 4;

		this.panel = panel;
		this.block = block;
		this.session = session;
		this.rawEdit = rawEdit;

		Rebuild();
	}

	public void Rebuild()
	{
		Layout.Clear(true);

		PanelStyleBlockHeader header = new( this, panel, block );
		Layout.Add( header );

		var entryColumn = Layout.AddColumn();
		entryColumn.Margin = new( 8f, 0f, 0f, 0f);
		entryColumn.Spacing = 2f;
		foreach(var entry in block.GetRawValues())
		{
			PanelStyleRowEditor row = new( panel, block, entry, session, rawEdit );
			if(row.IsValid())
				entryColumn.Add( row );
		}
	}
	protected override void OnPaint()
	{
		base.OnPaint();

		Paint.ClearPen();
		Paint.SetBrush( Theme.WidgetBackground.Lighten(0.4f) );
		Paint.DrawRect( LocalRect );
	}
}

file class PanelStyleBlockHeader : Widget
{
	private record StyleOption(string Path, PropertyInfo Prop, bool HasGroup = false);

	Label text;
	Button deleteRuleButton;
	Button editRuleButton;
	Button addStyleButton;
	PanelStyleBlockEditor editor;
	Panel panel;
	IStyleBlock block;
	public PanelStyleBlockHeader( PanelStyleBlockEditor editor, Panel panel, IStyleBlock block )
	{
		this.editor = editor;
		this.panel = panel;
		this.block = block;

		Layout = Layout.Row();
		Rebuild();
	}

	public void Rebuild()
	{
		Layout.Clear( true );

		text = new( string.Join( ", ", block.SelectorStrings.Select( x => $"<span style=\"color: {Theme.Yellow.Hex}\">{x}</span>" ) ) );
		Layout.Add( text, 1 );

		deleteRuleButton = new( "Delete", "remove" );
		deleteRuleButton.Clicked += DeleteRule;
		Layout.Add( deleteRuleButton );

		editRuleButton = new( "Edit", "edit" );
		editRuleButton.Clicked += EditRule;
		Layout.Add( editRuleButton );

		addStyleButton = new( "Add Style", "add" );
		addStyleButton.Clicked += AddStylePopup;
		Layout.Add( addStyleButton, 0 );
	}

	private void DeleteRule()
	{
		if ( block is not StyleBlock activeBlock ) return;

		foreach(var sheet in panel.AllStyleSheets)
		{
			if(sheet.Nodes.Contains( activeBlock ) )
			{
				sheet.Nodes.Remove( activeBlock );
			}
		}

		PanelUtils.DirtyStyles( panel );
		editor.Destroy();
	}

	private void EditRule()
	{
		throw new NotImplementedException();
	}

	private void AddStylePopup()
	{
		ContextMenu menu = new( this );
		menu.Layout = Layout.Column();

		menu.AddLineEdit( "Filter",
			placeholder: "Filter style...",
			onChange: (text) => BuildStyleOptions(menu, text) );

		BuildStyleOptions( menu );

		menu.OpenAtCursor();
	}

	private void BuildStyleOptions(Menu menu, string filter = "")
	{
		menu.RemoveMenus();
		menu.RemoveOptions();

		List<StyleOption> options = new();
		foreach ( var prop in PanelUtils.GetStyleProperties() )
		{
			if ( !prop.Name.Contains( filter, StringComparison.OrdinalIgnoreCase ) )
				continue;

			if ( !ShowProperty( prop.Name ) )
				continue;

			string group = GetGroup( prop.Name );
			if(!string.IsNullOrEmpty(group))
			{
				options.Add( new( $"{group}/{prop.Name.ToTitleCase()}", prop, true));
			}
			else
			{
				options.Add( new( prop.Name.ToTitleCase(), prop));
			}
		}

		menu.AddOptions( options.OrderBy( o => o.Path + (o.HasGroup ? 10000 : 0) )
			, (o) => o.Path, ( o ) => AddStyle( o.Prop ), reduce: false );
		menu.AdjustSize();
		menu.Update();
	}

	private void AddStyle(PropertyInfo prop)
	{
		string property = PanelUtils.GetStyleProperty( prop );
		// prop.GetValue(Styles.Default).ToString()
		block.SetRawValue( property, PanelUtils.GetStyleDefaultValue(prop) );
		
		editor.Rebuild();
	}

	private string GetGroup( string property )
	{
		Dictionary<string, string> propertyGroups = new()
		{
			{ "Width", "Size" },
			{ "MinWidth", "Size" },
			{ "MaxWidth", "Size" },
			{ "Height", "Size" },
			{ "MinHeight", "Size" },
			{ "MaxHeight", "Size" },
			{ "AspectRatio", "Size" },

			{ "WordBreak", "Text" },
			{ "WordSpacing", "Text" },
			{ "LetterSpacing", "Text" },

			{ "RowGap", "Flex" },
			{ "ColumnGap", "Flex" },

			{ "JustifyContent", "Alignment" },

			{ "Left", "Position" },
			{ "Top", "Position" },
			{ "Right", "Position" },
			{ "Bottom", "Position" },
			{ "Position", "Position" },
		};

		Dictionary<string, string> nameStartGroups = new()
		{
			{ "Background", "Background" },
			{ "Mask", "Mask" },
			{ "Filter", "Filter" },
			{ "Backdrop", "Backdrop" },
			{ "Sound", "Sound" },
			{ "Transform", "Transform" },
			{ "Text", "Text" },
			{ "Align", "Alignment" },
			{ "Font", "Font" },
			{ "Flex", "Flex" },
			{ "Border", "Border" },
			{ "Animation", "Animation" },
			{ "Overflow", "Overflow" },
			{ "Margin", "Margin" },
			{ "Padding", "Padding" },
		};

		bool groupSet = false;
		foreach ( (string name, string group) in propertyGroups )
		{
			if ( groupSet ) break;

			if ( property == name )
			{
				return group;
			}
		}

		foreach ( (string group, string value) in nameStartGroups )
		{
			if ( groupSet ) break;

			if ( property.StartsWith( group ) )
			{
				return value;
			}
		}


		return "";
	}

	private bool ShowProperty(string property)
	{
		string[] ignoredProperties = [
			"Display",
			"Content"
		];

		return !ignoredProperties.Contains( property );
	}
}
