using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Label = Editor.Label;

namespace Panelize;

/// <summary>
/// Display a singular style property for editing.
/// </summary>
public class PanelStyleRowEditor : Widget
{
	Panel panel;
	StyleBlock block;
	IStyleBlock.StyleProperty entry;
	PanelEditorSession session;
	PropertyInfo entryProperty;
	public PanelStyleRowEditor(Panel panel, IStyleBlock block, IStyleBlock.StyleProperty entry, PanelEditorSession session, bool rawEdit = false)
	{
		this.panel = panel;
		this.block = (StyleBlock)block;
		this.entry = entry;
		this.session = session;

		entryProperty = PanelUtils.GetGeneratedProperty( entry.Name );
		if(entryProperty != null && !entryProperty.CanRead)
		{
			Destroy();
			return;
		}

		Layout = Layout.Row();

		Rebuild(rawEdit);
	}

	public void Rebuild(bool rawEdit = false)
	{
		Layout.Clear( true );

		Label name = new( $"{entry.Name}:" )
		{
			Alignment = TextFlag.RightCenter,
		};
		name.ToolTip = entryProperty?.Name ?? entry.Name;
		name.Color = entry.IsValid ? Theme.Blue : Theme.White;

		foreach ( var activeBlock in session.GetStyleBlocks(panel) )
		{
			if ( activeBlock == block )
			{
				break;
			}

			foreach ( var activeEntry in activeBlock.GetRawValues() )
			{
				if ( entry.Name == activeEntry.Name )
				{
					name.SetStyles( "text-decoration: line-through; " );
					break;
				}
			}
		}
		Layout.Add( name, 0 );

		Layout.AddSpacingCell( 4f );

		Widget editor = PanelUtils.CreateEditor( block, entryProperty, entry, panel, rawEdit || entryProperty == null || !entry.IsValid );
		Layout.Add( editor, 1 );

		Layout.AddStretchCell();
	}

	protected override void OnContextMenu( ContextMenuEvent e )
	{
		ContextMenu menu = new();
		menu.AddOption( $"Delete {entry.Name}", "delete", DeleteRow );
		menu.OpenAtCursor();
	}

	private void DeleteRow()
	{
		//block.SetRawValue( entry.Name, "" );
		block.SetRawValue(entry.Name, entry.OriginalValue);
		if(block is StyleBlock styleBlock)
		{
			 styleBlock.Styles.GetMember<Dictionary<string, IStyleBlock.StyleProperty>>( "RawValues" ).Remove(entry.Name);
		}
		panel.StateHasChanged();
		Destroy();
	}
}
