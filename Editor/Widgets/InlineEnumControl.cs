using Sandbox;
using Sandbox.Physics;
using Sandbox.UI;
using System;
using static Editor.ProjectSettingPages.CollisionMatrixWidget;
using System.Runtime.CompilerServices;
using Button = Editor.Button;
using System.Collections.Generic;
using System.Reflection;
using Sandbox.Diagnostics;
using System.Linq;

namespace Panelize;

public class InlineEnumControl<T> : EnumControl<T> where T : struct, Enum
{
	public class EnumOption : Widget
	{
		InlineEnumControl<T> control;
		public string Icon { get; set; }
		public string Text { get; set; }
		public T Value { get; }
		public EnumOption( InlineEnumControl<T> parent, T value ) : base( parent )
		{
			ArgumentNullException.ThrowIfNull( parent );
			control = parent;
			Value = value;

			if(!ReadOnly)
			{
				Cursor = CursorShape.Finger;
			}
		}
		protected override void OnPaint()
		{
			base.OnPaint();

			var current = control.Value;

			Paint.ClearPen();
			Paint.Antialiasing = false;
			var rect = LocalRect.Shrink( 0, 0, 2, 2 );
			bool mouseOver = Paint.HasMouseOver && !ReadOnly;

			if ( current.Equals(Value) )
			{
				Paint.SetBrush( Theme.Blue.WithAlpha( mouseOver ? 0.4f : 0.3f ) );
				Paint.ClearPen();
				Paint.DrawRect( rect, 2 );

				Paint.SetPen( Theme.Blue );
				Paint.DrawIcon( rect, Icon, 14 );
			} 
			else 
			{
				if ( ReadOnly )
				{
					Paint.SetBrush( Theme.WindowBackground.WithAlpha( 0.3f ) );
					Paint.ClearPen();
					Paint.DrawRect( rect, 2 );

					Paint.SetPen( Theme.WindowBackground );
					Paint.DrawIcon( rect, Icon, 14 );
					return;
				}
				else
				{
					Paint.SetBrush( Theme.Grey.Darken( mouseOver ? 0.4f : 0.5f ) );
					Paint.ClearPen();
					Paint.DrawRect( rect, 2 );

					Paint.SetPen( Theme.Grey );
					Paint.DrawIcon( rect, Icon, 14 );
				}
			}
		}
		protected override void OnMouseClick( MouseEvent e )
		{
			if ( ReadOnly ) return;

			control.SetValue( Value );
		}
	}
	public float CellSize { get; set; } = 24f;
	public float CellSpacing { get; set; } = 8f;
	public int CellColumns { get; set; } = 3;
	protected GridLayout Editor { get; set; }
	public InlineEnumControl( T defaultValue = default ) : base( defaultValue )
	{
		Editor = Layout.Grid();
		Editor.Margin = 8;
		Editor.Spacing = CellSpacing;
		Layout = Editor;
	}
	public void BuildOptions()
	{
		Editor.Clear( true );
		int row = 0;
		int column = 0;

		foreach ((T value, DisplayInfo display) in GetValueDisplays() )
		{
			if( column >= CellColumns && CellColumns > 0)
			{
				row++;
				column = 0;
			}

			string icon = display.Icon;

			var option = new EnumOption( this, value ) 
			{ 
				Icon = icon,
				ToolTip = display.Name,
				FixedSize = CellSize
			};


			Editor.AddCell( column, row, option );
			column++;
		}
	}
}
