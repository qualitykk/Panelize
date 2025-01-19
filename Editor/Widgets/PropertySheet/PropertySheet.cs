using Sandbox;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Panelize;

public class PropertySheet : GridLayout
{
	public record Entry( SerializedProperty Property, Widget Control );
	public int Rows { get; private set; }
	public List<Entry> Entries { get; set; } = new();
	public Func<SerializedProperty, object> PropertyOrder { get; set; }
	public Func<string, object> GroupOrder { get; set; }
	Dictionary<string, PropertySheetGroup> groupWidgets = new();
	public PropertySheet()
	{
		Margin = new( 16, 8, 16, 8 );
		HorizontalSpacing = 10f;
		VerticalSpacing = 2f;
		SetColumnStretch( 1, 2 );
		SetMinimumColumnWidth( 0, 120 );
	}

	public void Rebuild()
	{
		groupWidgets.Clear();
		Clear(true);

		var orderProperties = Entries.ToArray().OrderBy( (e) => PropertyOrder?.Invoke(e.Property) );
		string[] groups = orderProperties.Select(p => p.Property.GroupName).Distinct().OrderBy(g => GroupOrder?.Invoke(g)).ToArray();

		foreach ( var group in groups )
		{
			AddGroup(group);
		}

		foreach((var prop, var control) in orderProperties)
		{
			CreatePropertyRow( prop, control );
		}
	}
	private void CreatePropertyRow(SerializedProperty prop, Widget control)
	{
		PropertySheetRow row = null;
		if ( control != null )
		{
			row = new( prop );
			row.Build( control );
		}
		else
		{
			row = PropertySheetRow.Create( prop );
		}

		if ( !row.IsValid() ) return;

		PropertySheetGroup group = GetOrCreateGroup( prop.GroupName );
		if ( group == null )
		{
			SetColumnStretch( 0 );
			AddCell( 0, Rows++, row );
		}
		else
		{
			group?.AddProperty( row );
		}
	}
	public void AddProperty(SerializedProperty prop, Widget control = null )
	{
		Entries.Add( new( prop, control ) );
	}
	public PropertySheetGroup GetOrCreateGroup(string name)
	{
		if ( string.IsNullOrEmpty( name ) ) return null;

		if ( !groupWidgets.TryGetValue( name, out PropertySheetGroup group ) )
		{
			group = AddGroup( name );
		}

		return group;
	}
	public PropertySheetGroup AddGroup(string name)
	{
		PropertySheetGroup group = new( name );
		AddCell( 0, Rows++, group, 2 );
		groupWidgets.Add(name, group);

		return group;
	}
	public void AddBuilder(Func<Widget> builder, string label = "", string group = "")
	{
		ArgumentNullException.ThrowIfNull( builder );

		if(!string.IsNullOrEmpty(group))
		{
			PropertySheetGroup groupWidget = GetOrCreateGroup( group );
			groupWidget.AddBuilder( builder, label );
		}
		else
		{
			Widget widget = builder();
			if(string.IsNullOrEmpty(label))
			{
				AddCell( 0, Rows++, widget, 2 );
			}
			else
			{
				PropertySheetLabel labelWidget = new()
				{ 
					Text = label 
				};

				AddCell(0, Rows, labelWidget );
				AddCell(1, Rows++, widget );
			}
		}
	}
}

internal class PropertySheetLabel : Widget
{
	public string Text { get; set; }
	public PropertySheetLabel()
	{
		MinimumHeight = Theme.RowHeight;
		MinimumWidth = 140f;
		HorizontalSizeMode = SizeMode.Flexible;
	}

	protected override void OnPaint()
	{
		base.OnPaint();

		Paint.Pen = Theme.ControlText.WithAlpha( Paint.HasMouseOver ? 1.0f : 0.6f );
		Paint.DrawText( LocalRect.Shrink( 8, 4 ), Text, TextFlag.LeftTop );
	}
}
