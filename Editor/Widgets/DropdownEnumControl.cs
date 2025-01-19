using Editor;
using Sandbox;
using Sandbox.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panelize;

public class DropdownEnumControl<T> : EnumControl<T> where T : struct, Enum
{
	public class MenuOption : Widget
	{
		public T Value { get; set; }
		DropdownEnumControl<T> editor;
		public MenuOption( DropdownEnumControl<T> control, T value, string title, string icon = "", string description = "")
		{
			Layout = Layout.Row();
			Layout.Margin = 8f;
			Layout.Spacing = 4f;
			editor = control;
			Value = value;
			
			if(!string.IsNullOrEmpty(icon))
			{
				IconButton iconWidget = new( icon );
				iconWidget.Background = Color.Transparent;
				iconWidget.TransparentForMouseEvents = true;
				iconWidget.IconSize = 18f;
				Layout.Add( iconWidget );
			}

			Layout textColumn = Layout.AddColumn();
			Label text = new( title );
			//text.SetStyles( "font-size: 12px; font-weight: bold; font-family: Poppins; color: white;" );
			text.SetStyles( "font-weight: bold; font-family: Poppins; color: white;" );
			textColumn.Add( text );

			ToolTip = description;
		}
		public bool IsSelected()
		{
			return editor.Value.Equals(Value);
		}
		protected override void OnPaint()
		{
			if ( !Paint.HasMouseOver && !IsSelected() )
				return;

			Paint.SetBrushAndPen( Theme.Blue.WithAlpha( IsSelected() ? 0.3f : 0.1f ) );
			Rect rect = LocalRect;
			rect = rect.Shrink( 2f );
			Paint.DrawRect( in rect, 2f );
		}
	}

	private PopupWidget menu;
	public DropdownEnumControl(T defaultValue = default) : base(defaultValue)
	{
		Cursor = CursorShape.Finger;
		Layout = Layout.Row();
		MinimumWidth = 200f;
		MinimumHeight = 20f;
	}

	protected override void OnPaint()
	{
		Color color = Paint.HasMouseOver ? Theme.Blue : Theme.ControlText;
		if ( ReadOnly )
			color = color.WithAlpha( 0.5f );

		Paint.ClearPen();

		Color backgroundColor = "#201F21";
		Paint.SetBrush( backgroundColor );
		Paint.DrawRect( LocalRect );

		Rect rect = LocalRect.Shrink( 8, 0 );
		DisplayInfo display = GetValueDisplay( Value );

		if(!string.IsNullOrEmpty(display.Icon))
		{
			Paint.SetPen(color.WithAlpha(0.5f) );
			Rect iconRect = Paint.DrawIcon( rect, display.Icon, 18f, TextFlag.LeftCenter );
			rect.Left += iconRect.Width + 8f;
		}

		Paint.SetPen( color );
		Paint.SetFont( "Poppins", weight: 600 );
		Paint.DrawText( rect, display.Name ?? Value.ToString() ?? "Unset", TextFlag.LeftCenter );
		Paint.DrawIcon( rect, "arrow_drop_down", 17f, TextFlag.RightCenter );
	}
	protected override void OnMouseClick( MouseEvent e )
	{
		if ( e.LeftMouseButton && !menu.IsValid() )
		{
			OpenMenu();
		}
	}

	protected override void OnDoubleClick( MouseEvent e )
	{
	}

	private void OpenMenu()
	{
		menu = new PopupWidget( null );
		menu.Layout = Layout.Column();
		menu.MinimumWidth = ScreenRect.Width;
		menu.MaximumWidth = ScreenRect.Width;
		ScrollArea scrollArea = menu.Layout.Add( new ScrollArea( this ), 1 );
		scrollArea.Canvas = new Widget( scrollArea )
		{
			Layout = Layout.Column(),
			VerticalSizeMode = SizeMode.Expand | SizeMode.CanGrow
		};

		foreach ( (T value, DisplayInfo display) in GetValueDisplays() )
		{
			if ( !display.Browsable )
			{
				continue;
			}

			string title = display.Name ?? value.ToString();
			string icon = display.Icon;

			MenuOption option = new( this, value, title, icon );
			option.MouseLeftPress = () => {
				SetValue( value );
				menu.Update();
				menu.Close();
			};

			scrollArea.Canvas.Layout.Add( option );
		}

		menu.Position = ScreenRect.BottomLeft;
		menu.Visible = true;
		menu.AdjustSize();
		menu.ConstrainToScreen();
		menu.OnPaintOverride = PaintMenuBackground;
	}

	private bool PaintMenuBackground()
	{
		Paint.SetBrushAndPen( Theme.ControlBackground );
		Rect rect = Paint.LocalRect;
		Paint.DrawRect( rect, 0f );
		return true;
	}
}
