using Sandbox.Diagnostics;
using Sandbox.UI;
using Label = Sandbox.UI.Label;

namespace Panelize;

public static class PanelSerializer
{
	const string TAB = "    ";
	public static string SerializeSession(PanelEditorSession session, bool serializeStyle = false, string panelName = "")
	{
		Assert.NotNull( session );
		var panel = session.SelectedPanel;
		Assert.NotNull( panel );

		string content = @"
@using System;
@using Sandbox.UI;

";

		if(serializeStyle)
		{
			content += "<style>\n";
			content += SerializeStyle( session, panelName );
			content += "</style>\n\n";
		}

		else
		{
			content += "@attribute [StyleSheet]\n\n";
		}

		content += "<root>\n";

		if ( panel.HasChildren )
		{
			foreach ( var child in panel.Children )
			{
				content += SerializeContent( child, session, 1 );
			}
		}

		content += "</root>";
		return content;
	}
	public static string SerializeContent(Panel panel, PanelEditorSession session, int indent)
	{
		PanelState state = session.GetPanelState( panel );
		string content = AddIndented("", $"<{panel.ElementName}", indent);
		if ( !string.IsNullOrEmpty( panel.Id ) )
			content += $" id=\"{panel.Id}\"";

		if ( !string.IsNullOrEmpty( panel.Classes ) )
			content += $" class=\"{panel.Classes}\"";

		if(state != null && state.Properties.Count > 0)
		{
			foreach((string key, object value) in state.Properties)
			{
				content += $" {key}=\"{value}\"";
			}
		}

		content += ">\n";

		if(panel is Label label)
		{
			content = AddLine(content, label.Text.Trim(), indent + 1);
		}
		else if ( panel.HasChildren )
		{
			foreach ( var child in panel.Children )
			{
				content += SerializeContent( child, session, indent + 1 );
			}
		}

		content = AddLine(content, $"</{panel.ElementName}>", indent);
		return content;
	}

	private static string AddLine(string content, string value, int indent)
	{
		return AddIndented(content, value, indent) + "\n";
	}
	private static string AddIndented(string content, string value, int indent)
	{
		string indentString = "";
		for ( int i = 0; i < indent; i++ )
		{
			indentString += TAB;
		}
		return content + indentString + value;
	}

	public static string SerializeStyle(PanelEditorSession session, string panelName = "")
	{
		string style = "";
		foreach ( var sheet in session.SelectedPanelSheets )
		{
			foreach ( var node in sheet.Nodes ) 
			{
				style += string.Join( ' ', node.SelectorStrings ) + " {\n";

				var properties = node.Styles.GetMember<Dictionary<string, IStyleBlock.StyleProperty>>( "RawValues" );
				foreach((string key, var prop) in properties)
				{
					if ( !prop.IsValid ) continue;

					style = AddLine(style, $"{prop.Name}: {prop.Value};", 1);
				}

				style += "}\n";
			}
		}

		if ( !string.IsNullOrEmpty( panelName ) )
		{
			string oldName = session.SelectedPanel.ElementName;
			if ( string.IsNullOrEmpty( oldName ) )
				oldName = session.SelectedPanel.GetType().Name;

			style = style.Replace( oldName, panelName, StringComparison.OrdinalIgnoreCase );
		}
		return style;
	}
}
