using System;
using System.Linq;
using System.Reflection;
using Sandbox;
using Sandbox.UI;

namespace Panelize;
public class PropertySheetRow : Widget
{
	public Widget Control { get; private set; }

	public static PropertySheetRow Create( SerializedProperty property )
	{
		Widget editor = default;

		if(EditorTypeLibrary.TryGetType(property.PropertyType, out _))
		{
			try
			{
				editor = ControlWidget.Create( property );
			}
			catch ( Exception e )
			{
				Log.Warning( e, $"Error creating controlwidget for {property.Name}" );
			}
		}

		if ( !editor.IsValid() )
		{
			editor = CanEditAttribute.CreateEditorFor( property.PropertyType );
		}

		if ( !editor.IsValid() ) return null;

		var row = new PropertySheetRow( property );
		row.Build( editor );

		return row;
	}
	public static PropertySheetRow Create( PropertyInfo info, object target )
	{
		CustomSerializedProperty property = new( info, target );
		return Create( property );
	}
	public SerializedProperty Property { get; private set; }
	public PropertySheetRow( SerializedProperty property )
	{
		Property = property;
		FocusMode = FocusMode.Click;
	}

	public void Build( Widget controlWidget, bool hasLabel = true, bool isExpanded = false )
	{
		if ( Property is null )
			return;

		var gridLayout = Layout.Grid();

		gridLayout.HorizontalSpacing = 0;
		gridLayout.Margin = new Margin( 0, 0, 4, 0 );
		Layout = gridLayout;

		Control = controlWidget;
		ToolTip = $"<font>{Property.Description ?? Property.DisplayName}</font>";
		HorizontalSizeMode = SizeMode.CanShrink;

		Control.HorizontalSizeMode = SizeMode.Flexible;

		/*
		if ( EditorUtility.Prefabs.GetVariables( property.Parent ) is not null )
		{
			gridLayout.AddCell( 0, 1, new VariableButton( property, controlWidget ), 1, 1, TextFlag.LeftTop );
		}

		if ( property.IsNullable )
		{
			gridLayout.AddCell( 1, 1, new PropertyButton( property, controlWidget ), 1, 1, TextFlag.LeftTop );
			controlWidget.Enabled = !property.IsNull;
		}
		*/

		if ( hasLabel )
		{
			var label = new PropertySheetPropertyLabel( Property );

			label.ContentMargins = isExpanded ? new( 0, 0, 0, 4 ) : new( 0, 0, 4, 0 );
			gridLayout.AddCell( 2, 1, label, xSpan: (isExpanded ? 2 : 1), alignment: TextFlag.LeftTop );
			gridLayout.AddCell( 3 - (isExpanded ? 1 : 0), 1 + (isExpanded ? 1 : 0), controlWidget, alignment: TextFlag.LeftTop );
		}
		else
		{
			gridLayout.AddCell( 2, 1, controlWidget, xSpan: 2, alignment: TextFlag.LeftTop );
		}

		gridLayout.SetColumnStretch( 0, 0, 0, 1 );
		gridLayout.SetMinimumColumnWidth( 0, 0 );
		gridLayout.SetMinimumColumnWidth( 1, 0 );
		gridLayout.SetMinimumColumnWidth( 2, 140 );
	}
}

file class PropertySheetPropertyLabel : PropertySheetLabel
{
	private SerializedProperty Property { get; }
	private Drag _drag;

	public PropertySheetPropertyLabel( SerializedProperty property )
	{
		Property = property;
		Text = Property.DisplayName;

		IsDraggable = false;
	}

	protected override void OnDragStart()
	{
		base.OnDragStart();

		_drag = new Drag( this )
		{
			Data = { Object = Property, Text = Property.As.String }
		};

		_drag.Execute();
	}

	protected override void OnPaint()
	{
		base.OnPaint();

		Paint.Pen = Theme.ControlText.WithAlpha( Paint.HasMouseOver ? 1.0f : 0.6f );
		Paint.DrawText( LocalRect.Shrink( 8, 4 ), Property.DisplayName, TextFlag.LeftTop );

		if ( !IsDraggable ) return;

		var isDragging = _drag.IsValid();
		if ( isDragging )
		{
			Paint.ClearPen();
			Paint.SetBrush( Theme.Pink.WithAlpha( 0.3f ) );
			Paint.DrawRect( ContentRect, 3f );
		}
	}
}
